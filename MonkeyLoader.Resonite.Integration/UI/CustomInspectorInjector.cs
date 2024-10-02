using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using MonkeyLoader.Events;
using MonkeyLoader.Patching;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MonkeyLoader.Resonite.UI
{
    [HarmonyPatchCategory(nameof(CustomInspectorInjector))]
    [HarmonyPatch(typeof(WorkerInspector), nameof(WorkerInspector.BuildUIForComponent))]
    internal sealed class CustomInspectorInjector : ResoniteMonkey<CustomInspectorInjector>,
        IEventSource<BuildInspectorHeaderEvent>, IEventSource<BuildInspectorBodyEvent>
    {
        private static EventDispatching<BuildInspectorBodyEvent>? _buildInspectorBody;
        private static EventDispatching<BuildInspectorHeaderEvent>? _buildInspectorHeader;

        public override bool CanBeDisabled => true;

        /// <inheritdoc/>
        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => [];

        protected override bool OnEngineReady()
        {
            Mod.RegisterEventSource<BuildInspectorHeaderEvent>(this);
            Mod.RegisterEventSource<BuildInspectorBodyEvent>(this);

            return base.OnEngineReady();
        }

        [HarmonyPrefix]
        private static bool BuildUIForComponentPrefix(WorkerInspector __instance, Worker worker,
            bool allowRemove, bool allowDuplicate, bool allowContainer, Predicate<ISyncMember> memberFilter)
        {
            if (!Enabled)
                return true;

            var ui = new UIBuilder(__instance.Slot);
            RadiantUI_Constants.SetupEditorStyle(ui);
            var vertical = ui.VerticalLayout(6f);

            if (worker is not Slot)
            {
                ui.Style.MinHeight = 32f;
                ui.HorizontalLayout(4f);
                ui.Style.MinHeight = 24f;
                ui.Style.FlexibleWidth = 1000f;

                OnBuildInspectorHeader(ui, __instance, worker, allowContainer, allowDuplicate, allowRemove, memberFilter);

                ui.NestInto(vertical.Slot);
            }

            if (worker is ICustomInspector customInspector)
            {
                ui.Style.MinHeight = 24f;
                customInspector.BuildInspectorUI(ui);
            }
            else
            {
                WorkerInspector.BuildInspectorUI(worker, ui, memberFilter);
            }

            OnBuildInspectorBody(ui, __instance, worker, allowContainer, allowDuplicate, allowRemove, memberFilter);

            ui.Style.MinHeight = 8f;
            ui.Panel();
            ui.NestOut();

            return false;
        }

        private static void OnBuildInspectorBody(UIBuilder ui, WorkerInspector inspector, Worker worker,
            bool allowContainer, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter)
        {
            var root = ui.Root;

            var eventData = new BuildInspectorBodyEvent(ui, inspector, worker, allowContainer, allowDuplicate, allowDestroy, memberFilter);

            _buildInspectorBody?.Invoke(eventData);

            ui.NestInto(root);
        }

        private static void OnBuildInspectorHeader(UIBuilder ui, WorkerInspector inspector, Worker worker,
            bool allowContainer, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter)
        {
            var root = ui.Root;

            var eventData = new BuildInspectorHeaderEvent(ui, inspector, worker, allowContainer, allowDuplicate, allowDestroy, memberFilter);

            _buildInspectorHeader?.Invoke(eventData);

            ui.NestInto(root);
        }

        event EventDispatching<BuildInspectorHeaderEvent>? IEventSource<BuildInspectorHeaderEvent>.Dispatching
        {
            add => _buildInspectorHeader += value;
            remove => _buildInspectorHeader -= value;
        }

        event EventDispatching<BuildInspectorBodyEvent>? IEventSource<BuildInspectorBodyEvent>.Dispatching
        {
            add => _buildInspectorBody += value;
            remove => _buildInspectorBody -= value;
        }
    }
}