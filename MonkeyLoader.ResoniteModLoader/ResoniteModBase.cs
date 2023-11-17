using MonkeyLoader;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using System.Collections.Generic;
using System.Linq;

namespace ResoniteModLoader
{
    /// <summary>
    /// Contains public metadata about a mod.
    /// </summary>
    public abstract class ResoniteModBase : Mod, IMonkey
    {
        /// <inheritdoc/>
        public AssemblyName AssemblyName { get; }

        /// <summary>
        /// Gets the mod's author.
        /// </summary>
        public abstract string Author { get; }

        /// <inheritdoc/>
        public bool Failed { get; } = false;

        /// <inheritdoc/>
        public IEnumerable<IFeaturePatch> FeaturePatches => Enumerable.Empty<IFeaturePatch>();

        /// <summary>
        /// Gets an optional hyperlink to the mod's homepage.
        /// </summary>
        public virtual string? Link { get; }

        /// <inheritdoc/>
        public Mod Mod => this;

        /// <summary>
        /// Gets the mod's name. This must be unique.
        /// </summary>
        public abstract string Name { get; }

        /// <inheritdoc/>
        public bool Ran { get; } = false;

        /// <summary>
        /// Gets the mod's semantic version.
        /// </summary>
        public abstract new string Version { get; }

        internal static MonkeyLoader.MonkeyLoader MonkeyLoader { get; set; } = null!;

        protected Mod()
            : base(MonkeyLoader, new NuGet.Packaging.Core.PackageIdentity( , new NuGet.Versioning.NuGetVersion(Version)
            {
            }


public int CompareTo(IMonkey other) => Comparer<IMonkey>.Default.Compare(this, other);

        /// <summary>
        /// Gets this mod's current <see cref="ModConfiguration"/>.
        /// <para/>
        /// This will always be the same instance.
        /// </summary>
        /// <returns>This mod's current configuration.</returns>
        public ModConfiguration? GetConfiguration()
        {
            if (!FinishedLoading)
                throw new ModConfigurationException($"GetConfiguration() was called before {Name} was done initializing. Consider calling GetConfiguration() from within OnEngineInit()");

            return loadedResoniteMod?.ModConfiguration;
        }

        public bool Run() => throw new System.NotImplementedException();
    }
}