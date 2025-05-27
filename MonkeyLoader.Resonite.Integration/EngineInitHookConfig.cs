using MonkeyLoader.Configuration;
using System;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Contains settings for the engine init hook.
    /// </summary>
    public sealed class EngineInitHookConfig : SingletonConfigSection<EngineInitHookConfig>
    {
        private static readonly DefiningConfigKey<bool> _slowdownSplash = new("PrettySplashEnabled", "Controls whether splash screen initialization awaits showing loading state texts.", () => true);
        /// <inheritdoc/>
        public override string Description => "Contains settings for engine initialization hook.";

        /// <summary>
        /// Gets whether the Resonite splash should be made to slow down to have time to read the texts
        /// </summary>
        public bool PrettySplashEnabled => _slowdownSplash;

        /// <inheritdoc/>
        public override string Id => "EngineInitHook";

        /// <inheritdoc/>
        public override Version Version { get; } = new Version(1, 0, 0);
    }
}
