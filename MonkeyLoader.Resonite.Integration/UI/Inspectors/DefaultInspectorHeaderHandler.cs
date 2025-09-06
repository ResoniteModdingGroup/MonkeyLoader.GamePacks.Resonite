using Elements.Core;
using FrooxEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.UI.Inspectors
{
    internal sealed class DefaultInspectorHeaderHandler
        : ConfiguredResoniteEventHandlerMonkey<DefaultInspectorHeaderHandler, DefaultInspectorHeaderConfig, BuildInspectorHeaderEvent>
    {
        public override int Priority => HarmonyLib.Priority.Normal;

        protected override void Handle(BuildInspectorHeaderEvent eventData)
        {
            var ui = eventData.UI;
            var worker = eventData.Worker;

            if (eventData.CreateWorkerNameButton)
            {
                LocaleString text = $"<b>{worker.GetType().GetNiceName()}</b>";

                var button = ui.ButtonRef(in text, eventData.Inspector.OnWorkerTypePressed, worker);
                button.Slot.AttachComponent<ReferenceProxySource>().Reference.Target = worker;
                button.Label.Color.Value = RadiantUI_Constants.LABEL_COLOR;

                ConfigSection.WorkerNameOffset.DriveFromVariable(button.Slot._orderOffset);

                eventData.WorkerNameButton = button;
            }

            ui.Style.FlexibleWidth = 0;
            ui.Style.MinWidth = 40;

            if (eventData.CreateOpenContainerButton)
            {
                var button = ui.ButtonRef(OfficialAssets.Graphics.Icons.Inspector.RootUp, RadiantUI_Constants.Sub.PURPLE, eventData.Inspector.OnOpenContainerPressed, worker);
                ConfigSection.OpenContainerOffset.DriveFromVariable(button.Slot._orderOffset);

                eventData.OpenContainerButton = button;
            }

            if (eventData.CreateDuplicateButton)
            {
                var button = ui.ButtonRef(OfficialAssets.Graphics.Icons.Inspector.Duplicate, RadiantUI_Constants.Sub.GREEN, eventData.Inspector.OnDuplicateComponentPressed, worker);
                ConfigSection.DuplicateOffset.DriveFromVariable(button.Slot._orderOffset);

                eventData.DuplicateButton = button;
            }

            if (eventData.CreateDestroyButton)
            {
                var button = ui.ButtonRef(OfficialAssets.Graphics.Icons.Inspector.Destroy, RadiantUI_Constants.Sub.RED, eventData.Inspector.OnRemoveComponentPressed, worker);
                ConfigSection.DestroyOffset.DriveFromVariable(button.Slot._orderOffset);

                eventData.DestroyButton = button;
            }
        }
    }
}