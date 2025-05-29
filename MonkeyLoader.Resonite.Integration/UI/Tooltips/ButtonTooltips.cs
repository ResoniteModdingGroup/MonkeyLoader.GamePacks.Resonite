using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.UI.Tooltips
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
            if (!Enabled
             || TooltipManager.HasTooltip(__instance)
             || __instance.RectTransform.FilterWorldElement() is not RectTransform buttonRect
             || buttonRect?.Canvas.Slot is not Slot tooltipParent)
                return;

            var canvasBounds = buttonRect.GetCanvasBounds();
            var canvasHitPoint = tooltipParent.GlobalPointToLocal(eventData.globalPoint);

            var localOffset = canvasBounds.Center.x_ + canvasBounds.Min._y - canvasHitPoint.xy;
            var offset = tooltipParent.LocalVectorToGlobal(localOffset.xy_) + (0.01f * tooltipParent.Backward);

            __instance.World.RunInSeconds(TooltipConfig.Instance.HoverTime, () =>
            {
                if (!TooltipManager.HasTooltip(__instance) && __instance.IsHovering)
                    TooltipManager.TryOpenTooltip(__instance, eventData, tooltipParent, in offset, 1f / buttonRect.Canvas.UnitScale);
            });
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Button.RunHoverLeave))]
        private static void RunHoverLeavePostfix(Button __instance)
            => TooltipManager.CloseTooltip(__instance);
    }
}