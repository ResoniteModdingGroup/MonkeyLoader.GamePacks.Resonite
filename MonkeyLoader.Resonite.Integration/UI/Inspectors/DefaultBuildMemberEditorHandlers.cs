using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.UI.Inspectors
{
    [HarmonyPatch(typeof(SyncMemberEditorBuilder))]
    [HarmonyPatchCategory(nameof(DefaultBuildMemberEditorHandlers))]
    internal sealed class DefaultBuildMemberEditorHandlers : ResoniteCancelableEventHandlerMonkey<DefaultBuildMemberEditorHandlers,
        BuildArrayEditorEvent, BuildBagEditorEvent, BuildListEditorEvent,
        BuildPlaybackEditorEvent, BuildFieldEditorEvent, BuildObjectEditorEvent>
    {
        public override int Priority => HarmonyLib.Priority.Normal;

        public override bool SkipCanceled => true;

        protected override void Handle(BuildArrayEditorEvent eventData)
        {
            BuildArray(eventData.Member, eventData.Name, eventData.FieldInfo, eventData.UI, eventData.LabelSize!.Value);

            eventData.Canceled = true;
        }

        protected override void Handle(BuildBagEditorEvent eventData)
        {
            BuildBag(eventData.Member, eventData.Name, eventData.FieldInfo, eventData.UI);

            eventData.Canceled = true;
        }

        protected override void Handle(BuildListEditorEvent eventData)
        {
            BuildList(eventData.Member, eventData.Name, eventData.FieldInfo, eventData.UI);

            eventData.Canceled = true;
        }

        protected override void Handle(BuildPlaybackEditorEvent eventData)
        {
            BuildPlayback(eventData.Member, eventData.Name, eventData.FieldInfo, eventData.UI, eventData.LabelSize!.Value);

            eventData.Canceled = true;
        }

        protected override void Handle(BuildFieldEditorEvent eventData)
        {
            BuildField(eventData.Member, eventData.Name, eventData.FieldInfo, eventData.UI, eventData.LabelSize!.Value);

            eventData.Canceled = true;
        }

        protected override void Handle(BuildObjectEditorEvent eventData)
        {
            BuildSyncObject(eventData.Member, eventData.Name, eventData.FieldInfo, eventData.UI, eventData.LabelSize!.Value);

            eventData.Canceled = true;
        }

#pragma warning disable IDE0060 // Remove unused parameter

        [HarmonyReversePatch]
        [HarmonyPatch(nameof(SyncMemberEditorBuilder.BuildArray))]
        private static void BuildArray(ISyncArray array, string name, FieldInfo fieldInfo, UIBuilder ui, float labelSize)
            => ThrowNotImplemented();

        [HarmonyReversePatch]
        [HarmonyPatch(nameof(SyncMemberEditorBuilder.BuildBag))]
        private static void BuildBag(ISyncBag bag, string name, FieldInfo fieldInfo, UIBuilder ui)
            => ThrowNotImplemented();

        [HarmonyReversePatch]
        [HarmonyPatch(nameof(SyncMemberEditorBuilder.BuildField), [typeof(IField), typeof(string), typeof(FieldInfo), typeof(UIBuilder), typeof(float)])]
        private static void BuildField(IField field, string name, FieldInfo fieldInfo, UIBuilder ui, float labelSize)
            => ThrowNotImplemented();

        [HarmonyReversePatch]
        [HarmonyPatch(nameof(SyncMemberEditorBuilder.BuildList))]
        private static void BuildList(ISyncList list, string name, FieldInfo fieldInfo, UIBuilder ui)
            => ThrowNotImplemented();

        [HarmonyReversePatch]
        [HarmonyPatch(nameof(SyncMemberEditorBuilder.BuildPlayback))]
        private static void BuildPlayback(SyncPlayback playback, string name, FieldInfo fieldInfo, UIBuilder ui, float labelSize)
            => ThrowNotImplemented();

        [HarmonyReversePatch]
        [HarmonyPatch(nameof(SyncMemberEditorBuilder.BuildSyncObject))]
        private static void BuildSyncObject(SyncObject syncObject, string name, FieldInfo fieldInfo, UIBuilder ui, float labelSize)
            => ThrowNotImplemented();

#pragma warning restore IDE0060 // Remove unused parameter

        [DoesNotReturn]
        private static void ThrowNotImplemented()
            => throw new NotImplementedException();
    }
}