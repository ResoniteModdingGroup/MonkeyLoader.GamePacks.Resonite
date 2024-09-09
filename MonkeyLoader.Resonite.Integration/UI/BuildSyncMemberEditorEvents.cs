using FrooxEngine.UIX;
using FrooxEngine;
using System;
using MonkeyLoader.Resonite.Events;
using System.Reflection;
using Elements.Assets;
using MonkeyLoader.Events;
using System.Collections.Generic;
using MonkeyLoader.Patching;
using System.Linq;

namespace MonkeyLoader.Resonite.UI
{
    /// <summary>
    /// Represents the base class for the events fired during construction of a <see cref="MemberEditor"/>
    /// </summary>
    [DispatchableBaseEvent]
    public class BuildSyncMemberEditorEvent : CancelableBuildUIEvent
    {
        public ISyncMember Member { get; }

        public string Name { get; }

        public FieldInfo FieldInfo { get; }

        public float? LabelSize { get; }

        internal BuildSyncMemberEditorEvent(ISyncMember member, string name, FieldInfo fieldInfo, UIBuilder ui, float? labelSize)
            : base(ui)
        {
            Member = member;
            Name = name;
            FieldInfo = fieldInfo;
            LabelSize = labelSize;
        }
    }

    public sealed class BuildSyncArrayEditorEvent : BuildSyncMemberEditorEvent
    {
        public new ISyncArray Member { get; }
        internal BuildSyncArrayEditorEvent(ISyncArray array, string name, FieldInfo fieldInfo, UIBuilder ui, float labelSize)
            : base(array, name, fieldInfo, ui, labelSize)
        {
            Member = array;
        }
    }

    class TestThing : ResoniteCancelableEventHandlerMonkey<TestThing, BuildSyncArrayEditorEvent>
    {
        public override int Priority => HarmonyLib.Priority.First;

        public override bool SkipCanceled => true;

        protected override bool AppliesTo(BuildSyncArrayEditorEvent eventData) => true;
        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => Enumerable.Empty<IFeaturePatch>();
        protected override void Handle(BuildSyncArrayEditorEvent eventData) 
        {
            Logger.Info(() => "Array build event");
        }
    }

    class TestThing2 : ResoniteCancelableEventHandlerMonkey<TestThing2, BuildSyncMemberEditorEvent>
    {
        public override int Priority => HarmonyLib.Priority.First;

        public override bool SkipCanceled => true;

        protected override bool AppliesTo(BuildSyncMemberEditorEvent eventData) => true;
        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => Enumerable.Empty<IFeaturePatch>();
        protected override void Handle(BuildSyncMemberEditorEvent eventData)
        {
            Logger.Info(() => "Member editor build event");
        }
    }
}