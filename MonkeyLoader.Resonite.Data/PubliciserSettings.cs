using MonkeyLoader.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite
{
    internal sealed class PubliciserSettings : ConfigSection
    {
        public static DefiningConfigKey<HashSet<string>> AssembliesKey = new("Assemblies", "Names of the Assemblies which should be publicised.", () => new() { "FrooxEngine" });

        public static HashSet<string> Assemblies
        {
            get => AssembliesKey.GetValue()!;
            set => AssembliesKey.SetValue(value);
        }

        public override string Description { get; } = "Contains settings for the publiciser.";
        public override string Name { get; } = "Publiciser";
        public override Version Version { get; } = new Version(1, 0, 0);
    }
}