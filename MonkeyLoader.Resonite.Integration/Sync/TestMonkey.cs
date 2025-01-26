using FrooxEngine;
using MonkeyLoader.Resonite.UI;
using MonkeyLoader.Resonite.UI.Inspectors;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLoader.Resonite.Sync
{
    internal class TestMonkey : ResoniteInspectorMonkey<TestMonkey, BuildInspectorBodyEvent, DynamicVariableSpace>
    {
        private static readonly List<TestObject> _objects = [];

        public override int Priority => HarmonyLib.Priority.Last;

        protected override void Handle(BuildInspectorBodyEvent eventData)
        {
            var space = (DynamicVariableSpace)eventData.Worker;
            eventData.UI.LocalActionButton("Create Test Link!", button =>
            {
                var syncObject = new TestObject(Logger);
                _objects.Add(syncObject);
                syncObject.LinkWith(space);
            });
        }
    }
}