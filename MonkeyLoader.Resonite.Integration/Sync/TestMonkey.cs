using FrooxEngine;
using MonkeyLoader.Resonite.UI;
using MonkeyLoader.Resonite.UI.Inspectors;
using MonkeyLoader.Sync;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLoader.Resonite.Sync
{
    internal class TestMonkey : ResoniteInspectorMonkey<TestMonkey, BuildInspectorBodyEvent, DynamicVariableSpace>
    {
        public override int Priority => HarmonyLib.Priority.Last;

        protected override void Handle(BuildInspectorBodyEvent eventData)
        {
            var space = (DynamicVariableSpace)eventData.Worker;
            eventData.UI.LocalActionButton("Create Test Link!", button =>
            {
                var syncObject = new TestObject(Logger);
                syncObject.LinkWith(space);
                syncObject.Space.Value = space;
            });
        }
    }
}