using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Events;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.Locale
{
    [HarmonyPatchCategory(nameof(LocaleDataInjector))]
    [HarmonyPatch(typeof(LocaleResource), nameof(LocaleResource.LoadTargetVariant))]
    internal sealed class LocaleDataInjector : ResoniteMonkey<LocaleDataInjector>, IAsyncEventSource<LocaleLoadingEvent>
    {
        private static AsyncEventDispatching<LocaleLoadingEvent>? _localeLoading;

        /// <inheritdoc/>
        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => Enumerable.Empty<IFeaturePatch>();

        /// <inheritdoc/>
        protected override bool OnEngineReady()
        {
            Mod.RegisterEventSource(this);

            return base.OnEngineReady();
        }

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

            for (var i = 0; i < localeCodes.Count; ++i)
            {
                var eventData = new LocaleLoadingEvent(__instance.Data, localeCodes[i], i == localeCodes.Count - 1);

                await (_localeLoading?.Invoke(eventData) ?? Task.CompletedTask);
            }

            if (__state)
                __instance.OnLoadStateChanged();
        }

        [HarmonyPrefix]
        private static void LoadTargetVariantPrefix(LocaleResource __instance, ref bool __state)
            => __state = __instance.Data != null;

        /// <inheritdoc/>
        event AsyncEventDispatching<LocaleLoadingEvent>? IAsyncEventSource<LocaleLoadingEvent>.Dispatching
        {
            add => _localeLoading += value;
            remove => _localeLoading -= value;
        }
    }
}