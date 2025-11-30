using MonkeyLoader.Configuration;
using System;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Contains settings for the engine init hook.
    /// </summary>
    public sealed class LoadingConfig : SingletonConfigSection<LoadingConfig>
    {
        private static readonly DefiningConfigKey<bool> _alwaysShowLoadingPhases = new("AlwaysShowLoadingPhases", "Controls whether every fixed phase and subphase is shown as text on the splash screen's loading state.<br/>Only used when there is a load progress indicator and only takes effect on the next launch.", () => true);
        private static readonly DefiningConfigKey<bool> _hijackLoadProgessIndicator = new("HijackLoadProgressIndicator", "Controls whether the load progress indicator on the splash screen of the graphical client should be hijacked and made to show the loading state.<br/>Only takes effect on the next launch.", () => true);
        private static readonly DefiningConfigKey<bool> _prettySplashProgress = new("PrettySplashProgress", "Controls whether the loading is slowed down slightly to show every initializing monkey on the splash screen's loading state.<br/>Only used when there is a load progress indicator and only takes effect on the next launch.", () => true);

        /// <summary>
        /// Gets whether every fixed phase and subphase is shown as text on the splash screen's loading state.
        /// </summary>
        public bool AlwaysShowLoadingPhases => _alwaysShowLoadingPhases;

        /// <inheritdoc/>
        public override string Description => "Contains settings for engine initialization hook.";

        /// <summary>
        /// Gets whether the load progress indicator on the splash screen of
        /// the graphical client should be hijacked and made to show the loading state.
        /// </summary>
        public bool HijackLoadProgressIndicator => _hijackLoadProgessIndicator;

        /// <inheritdoc/>
        public override string Id => "Loading";

        /// <summary>
        /// Gets whether the loading should be slowed down slightly to show
        /// every initializing monkey on the splash screen's loading state.
        /// </summary>
        public bool PrettySplashProgress => _prettySplashProgress;

        /// <inheritdoc/>
        public override Version Version { get; } = new Version(1, 0, 0);
    }
}