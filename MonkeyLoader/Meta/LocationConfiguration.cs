using MonkeyLoader.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Meta
{
    /// <summary>
    /// Contains collections of paths which will be searched for certain resources.
    /// </summary>
    public sealed class LocationConfigSection : ConfigSection
    {
        public static ConfigKey<List<string>> ConfigsKey = new("Configs", "Paths to check for configuration files.", () => new List<string>());
        public static ConfigKey<List<string>> GamePacksKey = new("GamePacks", "Paths to check for game packs.", () => new List<string>());
        public static ConfigKey<List<string>> LibsKey = new("Libs", "Paths to check for dependency libraries.", () => new List<string>());
        public static ConfigKey<List<string>> ModsKey = new("Mods", "Paths to check for mods.", () => new List<string>());
        public override string Description { get; } = "Contains collections of paths which will be searched for certain resources.";

        public override string Name { get; } = "Locations";
    }
}