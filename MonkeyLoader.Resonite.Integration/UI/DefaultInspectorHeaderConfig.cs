using FrooxEngine;
using MonkeyLoader.Configuration;
using MonkeyLoader.Resonite.Configuration;
using MonkeyLoader.Resonite.UI.Inspectors;
using System;

namespace MonkeyLoader.Resonite.UI
{
    /// <summary>
    /// Represents the config for the <see cref="DefaultInspectorHeaderHandler"/>.
    /// </summary>
    public sealed class DefaultInspectorHeaderConfig : SingletonConfigSection<DefaultInspectorHeaderConfig>
    {
        private readonly DefiningConfigKey<int> _destroyOffset = new("DestroyOffset", "The Order Offset of the Destroy button on Inspector Headers. Range: 0-16 - Higher is further right.", () => 12)
        {
            OffsetRange,
            new ConfigKeySessionShare<int, long>(IntToLong, LongToInt, 12)
        };

        private readonly DefiningConfigKey<int> _duplicateOffset = new("DuplicateOffset", "The Order Offset of the Duplicate button on Inspector Headers. Range: 0-16 - Higher is further right.", () => 11)
        {
            OffsetRange,
            new ConfigKeySessionShare<int, long>(IntToLong, LongToInt, 11)
        };

        private readonly DefiningConfigKey<int> _openContainerOffset = new("OpenContainerOffset", "The Order Offset of the Open Container button on Inspector Headers. Range: 0-16 - Higher is further right.", () => 4)
        {
            OffsetRange,
            new ConfigKeySessionShare<int, long>(IntToLong, LongToInt, 10)
        };

        private readonly DefiningConfigKey<int> _workerNameOffset = new("NameOffset", "The Order Offset of the Worker Name button on Inspector Headers. Range: 0-16 - Higher is further right.", () => 6)
        {
            OffsetRange,
            new ConfigKeySessionShare<int, long>(IntToLong, LongToInt, 6)
        };

        /// <summary>
        /// Gets the range component used for <see cref="WorkerInspector"/> header items.
        /// </summary>
        public static ConfigKeyRange<int> OffsetRange { get; } = new ConfigKeyRange<int>(0, 16);

        /// <inheritdoc/>
        public override string Description => "Options for the default inspector header generation.";

        /// <summary>
        /// Gets the Order Offset share for the Destroy button on Inspector Headers.
        /// </summary>
        public ConfigKeySessionShare<int, long> DestroyOffset => _destroyOffset.Components.Get<ConfigKeySessionShare<int, long>>();

        /// <summary>
        /// Gets the Order Offset share for the Duplicate button on Inspector Headers.
        /// </summary>
        public ConfigKeySessionShare<int, long> DuplicateOffset => _duplicateOffset.Components.Get<ConfigKeySessionShare<int, long>>();

        /// <inheritdoc/>
        public override string Id => "DefaultInspectorHeader";

        /// <summary>
        /// Gets the Order Offset share for the Open Container button on Inspector Headers.
        /// </summary>
        public ConfigKeySessionShare<int, long> OpenContainerOffset => _openContainerOffset.Components.Get<ConfigKeySessionShare<int, long>>();

        /// <inheritdoc/>
        public override Version Version { get; } = new Version(1, 0, 1);

        /// <summary>
        /// Gets the Order Offset share for the Worker Name button on Inspector Headers.
        /// </summary>
        public ConfigKeySessionShare<int, long> WorkerNameOffset => _workerNameOffset.Components.Get<ConfigKeySessionShare<int, long>>();

        private static long IntToLong(int value) => value;

        private static int LongToInt(long value) => (int)value;
    }
}