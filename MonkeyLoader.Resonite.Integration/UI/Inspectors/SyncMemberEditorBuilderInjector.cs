using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using MonkeyLoader.Events;
using MonkeyLoader.Patching;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MonkeyLoader.Resonite.UI.Inspectors
{
    [HarmonyPatch(typeof(SyncMemberEditorBuilder))]
    [HarmonyPatchCategory(nameof(SyncMemberEditorBuilderInjector))]
    internal sealed class SyncMemberEditorBuilderInjector : ResoniteMonkey<SyncMemberEditorBuilderInjector>,
        ICancelableEventSource<BuildArrayEditorEvent>, ICancelableEventSource<BuildBagEditorEvent>,
        ICancelableEventSource<BuildListEditorEvent>, ICancelableEventSource<BuildPlaybackEditorEvent>,
        ICancelableEventSource<BuildFieldEditorEvent>, ICancelableEventSource<BuildObjectEditorEvent>
    {
        private static CancelableEventDispatching<BuildArrayEditorEvent>? _buildArrayEditor;
        private static CancelableEventDispatching<BuildBagEditorEvent>? _buildBagEditor;
        private static CancelableEventDispatching<BuildFieldEditorEvent>? _buildFieldEditor;
        private static CancelableEventDispatching<BuildListEditorEvent>? _buildListEditor;
        private static CancelableEventDispatching<BuildObjectEditorEvent>? _buildObjectEditor;
        private static CancelableEventDispatching<BuildPlaybackEditorEvent>? _buildPlaybackEditor;

        public override bool CanBeDisabled => true;

        /// <inheritdoc/>
        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => [];

        protected override bool OnEngineReady()
        {
            Mod.RegisterEventSource<BuildArrayEditorEvent>(this);
            Mod.RegisterEventSource<BuildBagEditorEvent>(this);
            Mod.RegisterEventSource<BuildListEditorEvent>(this);
            Mod.RegisterEventSource<BuildPlaybackEditorEvent>(this);
            Mod.RegisterEventSource<BuildFieldEditorEvent>(this);
            Mod.RegisterEventSource<BuildObjectEditorEvent>(this);

            return base.OnEngineReady();
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SyncMemberEditorBuilder.BuildArray))]
        private static bool BuildArrayPrefix(ISyncArray array, string name, FieldInfo fieldInfo, UIBuilder ui, float labelSize)
        {
            if (!Enabled)
                return true;

            var eventData = new BuildArrayEditorEvent(array, name, fieldInfo, ui, labelSize);

            _buildArrayEditor?.Invoke(eventData);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SyncMemberEditorBuilder.BuildBag))]
        private static bool BuildBagPrefix(ISyncBag bag, string name, FieldInfo fieldInfo, UIBuilder ui)
        {
            if (!Enabled)
                return true;

            var eventData = new BuildBagEditorEvent(bag, name, fieldInfo, ui);

            _buildBagEditor?.Invoke(eventData);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SyncMemberEditorBuilder.BuildField), [typeof(IField), typeof(string), typeof(FieldInfo), typeof(UIBuilder), typeof(float)])]
        private static bool BuildFieldPrefix(IField field, string name, FieldInfo fieldInfo, UIBuilder ui, float labelSize)
        {
            if (!Enabled)
                return true;

            var eventData = new BuildFieldEditorEvent(field, name, fieldInfo, ui, labelSize);

            _buildFieldEditor?.Invoke(eventData);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SyncMemberEditorBuilder.BuildList))]
        private static bool BuildListPrefix(ISyncList list, string name, FieldInfo fieldInfo, UIBuilder ui)
        {
            if (!Enabled)
                return true;

            var eventData = new BuildListEditorEvent(list, name, fieldInfo, ui);

            _buildListEditor?.Invoke(eventData);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SyncMemberEditorBuilder.BuildPlayback))]
        private static bool BuildPlaybackPrefix(SyncPlayback playback, string name, FieldInfo fieldInfo, UIBuilder ui, float labelSize)
        {
            if (!Enabled)
                return true;

            var eventData = new BuildPlaybackEditorEvent(playback, name, fieldInfo, ui, labelSize);

            _buildPlaybackEditor?.Invoke(eventData);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SyncMemberEditorBuilder.BuildSyncObject))]
        private static bool BuildSyncObjectPrefix(SyncObject syncObject, string name, FieldInfo fieldInfo, UIBuilder ui, float labelSize)
        {
            if (!Enabled)
                return true;

            var eventData = new BuildObjectEditorEvent(syncObject, name, fieldInfo, ui, labelSize);

            _buildObjectEditor?.Invoke(eventData);

            return false;
        }

        event CancelableEventDispatching<BuildArrayEditorEvent>? ICancelableEventSource<BuildArrayEditorEvent>.Dispatching
        {
            add => _buildArrayEditor += value;
            remove => _buildArrayEditor -= value;
        }

        event CancelableEventDispatching<BuildBagEditorEvent>? ICancelableEventSource<BuildBagEditorEvent>.Dispatching
        {
            add => _buildBagEditor += value;
            remove => _buildBagEditor -= value;
        }

        event CancelableEventDispatching<BuildPlaybackEditorEvent>? ICancelableEventSource<BuildPlaybackEditorEvent>.Dispatching
        {
            add => _buildPlaybackEditor += value;
            remove => _buildPlaybackEditor -= value;
        }

        event CancelableEventDispatching<BuildListEditorEvent>? ICancelableEventSource<BuildListEditorEvent>.Dispatching
        {
            add => _buildListEditor += value;
            remove => _buildListEditor -= value;
        }

        event CancelableEventDispatching<BuildObjectEditorEvent>? ICancelableEventSource<BuildObjectEditorEvent>.Dispatching
        {
            add => _buildObjectEditor += value;
            remove => _buildObjectEditor -= value;
        }

        event CancelableEventDispatching<BuildFieldEditorEvent>? ICancelableEventSource<BuildFieldEditorEvent>.Dispatching
        {
            add => _buildFieldEditor += value;
            remove => _buildFieldEditor -= value;
        }
    }
}