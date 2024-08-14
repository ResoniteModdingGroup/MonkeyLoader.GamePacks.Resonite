using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.UI
{
    /// <summary>
    /// Adds tooltips to <see cref="Button"/>s,
    /// similar to how they were originally done for Neos by Psychpsyo.<br/>
    /// <see href="https://github.com/Psychpsyo/Tooltippery"/>
    /// </summary>
    [HarmonyPatch(typeof(Button))]
    [HarmonyPatchCategory(nameof(ButtonTooltips))]
    internal sealed class ButtonTooltips : ResoniteMonkey<ButtonTooltips>
    {
        public override bool CanBeDisabled => true;

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => [];

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Button.OnDispose))]
        private static void OnDisposePostfix(Button __instance)
            => TooltipManager.CloseTooltip(__instance);

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Button.RunHoverEnter))]
        private static void RunHoverEnterPostfix(Button __instance, ButtonEventData eventData)
        {
            if (!Enabled || __instance.Slot.GetComponentInParents<Canvas>()?.Slot is not Slot tooltipParent)
                return;

            TooltipManager.TryOpenTooltip(__instance, eventData, tooltipParent);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Button.RunHoverLeave))]
        private static void RunHoverLeavePostfix(Button __instance)
            => TooltipManager.CloseTooltip(__instance);
    }
}