using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.UI.Inspectors
{
    [HarmonyPatch]
    [HarmonyPatchCategory(nameof(InspectorMemberActionsMenuInjector))]
    internal sealed class InspectorMemberActionsMenuInjector : ResoniteMonkey<InspectorMemberActionsMenuInjector>,
        IAsyncEventSource<InspectorMemberActionsMenuItemsGenerationEvent>
    {
        private static AccessTools.FieldRef<object, ButtonEventData> _accessButtonEventData = null!;
        private static AccessTools.FieldRef<object, InspectorMemberActions> _accessMemberActions = null!;
        private static AccessTools.FieldRef<object, ISyncMember> _accessTarget = null!;

        private static AsyncEventDispatching<InspectorMemberActionsMenuItemsGenerationEvent>? _inspectorMemberActionsMenuItemsGeneration;

        public override bool CanBeDisabled => true;

        protected override bool OnEngineReady()
        {
            Mod.RegisterEventSource(this);

            return base.OnEngineReady();
        }

        protected override bool OnShutdown(bool applicationExiting)
        {
            if (!applicationExiting)
                Mod.UnregisterEventSource(this);

            return base.OnShutdown(applicationExiting);
        }

        private static bool MatchesRequiredMethod(MethodInfo method)
            => method.ReturnType == typeof(Task);

        private static bool MatchesRequiredType(Type innerType)
        {
            var fields = AccessTools.GetDeclaredFields(innerType).ToArray();

            return fields.Any(field => field.FieldType == typeof(InspectorMemberActions))
                && fields.Any(field => field.FieldType == typeof(ButtonEventData))
                && fields.Any(field => field.FieldType == typeof(ISyncMember))
                && AccessTools.FirstMethod(innerType, MatchesRequiredMethod) is not null;
        }

        [HarmonyPostfix]
        private static async Task PostfixAsync(Task __result, object __instance)
        {
            var memberActions = _accessMemberActions(__instance);
            var buttonEventData = _accessButtonEventData(__instance);
            var target = _accessTarget(__instance);

            await __result;

            var eventData = new InspectorMemberActionsMenuItemsGenerationEvent(
                memberActions, buttonEventData, target);

            await (_inspectorMemberActionsMenuItemsGeneration?.Invoke(eventData) ?? Task.CompletedTask);
        }

        private static MethodBase TargetMethod()
        {
            // The actual menu generation happens inside a lambda,
            // which gets turned into a compiler generated class.
            // We look for this based on the necessary fields and method.

            var generatedType = AccessTools.InnerTypes(typeof(InspectorMemberActions))
                .SingleOrDefault(MatchesRequiredType)
                ?? throw new InvalidOperationException("Found no suitable nested type in InspectorMemberActions!");

            var method = AccessTools.GetDeclaredMethods(generatedType)
                .SingleOrDefault(MatchesRequiredMethod)
                ?? throw new InvalidOperationException($"Found no suitable method on the nested type [{generatedType.CompactDescription()}] in InspectorMemberActions!");

            var fields = AccessTools.GetDeclaredFields(generatedType).ToArray();

            var memberActionsField = fields.Single(field => field.FieldType == typeof(InspectorMemberActions));
            var buttonEventDataField = fields.Single(field => field.FieldType == typeof(ButtonEventData));
            var targetField = fields.Single(field => field.FieldType == typeof(ISyncMember));

            _accessMemberActions = AccessTools.FieldRefAccess<object, InspectorMemberActions>(memberActionsField);
            _accessButtonEventData = AccessTools.FieldRefAccess<object, ButtonEventData>(buttonEventDataField);
            _accessTarget = AccessTools.FieldRefAccess<object, ISyncMember>(targetField);

            return method;
        }

        event AsyncEventDispatching<InspectorMemberActionsMenuItemsGenerationEvent>? IAsyncEventSource<InspectorMemberActionsMenuItemsGenerationEvent>.Dispatching
        {
            add => _inspectorMemberActionsMenuItemsGeneration += value;
            remove => _inspectorMemberActionsMenuItemsGeneration -= value;
        }
    }
}