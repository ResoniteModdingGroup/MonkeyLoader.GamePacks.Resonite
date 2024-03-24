using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Zio;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// <para>
    /// Handles injecting the <see cref="Elements.Assets.LocaleData">locale data</see> provided by
    /// <see cref="Mod">mods</see> in their <see cref="Mod.FileSystem">FileSystem</see> or through
    /// <see cref="ILocaleDataProvider"/>s into the loading process of the <see cref="LocaleResource"/> asset.
    /// </para>
    /// <para>
    /// Data files are automatically loaded from any <see cref="Mod">mods</see> that provide them.<br/>
    /// <see cref="ILocaleDataProvider"/>s have to be <see cref="AddProvider">registered</see> with this class.
    /// They are queried <i>after</i> files.<br/>
    /// <b>Make sure to <see cref="RemoveProvider">remove</see> them during Shutdown.</b>
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// To be considered for loading, <see cref="Elements.Assets.LocaleData">locale data</see> files have
    /// to be in <i>any</i> folder called <c>Locale</c> <i>anywhere</i> in a mod's <see cref="Mod.ContentPaths">content files</see>.
    /// Multiple <c>Locale</c> folders with files for the same locale are no problem.
    /// </para>
    /// <para>
    /// <see cref="Elements.Assets.LocaleData">Locale data</see> files need to have their language code
    /// as their filename, for example: <c>de-at.json</c> for austrian german, or <c>de.json</c> for any german.<br/>
    /// The locale definitions are loaded with the exact locale code (de-at) coming first,
    /// falling back to the base locale code (de), and finally the universal fallback english (en).<br/>
    /// All <see cref="Elements.Core.LocaleHelper.AsLocaleKey(string, ValueTuple{string, object}[])">locale keys</see>
    /// will use the first definition encountered for them:
    /// A key that's the same for <c>de-at</c> and <c>de</c> would not have to be present in <c>de-at</c>,
    /// while a different one in <c>de-at</c> would take priority over the one in <c>de</c>.
    /// </para>
    /// <para>
    /// The json files need to be in the following format (for <c>en.json</c> here),
    /// although the <see cref="Mod.Id">ModId</see>-prefix is just convention:
    /// <code>
    /// {
    ///   "localeCode": "en",
    ///   "authors": [ "Mod", "Locale", "Author", "Names" ],
    ///   "messages": {
    ///     "ModId.KeyA": "A first message.",
    ///     "ModId.KeyB": "Better locale support!"
    ///   }
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    [HarmonyPatchCategory(nameof(LocaleDataInjector))]
    [HarmonyPatch(typeof(LocaleResource), nameof(LocaleResource.LoadTargetVariant))]
    public sealed class LocaleDataInjector : Monkey<LocaleDataInjector>
    {
        private static readonly SortedSet<ILocaleDataProvider> _localeDataProviders = new(LocaleDataProviderComparer);

        /// <summary>
        /// Gets an <see cref="IComparer{T}"/> that compares <see cref="ILocaleDataProvider"/>s
        /// based on their <see cref="ILocaleDataProvider.Priority">priority</see>.
        /// </summary>
        public static IComparer<ILocaleDataProvider> LocaleDataProviderComparer { get; } = new LocaleDataProviderComparerImpl();

        /// <summary>
        /// Gets all the <see cref="ILocaleDataProvider"/> which currently
        /// get queried during the loading of locale data, ordered by their
        /// <see cref="ILocaleDataProvider.Priority">priority</see>.
        /// </summary>
        public static IEnumerable<ILocaleDataProvider> LocaleDataProviders => _localeDataProviders.AsSafeEnumerable();

        /// <summary>
        /// Adds the given <see cref="ILocaleDataProvider"/> to the
        /// set of providers queried during the loading of locale data.<br/>
        /// <b>Make sure to <see cref="RemoveProvider">remove</see> it during Shutdown.</b>
        /// </summary>
        /// <param name="localeDataProvider">The provider to add.</param>
        /// <returns><c>true</c> if the provider was added; <c>false</c> if it was already present.</returns>
        public bool AddProvider(ILocaleDataProvider localeDataProvider)
            => _localeDataProviders.Add(localeDataProvider);

        /// <summary>
        /// Determines whether the set of providers queried
        /// during the loading of locale data contains the given one.
        /// </summary>
        /// <param name="localeDataProvider">The provider to locate.</param>
        /// <returns><c>true</c> if the provider is present; otherwise, <c>false</c>.</returns>
        public bool HasProvider(ILocaleDataProvider localeDataProvider)
            => _localeDataProviders.Contains(localeDataProvider);

        /// <summary>
        /// Removes the given <see cref="ILocaleDataProvider"/>
        /// from the set of providers queried during the loading of locale data.
        /// </summary>
        /// <param name="localeDataProvider">The provider to remove.</param>
        /// <returns><c>true</c> if the provider was removed; <c>false</c> if it could not be found.</returns>
        public bool RemoveProvider(ILocaleDataProvider localeDataProvider)
            => _localeDataProviders.Remove(localeDataProvider);

        /// <inheritdoc/>
        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => Enumerable.Empty<IFeaturePatch>();

        [HarmonyTranspiler]
        [HarmonyPatch(MethodType.Async)]
        private static IEnumerable<CodeInstruction> LoadTargetVariantMoveNextTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase targetMethod)
        {
            var onLoadStateChangeMethod = AccessTools.Method(typeof(Asset), nameof(Asset.OnLoadStateChanged));

            foreach (var instruction in instructions)
            {
                if (instruction.Calls(onLoadStateChangeMethod))
                    yield return new CodeInstruction(OpCodes.Nop);
                else
                    yield return instruction;
            }
        }

        [HarmonyPostfix]
        private static async Task LoadTargetVariantPostfixAsync(Task __result, LocaleResource __instance, LocaleVariantDescriptor variant, bool __state)
        {
            await __result;

            var localeCodes = new List<string>();
            localeCodes.AddUnique(variant.LocaleCode);
            localeCodes.AddUnique(Elements.Assets.LocaleResource.GetMainLanguage(variant.LocaleCode));
            localeCodes.AddUnique("en");

            foreach (var localeCode in localeCodes)
            {
                var searchPath = (new UPath("Locale") / $"{localeCode}.json").ToRelative().ToString();

                foreach (var mod in Mod.Loader.Mods)
                {
                    foreach (var localeFilePath in mod.ContentPaths.Where(path => path.ToString().EndsWith(searchPath, StringComparison.OrdinalIgnoreCase)))
                    {
                        try
                        {
                            using var localeFileStream = mod.FileSystem.OpenFile(localeFilePath, FileMode.Open, FileAccess.Read);

                            var localeData = await JsonSerializer.DeserializeAsync<Elements.Assets.LocaleData>(localeFileStream);

                            if (localeData is null)
                                continue;

                            if (!localeCode.Equals(localeData.LocaleCode, StringComparison.OrdinalIgnoreCase))
                                Warn(() => $"Detected locale data with wrong locale code from locale file! Wanted [{localeCode}] - got [{localeData.LocaleCode}] in file: {mod.Id}:/{localeFilePath}");

                            __instance.Data.LoadDataAdditively(localeData);
                        }
                        catch (Exception ex)
                        {
                            Warn(() => ex.Format($"Failed to deserialize file as LocaleData: {localeFilePath}"));
                        }
                    }
                }

                _localeDataProviders.Where(ldp => ldp.SupportsLocale(localeCode))
                    .TrySelect((ILocaleDataProvider ldp, [NotNullWhen(true)] out Elements.Assets.LocaleData? localeData) =>
                    {
                        try
                        {
                            localeData = ldp.GetLocaleData(localeCode);
                            return true;
                        }
                        catch (Exception ex)
                        {
                            localeData = null;
                            Warn(() => ex.Format($"Locale Data Provider threw an exception while getting locale data!"));

                            return false;
                        }
                    })
                    .Where(data =>
                    {
                        if (data is null)
                            return false;

                        if (!localeCode.Equals(data.LocaleCode, StringComparison.OrdinalIgnoreCase))
                        {
                            Warn(() => $"Detected locale data with wrong locale code from LocaleDataProvider! Wanted [{localeCode}] - got [{data.LocaleCode}]! Messages:");
                            Warn(data.Messages.Select(message => $"{message.Key}: {message.Value}"));
                        }

                        return true;
                    })
                    .Do(__instance.Data.LoadDataAdditively);
            }

            if (__state)
                __instance.OnLoadStateChanged();
        }

        [HarmonyPrefix]
        private static void LoadTargetVariantPrefix(LocaleResource __instance, ref bool __state)
            => __state = __instance.Data != null;

        private sealed class LocaleDataProviderComparerImpl : IComparer<ILocaleDataProvider>
        {
            public int Compare(ILocaleDataProvider x, ILocaleDataProvider y)
            {
                var priorityComparison = x.Priority.CompareTo(y.Priority);

                if (priorityComparison != 0)
                    return priorityComparison;

                return Comparer.DefaultInvariant.Compare(x, y);
            }
        }
    }
}