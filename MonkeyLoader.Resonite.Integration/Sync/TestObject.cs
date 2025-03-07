using FrooxEngine;
using MonkeyLoader.Logging;
using MonkeyLoader.Resonite.Sync.DynamicVariables;
using MonkeyLoader.Sync;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLoader.Resonite.Sync
{
    internal class TestObject : DynamicVariableSpaceSyncObject<TestObject>
    {
        private static readonly HashSet<TestObject> _objects = [];

        public Logger Logger { get; }
        public DynamicReferenceVariableSyncValue<DynamicVariableSpace?> Space { get; } = new(null);
        public DynamicValueVariableSyncValue<string> TestValue { get; } = new("Hello, World!");

        public TestObject(Logger logger)
        {
            Logger = logger;
            _objects.Add(this);
        }

        [MonkeySyncMethod]
        public void TestMethod()
        {
            Logger.Info(() => $"Called from sync object on space [{Space}]!");

            Logger.Info(() => TestValue);
        }
    }
}