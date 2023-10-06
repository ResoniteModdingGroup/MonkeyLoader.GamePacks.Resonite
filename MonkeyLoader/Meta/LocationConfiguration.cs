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
        public readonly ConfigKey<string> ConfigsKey = new("Configs", "Paths to check for configuration files.", () => "./MonkeyLoader/Configs");
        public readonly ConfigKey<string> GamePacksKey = new("GamePacks", "Paths to check for game packs.", () => "./MonkeyLoader/GamePacks");
        public readonly ConfigKey<string> LibsKey = new("Libs", "Paths to check for dependency libraries.", () => "./MonkeyLoader/Libs");
        public readonly ConfigKey<List<ModLoadingLocation>> ModsKey = new("Mods", "Paths to check for mods.", () => new() { new ModLoadingLocation("./MonkeyLoader/Mods", true, "\\.disabled") });

        /// <inheritdoc/>
        public override string Description { get; } = "Contains definitions for which paths will be searched for certain resources.";

        /// <inheritdoc/>
        public override string Name { get; } = "Locations";

        /// <inheritdoc/>
        public override Version Version { get; } = new Version(1, 0);
    }
}