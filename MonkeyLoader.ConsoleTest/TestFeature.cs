using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.ConsoleTest
{
    internal class TestFeature : Feature
    {
        public override AssemblyName Assembly { get; } = new AssemblyName("Test");
        public override string Description { get; } = "Testing so hard right now.";

        public override string Name { get; } = "Test";
    }
}