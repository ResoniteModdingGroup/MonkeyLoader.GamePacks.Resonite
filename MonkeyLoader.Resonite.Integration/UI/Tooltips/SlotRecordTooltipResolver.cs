using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using System.Diagnostics.CodeAnalysis;

namespace MonkeyLoader.Resonite.UI.Tooltips
{
    internal sealed class SlotRecordTooltipResolver : ResoniteCancelableEventHandlerMonkey<SlotRecordTooltipResolver, ResolveTooltipLabelEvent>
    {
        public override bool CanBeDisabled => true;

        public override int Priority => HarmonyLib.Priority.HigherThanNormal + 2;

        public override bool SkipCanceled => true;

        public static bool TryGetTooltipLabel(IButton button, [NotNullWhen(true)] out LocaleString? label, out bool shouldCache)
        {
            label = null;
            shouldCache = true;

            if (button.Slot.GetComponent<SlotRecord>() is not SlotRecord slotRecord)
                return false;

            if (slotRecord.TargetSlot.Target is not Slot targetSlot)
                return false;

            var parentDevInterface = (button as Button)?.RectTransform?.Canvas.Slot.GetComponent<IDeveloperInterface>();

            switch (parentDevInterface)
            {
                case SceneInspector:
                case WorkerInspector:
                    label = Mod.GetLocaleString("Tooltip.SlotRecord.Inspector");
                    return true;

                default:
                    shouldCache = false;
                    label = Mod.GetLocaleString("Tooltip.SlotRecord.Generic", "target", targetSlot.GetReferenceLabel());
                    return true;
            }
        }

        protected override void Handle(ResolveTooltipLabelEvent eventData)
        {
            if (!TryGetTooltipLabel(eventData.Button, out var label, out var shouldCache))
                return;

            eventData.Label = label;
            eventData.ShouldCacheLabel = shouldCache;
        }
    }
}