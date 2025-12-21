using FrooxEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.UI.Facets
{
    internal sealed class FacetPresetLoadedHook : ResoniteEventSourceMonkey<FacetPresetLoadedHook, FacetPresetLoadedEvent>
    {
        public override bool CanBeDisabled => true;

        [HarmonyPatch(typeof(FacetPreset))]
        [HarmonyPatchCategory(nameof(FacetPresetLoadedHook))]
        private static class OnLoadingPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(nameof(FacetPreset.OnAttach))]
            private static void OnAttachPostfix(FacetPreset __instance)
            {
                // TemplateFacetPresets have their own fresh load events
                if (!Enabled || __instance is TemplateFacetPreset)
                    return;

                var eventData = new FacetPresetLoadedEvent(__instance, true);

                Dispatch(eventData);
            }

            [HarmonyPrefix]
            [HarmonyPatch(nameof(FacetPreset.OnLoading))]
            private static void OnLoadingPrefix(FacetPreset __instance, LoadControl control)
            {
                if (!Enabled)
                    return;

                var type = __instance.GetType();
                var oldVersion = control.GetTypeVersion(type);
                var isFreshLoad = oldVersion < __instance.Version || __instance.ForceUpgrade;

                // TemplateFacetPresets have their own fresh load events
                var templateFacetPreset = __instance as TemplateFacetPreset;
                if (isFreshLoad && templateFacetPreset is not null)
                    return;

                // Build is triggered through control.OnLoaded( ..., 100), when it's a fresh load
                // Add our event as the very last thing regardless of that
                control.OnLoaded(__instance, () =>
                {
                    var eventData = templateFacetPreset is not null
                        ? new TemplateFacetPresetLoadedEvent(templateFacetPreset, isFreshLoad)
                        : new FacetPresetLoadedEvent(__instance, isFreshLoad);

                    Dispatch(eventData);
                }, int.MaxValue);
            }
        }

        [HarmonyPatch]
        [HarmonyPatchCategory(nameof(FacetPresetLoadedHook))]
        private static class TemplateBuildFallbackPatch
        {
            [HarmonyPostfix]
            private static async Task BuildFallbackPostfixAsync(Task __result, TemplateFacetPreset __instance)
            {
                await __result;

                if (!Enabled)
                    return;

                var eventData = new TemplateFacetPresetFallbackBuiltEvent(__instance);

                Dispatch(eventData);
            }

            private static IEnumerable<MethodBase> TargetMethods()
            {
                var methods = FacetPresetHelper.TemplateFacetPresetTypes
                    .Select(type => AccessTools.Method(type, nameof(TemplateFacetPreset.BuildFallback)))
                    .Where(method => method is not null)
                    .Distinct();

                return methods;
            }
        }

        [HarmonyPatch]
        [HarmonyPatchCategory(nameof(FacetPresetLoadedHook))]
        private static class TemplateOnLoadedPatch
        {
            [HarmonyPostfix]
            private static async Task OnLoadedPostfixAsync(Task __result, TemplateFacetPreset __instance)
            {
                await __result;

                if (!Enabled)
                    return;

                var eventData = new TemplateFacetPresetTemplateLoadedEvent(__instance);

                Dispatch(eventData);
            }

            private static IEnumerable<MethodBase> TargetMethods()
            {
                var methods = FacetPresetHelper.TemplateFacetPresetTypes
                    .Select(type => AccessTools.Method(type, nameof(TemplateFacetPreset.OnLoaded)))
                    .Where(method => method is not null)
                    .Distinct();

                return methods;
            }
        }
    }
}