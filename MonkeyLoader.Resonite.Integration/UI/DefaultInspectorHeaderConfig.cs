using MonkeyLoader.Configuration;
using MonkeyLoader.Resonite.Configuration;
using System;

namespace MonkeyLoader.Resonite.UI
{
    /// <summary>
    /// Represents the config for the <see cref="DefaultInspectorHeaderHandler"/>.
    /// </summary>
    // No Ranges because there's no long slider :C
    public sealed class DefaultInspectorHeaderConfig : SingletonConfigSection<DefaultInspectorHeaderConfig>
    {
        private static readonly DefiningConfigKey<long> _destroyOffset = new("DestroyOffset", "The Order Offset of the Destroy button on Inspector Headers. Range: 0-16 - Higher is further right.", () => 12, valueValidator: ValidateRange)
        {
            // new ConfigKeyRange<long>(0, 16),
            new ConfigKeySessionShare<long>(12)
        };

        private static readonly DefiningConfigKey<long> _duplicateOffset = new("DuplicateOffset", "The Order Offset of the Duplicate button on Inspector Headers. Range: 0-16 - Higher is further right.", () => 11, valueValidator: ValidateRange)
        {
            // new ConfigKeyRange<long>(0, 16),
            new ConfigKeySessionShare<long>(11)
        };

        private static readonly DefiningConfigKey<long> _openContainerOffset = new("OpenContainerOffset", "The Order Offset of the Open Container button on Inspector Headers. Range: 0-16 - Higher is further right.", () => 4, valueValidator: ValidateRange)
        {
            // new ConfigKeyRange<long>(0, 16),
            new ConfigKeySessionShare<long>(10)
        };

        private readonly DefiningConfigKey<long> _workerNameOffset = new("NameOffset", "The Order Offset of the Worker Name button on Inspector Headers. Range: 0-16 - Higher is further right.", () => 6, valueValidator: ValidateRange)
        {
            // new ConfigKeyRange<long>(0, 16),
            new ConfigKeySessionShare<long>(6)
        };

        /// <inheritdoc/>
        public override string Description => "Options for the default inspector header generation.";

        /// <summary>
        /// Gets the Order Offset share for the Destroy button on Inspector Headers.
        /// </summary>
        public ConfigKeySessionShare<long> DestroyOffset => _destroyOffset.Components.Get<ConfigKeySessionShare<long>>();

        /// <summary>
        /// Gets the Order Offset share for the Duplicate button on Inspector Headers.
        /// </summary>
        public ConfigKeySessionShare<long> DuplicateOffset => _duplicateOffset.Components.Get<ConfigKeySessionShare<long>>();

        /// <inheritdoc/>
        public override string Id => "DefaultInspectorHeader";

        /// <summary>
        /// Gets the Order Offset share for the Open Container button on Inspector Headers.
        /// </summary>
        public ConfigKeySessionShare<long> OpenContainerOffset => _openContainerOffset.Components.Get<ConfigKeySessionShare<long>>();

        /// <inheritdoc/>
        public override Version Version { get; } = new Version(1, 0, 0);

        /// <summary>
        /// Gets the Order Offset share for the Worker Name button on Inspector Headers.
        /// </summary>
        public ConfigKeySessionShare<long> WorkerNameOffset => _workerNameOffset.Components.Get<ConfigKeySessionShare<long>>();

        private static bool ValidateRange(long value)
                                                                    => value is >= 0 and <= 16;
    }
}