using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using MonkeyLoader.Events;
using MonkeyLoader.Patching;
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
    internal sealed class DefaultBuildMemberEditorHandlers : ResoniteMonkey<DefaultBuildMemberEditorHandlers>,
        ICancelableEventHandler<BuildArrayEditorEvent>, ICancelableEventHandler<BuildBagEditorEvent>,
        ICancelableEventHandler<BuildListEditorEvent>, ICancelableEventHandler<BuildPlaybackEditorEvent>,
        ICancelableEventHandler<BuildFieldEditorEvent>, ICancelableEventHandler<BuildObjectEditorEvent>
    {
        public int Priority => HarmonyLib.Priority.Normal;

        public bool SkipCanceled => true;

        void ICancelableEventHandler<BuildArrayEditorEvent>.Handle(BuildArrayEditorEvent eventData) => throw new NotImplementedException();

        public void Handle(BuildBagEditorEvent eventData)
        {
            BuildBag(eventData.Member, eventData.Name, eventData.FieldInfo, eventData.UI);

            eventData.Canceled = true;
        }

        public void Handle(BuildListEditorEvent eventData)
        {
            BuildList(eventData.Member, eventData.Name, eventData.FieldInfo, eventData.UI);

            eventData.Canceled = true;
        }

        public void Handle(BuildPlaybackEditorEvent eventData)
        {
            BuildPlayback(eventData.Member, eventData.Name, eventData.FieldInfo, eventData.UI, eventData.LabelSize!.Value);

            eventData.Canceled = true;
        }

        public void Handle(BuildFieldEditorEvent eventData)
        {
            BuildField(eventData.Member, eventData.Name, eventData.FieldInfo, eventData.UI, eventData.LabelSize!.Value);

            eventData.Canceled = true;
        }

        public void Handle(BuildObjectEditorEvent eventData)
        {
            BuildSyncObject(eventData.Member, eventData.Name, eventData.FieldInfo, eventData.UI, eventData.LabelSize!.Value);

            eventData.Canceled = true;
        }

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => [];

        protected override bool OnEngineReady()
        {
            Mod.RegisterEventHandler<BuildArrayEditorEvent>(this);
            Mod.RegisterEventHandler<BuildBagEditorEvent>(this);
            Mod.RegisterEventHandler<BuildListEditorEvent>(this);
            Mod.RegisterEventHandler<BuildPlaybackEditorEvent>(this);
            Mod.RegisterEventHandler<BuildFieldEditorEvent>(this);
            Mod.RegisterEventHandler<BuildObjectEditorEvent>(this);

            return base.OnEngineReady();
        }

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

        [DoesNotReturn]
        private static void ThrowNotImplemented()
            => throw new NotImplementedException();
    }
}