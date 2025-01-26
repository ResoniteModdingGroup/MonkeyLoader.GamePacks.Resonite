using MonkeyLoader.Logging;
using MonkeyLoader.Sync;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLoader.Resonite.Sync
{
    internal class TestObject : DynamicVariableSpaceSyncObject<TestObject>
    {
        public Logger Logger { get; }
        public MonkeySyncValue<string> TestValue { get; } = "Hello, World!";

        public TestObject(Logger logger)
        {
            Logger = logger;
        }

        [MonkeySyncMethod]
        public void TestMethod()
        {
            Logger.Info(() => "Called from sync object!");

            // Need to overwrite ToString
            Logger.Info(() => TestValue);
        }
    }
}