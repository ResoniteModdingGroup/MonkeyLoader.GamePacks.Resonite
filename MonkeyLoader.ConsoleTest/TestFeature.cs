using MonkeyLoader.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.ConsoleTest
{
    internal class TestFeature : GameFeature
    {
        public override string Description => "Testing so hard right now.";

        public override string Name => "Test";

        public TestFeature()
        { }
    }
}