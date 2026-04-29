using EnumerableToolkit;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLoader.Resonite.UI.Inspectors
{
    internal sealed class DefaultInspectorHeaderTextResolver : ResoniteEventHandlerMonkey<DefaultInspectorHeaderTextResolver, ResolveInspectorHeaderTextEvent>
    {
        public override int Priority => HarmonyLib.Priority.Normal;

        public override Sequence<string> SubgroupPath => SubgroupDefinitions.InspectorDefaults;

        protected override void Handle(ResolveInspectorHeaderTextEvent eventData)
        {
            if (!eventData.HasDefaultHeader || eventData.DefaultHeaderWasAdded)
                return;

            eventData.AddItem(new(eventData.DefaultHeader.LocaleKey, eventData.DefaultHeader.MinHeight), -HarmonyLib.Priority.First);
            eventData.DefaultHeaderWasAdded = true;
        }
    }
}