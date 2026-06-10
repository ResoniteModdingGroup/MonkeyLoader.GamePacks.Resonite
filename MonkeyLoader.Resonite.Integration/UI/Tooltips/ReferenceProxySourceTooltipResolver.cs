using Elements.Core;
using FrooxEngine;
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

            // Slots, Users, (User)Components, SyncObjects themselves
            if (proxySource.Reference.Target is Worker targetWorker)
            {
                label = $"Tooltip.{string.Join('.', GetAllNestedNames(targetWorker.WorkerType).Reverse())}";
                return true;
            }

            // Any synchronized data owned by Workers
            if (proxySource.Reference.Target is not SyncElement targetElement)
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

            label = (nesting is 0
                ? $"Tooltip.{string.Join('.', GetAllNestedNames(targetField.DeclaringType!).Reverse())}.{targetElement.Name}"
                : $"Tooltip.{string.Join('.', GetAllNestedNames(targetField.DeclaringType!).Reverse())}.{targetElement.Name}.{string.Join('.', Enumerable.Repeat("Item", nesting))}"
                ).AsModLocaleKey(Mod);

            return true;
        }

        protected override void Handle(ResolveTooltipLabelEvent eventData)
        {
            if (TryGetTooltipLabel(eventData.Button, out var label))
                eventData.Label = label;
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
    }
}