using FrooxEngine;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLoader.Resonite.UI.Inspectors
{
    internal sealed class OpenWikiArticle : ResoniteInspectorMonkey<OpenWikiArticle, BuildInspectorHeaderEvent, Worker>
    {
        public override int Priority => HarmonyLib.Priority.HigherThanNormal;

        protected override void Handle(BuildInspectorHeaderEvent eventData)
        {
            var ui = eventData.UI;

            ui.PushStyle();
            ui.Style.FlexibleWidth = 0;
            ui.Style.MinWidth = 40;

            var workerName = eventData.Worker.WorkerType.Name;
            var tickIndex = workerName.IndexOf('`');
            workerName = tickIndex > 0 ? workerName[..tickIndex] : workerName;

            var button = ui.Button(OfficialAssets.Graphics.Badges.Mentor);
            button.Slot.AttachComponent<Hyperlink>().URL.Value = new Uri($"https://wiki.resonite.com/Component:{workerName}");

            ui.PopStyle();
        }
    }
}