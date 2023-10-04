using MonkeyLoader.Configuration;
using MonkeyLoader.Logging;
using MonkeyLoader.Meta;

namespace MonkeyLoader.Patching
{
    /// <summary>
    /// Represents the base class for patchers that run after a game's assemblies have been loaded.
    /// </summary>
    /// <remarks>
    /// Game assemblies and their types can be directly referenced from these.<br/>
    /// Game tooling packs should expand this with useful overridable methods
    /// that are hooked to different points in the game's lifecycle.
    /// </remarks>
    public abstract class Monkey
    {
        /// <summary>
        /// Gets the <see cref="Configuration.Config"/> that this pre-patcher can use to load <see cref="ConfigSection"/>s.
        /// </summary>
        public Config Config => Mod.Config;

        /// <summary>
        /// Gets the <see cref="MonkeyLogger"/> that this pre-patcher can use to log messages to game-specific channels.
        /// </summary>
        public MonkeyLogger Logger => Mod.Logger;

        /// <summary>
        /// Gets the mod that this patcher is a part of.
        /// </summary>
        public Mod Mod { get; internal set; }

        /// <summary>
        /// Called right after the game tooling packs and all the game's assemblies have been loaded.
        /// </summary>
        protected internal virtual void OnLoaded()
        { }
    }
}