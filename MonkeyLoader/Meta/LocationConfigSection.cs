using MonkeyLoader.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        public readonly DefiningConfigKey<string> ConfigsKey = new("Configs", "Paths to check for configuration files.", () => "./MonkeyLoader/Configs", valueValidator: path => !string.IsNullOrWhiteSpace(path));
        public readonly DefiningConfigKey<string> GamePacksKey = new("GamePacks", "Paths to check for game packs.", () => "./MonkeyLoader/GamePacks", valueValidator: path => !string.IsNullOrWhiteSpace(path));
        public readonly DefiningConfigKey<string> LibsKey = new("Libs", "Paths to check for dependency libraries.", () => "./MonkeyLoader/Libs", valueValidator: path => !string.IsNullOrWhiteSpace(path));
        public readonly DefiningConfigKey<List<ModLoadingLocation>> ModsKey = new("Mods", "Loading locations to check for mods.", () => new() { new ModLoadingLocation("./MonkeyLoader/Mods", true, "\\.disabled") }, valueValidator: locations => locations?.Count > 0);
        public readonly DefiningConfigKey<string?> PatchedAssembliesKey = new("PatchedAssemblies", "Path to save pre-patched assemblies to. Set null to disable.", () => "./MonkeyLoader/PatchedAssemblies");

        private const string SetEventLabel = "Property";

        public string Configs
        {
            get => ConfigsKey.GetValue()!;
            set => ConfigsKey.SetValue(value, SetEventLabel);
        }

        /// <inheritdoc/>
        public override string Description { get; } = "Contains definitions for which paths will be searched for certain resources.";

        public string GamePacks
        {
            get => GamePacksKey.GetValue()!;
            set => GamePacksKey.SetValue(value, SetEventLabel);
        }

        public string Libs
        {
            get => LibsKey.GetValue()!;
            set => LibsKey.SetValue(value, SetEventLabel);
        }

        // do something to make list changes fire config changed too?
        public List<ModLoadingLocation> Mods
        {
            get => ModsKey.GetValue()!;
            set => ModsKey.SetValue(value, SetEventLabel);
        }

        /// <inheritdoc/>
        public override string Name { get; } = "Locations";

        public string? PatchedAssemblies
        {
            get => PatchedAssembliesKey.GetValue();
            set => PatchedAssembliesKey.SetValue(value, SetEventLabel);
        }

        [MemberNotNullWhen(true, nameof(PatchedAssemblies))]
        public bool SavePatchedAssemblies => PatchedAssemblies is not null;

        /// <inheritdoc/>
        public override Version Version { get; } = new Version(1, 0);
    }
}