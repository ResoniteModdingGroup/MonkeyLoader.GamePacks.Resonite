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
using System.Xml.Linq;

namespace MonkeyLoader.Resonite.UI
{
    internal sealed class DefaultBuildSyncArrayEditorHandler
        : ResoniteCancelableEventHandlerMonkey<DefaultBuildSyncArrayEditorHandler, BuildSyncArrayEditorEvent>
    {
        public override int Priority => HarmonyLib.Priority.Normal;

        public override bool SkipCanceled => true;

        protected override bool AppliesTo(BuildSyncArrayEditorEvent eventData) => true;

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => [];

        protected override void Handle(BuildSyncArrayEditorEvent eventData)
        {
            var ui = eventData.UI;
            var array = eventData.Member;
            var name = eventData.Name;
            var labelSize = eventData.LabelSize!.Value;

            ui.Panel().Slot.GetComponent<LayoutElement>();
            ui.Style.MinHeight = 24f;

            Slot slot = SyncMemberEditorBuilder.GenerateMemberField(array, name, ui, labelSize);
            ui.ForceNext = slot.AttachComponent<RectTransform>();
            LocaleString text = "(arrays currently not supported)";
            ui.Text(in text);
            ui.NestOut();
        }
    }
}