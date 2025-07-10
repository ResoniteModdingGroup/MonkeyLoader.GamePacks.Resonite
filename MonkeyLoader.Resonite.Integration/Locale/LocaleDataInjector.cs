using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Meta;
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
    internal sealed class LocaleDataInjector
        : ResoniteAsyncEventSourceMonkey<LocaleDataInjector, LocaleLoadingEvent>
    {
        internal static async Task LoadLocalesAsync(Elements.Assets.LocaleResource localeResource, IEnumerable<string> localeCodes)
        {
            foreach (var localeCode in localeCodes)
            {
                var eventData = new LocaleLoadingEvent(localeResource, localeCode, localeCode == LocaleExtensions.FallbackLocaleCode);

                await DispatchAsync(eventData);
            }
        }

        [HarmonyTranspiler]
        [HarmonyPatch(MethodType.Async)]
        private static IEnumerable<CodeInstruction> LoadTargetVariantMoveNextTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var onLoadStateChangeMethod = AccessTools.Method(typeof(Asset), nameof(Asset.OnLoadStateChanged));

            foreach (var instruction in instructions)
            {
                if (instruction.Calls(onLoadStateChangeMethod))
                    yield return new CodeInstruction(OpCodes.Pop);
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
            localeCodes.AddUnique(LocaleExtensions.FallbackLocaleCode);

            await LoadLocalesAsync(__instance.Data, localeCodes);

            if (__state)
                __instance.OnLoadStateChanged();
        }

        [HarmonyPrefix]
        private static void LoadTargetVariantPrefix(LocaleResource __instance, out bool __state)
            => __state = __instance.Data != null;
    }
}