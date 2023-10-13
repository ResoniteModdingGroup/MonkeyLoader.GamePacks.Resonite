using MonkeyLoader.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.NuGet
{
    public sealed class NuGetConfigSection : ConfigSection
    {
        public readonly ConfigKey<bool> EnableLoadingLibsKey = new("EnableLoadingLibs", "Allows checking NuGet feeds to load mod's library dependencies.", () => true);
        public readonly ConfigKey<bool> EnableLoadingModsKey = new("EnableLoadingMods", "Allows checking NuGet feeds to load mod's other-mod dependencies.", () => true);
        public readonly ConfigKey<List<NuGetSource>> NuGetLibSourcesKey = new("NuGetLibSources", "NuGet feeds to check for libraries.", () => new() { new("Official NuGet Feed", new("https://api.nuget.org/v3/index.json")) });

        public readonly ConfigKey<List<NuGetSource>> NuGetModSourcesKey = new("NuGetModSources", "NuGet feeds to check for mods.", () => new());

        /// <inheritdoc/>
        public override string Description { get; } = "Contains definitions for how to use which NuGet feeds.";

        /// <summary>
        /// Gets whether checking NuGet feeds to load mod's library dependencies is enabled.
        /// </summary>
        public bool LoadingLibsEnabled
        {
            get => Config.GetValue(EnableLoadingLibsKey);
            set => Config.Set(EnableLoadingLibsKey, value);
        }

        /// <summary>
        /// Gets whether checking NuGet feeds to load mod's other-mod dependencies is enabled.
        /// </summary>
        public bool LoadingModsEnabled
        {
            get => Config.GetValue(EnableLoadingModsKey);
            set => Config.Set(EnableLoadingModsKey, value);
        }

        /// <inheritdoc/>
        public override string Name { get; } = "NuGet";

        /// <summary>
        /// Gets the NuGet feeds to check for libraries.
        /// </summary>
        public List<NuGetSource> NuGetLibSources
        {
            get => Config.GetValue(NuGetLibSourcesKey);
            set => Config.Set(NuGetLibSourcesKey, value);
        }

        /// <summary>
        /// Gets the NuGet feeds to check for mods.
        /// </summary>
        public List<NuGetSource> NuGetModSources
        {
            get => Config.GetValue(NuGetModSourcesKey);
            set => Config.Set(NuGetModSourcesKey, value);
        }

        /// <inheritdoc/>
        public override Version Version { get; } = new Version(1, 0);
    }
}