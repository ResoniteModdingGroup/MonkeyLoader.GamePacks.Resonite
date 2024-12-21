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
        private readonly DefiningConfigKey<int> _destroyOffset = new("DestroyOffset", "The Order Offset of the Destroy button on Inspector Headers. Higher is further right.", () => 12)
        {
            OffsetRange,
            MakeOffsetRangeShare(12)
        };

        private readonly DefiningConfigKey<int> _duplicateOffset = new("DuplicateOffset", "The Order Offset of the Duplicate button on Inspector Headers. Higher is further right.", () => 11)
        {
            OffsetRange,
            MakeOffsetRangeShare(11)
        };

        private readonly DefiningConfigKey<int> _openContainerOffset = new("OpenContainerOffset", "The Order Offset of the Open Container button on Inspector Headers. Higher is further right.", () => 4)
        {
            OffsetRange,
            MakeOffsetRangeShare(10)
        };

        private readonly DefiningConfigKey<int> _workerNameOffset = new("NameOffset", "The Order Offset of the Worker Name button on Inspector Headers. Higher is further right.", () => 6)
        {
            OffsetRange,
            MakeOffsetRangeShare(6)
        };

        /// <summary>
        /// Gets the range component used for the offset of <see cref="WorkerInspector"/> header items.
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

        /// <summary>
        /// Makes a new session share component used for the offset of
        /// <see cref="WorkerInspector"/> header items, optionally using the given default.
        /// </summary>
        /// <param name="defaultValue">The default value for the shared config item for users that don't have it themselves.</param>
        /// <returns>The newly created share component with the optional default.</returns>
        public static ConfigKeySessionShare<int, long> MakeOffsetRangeShare(int defaultValue = default)
            => new(IntToLong, LongToInt, defaultValue);

        private static long IntToLong(int value) => value;

        private static int LongToInt(long value) => (int)value;
    }
}