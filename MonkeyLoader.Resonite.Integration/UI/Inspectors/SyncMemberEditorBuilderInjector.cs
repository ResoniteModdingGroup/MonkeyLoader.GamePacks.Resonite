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
    internal sealed class SyncMemberEditorBuilderInjector
        : ResoniteCancelableEventSourceMonkey<SyncMemberEditorBuilderInjector,
            BuildArrayEditorEvent, BuildBagEditorEvent, BuildListEditorEvent,
            BuildPlaybackEditorEvent, BuildFieldEditorEvent, BuildObjectEditorEvent>
    {
        public override bool CanBeDisabled => true;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SyncMemberEditorBuilder.BuildArray))]
        private static bool BuildArrayPrefix(ISyncArray array, string name, FieldInfo fieldInfo, UIBuilder ui, float labelSize)
        {
            if (!Enabled)
                return true;

            var eventData = new BuildArrayEditorEvent(array, name, fieldInfo, ui, labelSize);

            Dispatch(eventData);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SyncMemberEditorBuilder.BuildBag))]
        private static bool BuildBagPrefix(ISyncBag bag, string name, FieldInfo fieldInfo, UIBuilder ui)
        {
            if (!Enabled)
                return true;

            var eventData = new BuildBagEditorEvent(bag, name, fieldInfo, ui);

            Dispatch(eventData);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SyncMemberEditorBuilder.BuildField), [typeof(IField), typeof(string), typeof(FieldInfo), typeof(UIBuilder), typeof(float)])]
        private static bool BuildFieldPrefix(IField field, string name, FieldInfo fieldInfo, UIBuilder ui, float labelSize)
        {
            if (!Enabled)
                return true;

            var eventData = new BuildFieldEditorEvent(field, name, fieldInfo, ui, labelSize);

            Dispatch(eventData);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SyncMemberEditorBuilder.BuildList))]
        private static bool BuildListPrefix(ISyncList list, string name, FieldInfo fieldInfo, UIBuilder ui)
        {
            if (!Enabled)
                return true;

            var eventData = new BuildListEditorEvent(list, name, fieldInfo, ui);

            Dispatch(eventData);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SyncMemberEditorBuilder.BuildPlayback))]
        private static bool BuildPlaybackPrefix(SyncPlayback playback, string name, FieldInfo fieldInfo, UIBuilder ui, float labelSize)
        {
            if (!Enabled)
                return true;

            var eventData = new BuildPlaybackEditorEvent(playback, name, fieldInfo, ui, labelSize);

            Dispatch(eventData);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SyncMemberEditorBuilder.BuildSyncObject))]
        private static bool BuildSyncObjectPrefix(SyncObject syncObject, string name, FieldInfo fieldInfo, UIBuilder ui, float labelSize)
        {
            if (!Enabled)
                return true;

            var eventData = new BuildObjectEditorEvent(syncObject, name, fieldInfo, ui, labelSize);

            Dispatch(eventData);

            return false;
        }
    }
}