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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace MonkeyLoader.Resonite.UI
{
    [HarmonyPatchCategory(nameof(SyncMemberEditorBuilderInjector))]
    [HarmonyPatch(typeof(SyncMemberEditorBuilder), nameof(SyncMemberEditorBuilder.BuildArray))]
    internal sealed class SyncMemberEditorBuilderInjector : ResoniteMonkey<SyncMemberEditorBuilderInjector>,
        IEventSource<BuildSyncMemberEditorEvent>, IEventSource<BuildSyncArrayEditorEvent>
    {
        private static EventDispatching<BuildSyncMemberEditorEvent>? _buildMemberEditor;
        private static EventDispatching<BuildSyncArrayEditorEvent>? _buildArrayEditor;

        public override bool CanBeDisabled => true;

        /// <inheritdoc/>
        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => [];

        protected override bool OnLoaded()
        {
            Mod.RegisterEventSource<BuildSyncMemberEditorEvent>(this);
            Mod.RegisterEventSource<BuildSyncArrayEditorEvent>(this);

            return base.OnLoaded();
        }

        [HarmonyPrefix]
        private static bool BuildArrayPrefix(ISyncArray array, string name, FieldInfo fieldInfo, UIBuilder ui, float labelSize)
        {
            if (!Enabled)
                return true;

            ui.Panel().Slot.GetComponent<LayoutElement>();
            ui.Style.MinHeight = 24f;

            OnBuildMemberEditor(array, name, fieldInfo, ui, labelSize);
            OnBuildArray(array, name, fieldInfo, ui, labelSize);

            Slot slot = SyncMemberEditorBuilder.GenerateMemberField(array, name, ui, labelSize);
            ui.ForceNext = slot.AttachComponent<RectTransform>();
            LocaleString text = "(arrays currently not supported)";
            ui.Text(in text);
            ui.NestOut();

            return false;
        }

        private static void OnBuildArray(ISyncArray array, string name, FieldInfo fieldInfo, UIBuilder ui, float labelSize)
        {
            var root = ui.Root;

            var eventData = new BuildSyncArrayEditorEvent(array, name, fieldInfo, ui, labelSize);

            _buildArrayEditor?.Invoke(eventData);

            ui.NestInto(root);
        }

        private static void OnBuildMemberEditor(ISyncMember member, string name, FieldInfo fieldInfo, UIBuilder ui, float labelSize)
        {
            var root = ui.Root;

            var eventData = new BuildSyncMemberEditorEvent(member, name, fieldInfo, ui, labelSize);

            _buildMemberEditor?.Invoke(eventData);

            ui.NestInto(root);
        }

        event EventDispatching<BuildSyncMemberEditorEvent>? IEventSource<BuildSyncMemberEditorEvent>.Dispatching
        {
            add => _buildMemberEditor += value;
            remove => _buildMemberEditor -= value;
        }

        event EventDispatching<BuildSyncArrayEditorEvent>? IEventSource<BuildSyncArrayEditorEvent>.Dispatching
        {
            add => _buildArrayEditor += value;
            remove => _buildArrayEditor -= value;
        }
    }
}