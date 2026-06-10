using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace MonkeyLoader.Resonite.UI.Tooltips
{
    internal sealed class ReferenceProxySourceTooltipResolver : ResoniteCancelableEventHandlerMonkey<ReferenceProxySourceTooltipResolver, ResolveTooltipLabelEvent>
    {
        public override bool CanBeDisabled => true;

        public override int Priority => HarmonyLib.Priority.HigherThanNormal + 1;

        public override bool SkipCanceled => true;

        public static bool TryGetTooltipLabel(IButton button, [NotNullWhen(true)] out LocaleString? label)
        {
            label = null;

            if (button.Slot.GetComponent<ReferenceProxySource>() is not ReferenceProxySource proxySource)
                return false;

            var parentDevInterface = (button as Button)?.RectTransform?.Canvas.Slot.GetComponent<IDeveloperInterface>();

            switch (parentDevInterface)
            {
                case UserInspector:
                case SceneInspector:
                case WorkerInspector:
                    return TryGetInspectorTooltipLabel(proxySource.Reference.Target, out label);

                default:
                    label = Mod.GetLocaleString("Tooltip.ReferenceProxySource", "target", proxySource.Reference.Target.GetReferenceLabel());
                    return true;
            }
        }

        protected override void Handle(ResolveTooltipLabelEvent eventData)
        {
            if (!TryGetTooltipLabel(eventData.Button, out var label))
                return;

            if (!TooltipConfig.Instance.EnableDebugButtonData && !label.Value.HasMessageInCurrent())
                return;

            eventData.Label = label;

            if (TooltipConfig.Instance.EnableDebugButtonData)
                Logger.Debug(() => $"LocaleKey: {eventData.Label.Value.content}");
        }

        private static IEnumerable<string> GetAllNestedNames(Type type)
        {
            yield return type.Name;

            while (type.IsNested)
            {
                type = type.DeclaringType!;
                yield return type.Name;
            }
        }

        private static bool TryGetInspectorTooltipLabel(IWorldElement? target, [NotNullWhen(true)] out LocaleString? label)
        {
            label = null;

            if (target is null)
                return false;

            // Slot, Users, (User)Components, SyncObjects themselves
            if (target is Worker targetWorker)
            {
                label = $"Tooltip.{string.Join('.', GetAllNestedNames(targetWorker.WorkerType).Reverse())}".AsModLocaleKey(Mod);
                return true;
            }

            // Any synchronized data owned by Workers
            if (target is not SyncElement targetElement)
                return false;

            var nesting = 0;

            // FieldList are SyncElements with more SyncElements inside
            // We want to use the name of the outermost one
            // Not sure if there is any deeper nestings than one, but just to be sure...
            while (targetElement.Parent is SyncElement parentElement)
            {
                ++nesting;
                targetElement = parentElement;
            }

            // Slots, Users, (User)Components, SyncObjects
            if (targetElement.FindNearestParent<Worker>() is not Worker parentWorker)
                return false;

            if (parentWorker.GetSyncMemberFieldInfo(targetElement.Name) is not FieldInfo targetField)
                return false;

            var typeIdentity = string.Join('.', GetAllNestedNames(targetField.DeclaringType!).Reverse());

            label = (nesting is 0
                ? $"Tooltip.{typeIdentity}.{targetElement.Name}"
                : $"Tooltip.{typeIdentity}.{targetElement.Name}.{string.Join('.', Enumerable.Repeat("Item", nesting))}"
                ).AsModLocaleKey(Mod);

            return true;
        }
    }
}