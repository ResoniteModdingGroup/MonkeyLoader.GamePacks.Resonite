using MonkeyLoader.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
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
        public readonly ConfigKey<string> ConfigsKey = new("Configs", "Paths to check for configuration files.", () => "./MonkeyLoader/Configs", valueValidator: path => !string.IsNullOrWhiteSpace(path));
        public readonly ConfigKey<string> GamePacksKey = new("GamePacks", "Paths to check for game packs.", () => "./MonkeyLoader/GamePacks", valueValidator: path => !string.IsNullOrWhiteSpace(path));
        public readonly ConfigKey<string> LibsKey = new("Libs", "Paths to check for dependency libraries.", () => "./MonkeyLoader/Libs", valueValidator: path => !string.IsNullOrWhiteSpace(path));
        public readonly ConfigKey<List<ModLoadingLocation>> ModsKey = new("Mods", "Loading locations to check for mods.", () => new() { new ModLoadingLocation("./MonkeyLoader/Mods", true, "\\.disabled") }, valueValidator: locations => locations?.Count > 0);

        public string Configs
        {
            get => Config.GetValue(ConfigsKey);
            set => Config.Set(ConfigsKey, value);
        }

        /// <inheritdoc/>
        public override string Description { get; } = "Contains definitions for which paths will be searched for certain resources.";

        public string GamePacks
        {
            get => Config.GetValue(GamePacksKey);
            set => Config.Set(GamePacksKey, value);
        }

        public string Libs
        {
            get => Config.GetValue(LibsKey);
            set => Config.Set(LibsKey, value);
        }

        // do something to make list changes fire config changed too?
        public List<ModLoadingLocation> Mods
        {
            get => Config.GetValue(ModsKey);
            set => Config.Set(ModsKey, value);
        }

        /// <inheritdoc/>
        public override string Name { get; } = "Locations";

        /// <inheritdoc/>
        public override Version Version { get; } = new Version(1, 0);
    }
}