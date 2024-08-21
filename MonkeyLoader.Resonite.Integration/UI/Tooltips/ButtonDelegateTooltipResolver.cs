using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.UI.Tooltips
{
    internal sealed class ButtonDelegateTooltipResolver : ResoniteCancelableEventHandlerMonkey<ButtonDelegateTooltipResolver, ResolveTooltipLabelEvent>
    {
        public override bool CanBeDisabled => true;
        public override int Priority => HarmonyLib.Priority.HigherThanNormal;
        public override bool SkipCanceled => true;

        protected override bool AppliesTo(ResolveTooltipLabelEvent eventData) => Enabled;

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => [];

        protected override void Handle(ResolveTooltipLabelEvent eventData)
        {
            ISyncDelegate pressed;
            Dictionary<string, object> arguments;

            if (eventData.Button.Pressed.Target is not null)
            {
                arguments = [];
                pressed = eventData.Button.Pressed;
            }
            else if (eventData.Button.Slot.GetComponent<ButtonRelayBase>() is ButtonRelayBase relay
             && relay.GetSyncMember(nameof(ButtonRelay<dummy>.ButtonPressed)) is ISyncDelegate relayPressed
             && relay.GetSyncMember(nameof(ButtonRelay<dummy>.Argument)) is IField relayArgument)
            {
                pressed = relayPressed;

                arguments = new() { ["RelayArgument"] = relayArgument is ISyncRef syncRef ? syncRef.Target.GetReferenceLabel() : relayArgument.BoxedValue };
            }
            else
            {
                return;
            }

            var targetType = pressed.Method.GetMethodInfo().DeclaringType;
            var localeKey = $"Tooltip.{targetType.Name}.{pressed.MethodName}";

            if (!localeKey.HasMessageInCurrent())
                return;

            arguments.Add("TargetType", targetType.Name);

            // Should always be an instance method for SyncDelegates, but who knows
            if (!pressed.IsStaticReference)
            {
                var target = ((IWorldElement)pressed.Method.Target).FindNearestParent<Worker>();

                foreach (var syncMemberName in WorkerInitializer.GetInitInfo(targetType).syncMemberNames)
                {
                    var member = target.GetSyncMember(syncMemberName);

                    if (member is ISyncRef syncRef)
                    {
                        arguments.Add(syncMemberName, syncRef.Target.GetReferenceLabel());
                        continue;
                    }

                    if (member is IField field)
                        arguments.Add(syncMemberName, field.BoxedValue);
                }
            }

            eventData.Label = localeKey.AsLocaleKey(arguments: arguments);
            Logger.Debug(eventData.Label.Value.content.Yield().Concat(eventData.Label.Value.arguments.Select(item => $"\"{item.Key}\" = \"{item.Value}\"")));
        }
    }
}