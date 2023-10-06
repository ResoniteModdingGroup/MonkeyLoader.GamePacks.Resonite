// Adapted from the NeosModLoader project.

namespace MonkeyLoader.Configuration
{
    /// <summary>
    /// Represents the data for the <see cref="Config.OnChanged"/> and <see cref="MonkeyLoader.OnAnyConfigChanged"/> events.
    /// </summary>
    public sealed class ConfigChangedEvent
    {
        /// <summary>
        /// Gets the <see cref="Configuration.Config"/> in which the change occured.
        /// </summary>
        public Config Config { get; }

        /// <summary>
        /// Gets the <see cref="ConfigKey"/> who's value changed.
        /// </summary>
        public ConfigKey Key { get; }

        /// <summary>
        /// Gets a custom label that may be set by whoever changed the configuration.
        /// </summary>
        public string? Label { get; }

        /// <summary>
        /// Gets the old value of the <see cref="ConfigKey"/>.<br/>
        /// This can be the default value.
        /// </summary>
        public object? OldValue { get; }

        internal ConfigChangedEvent(Config config, ConfigKey key, object? oldValue, string? label)
        {
            Config = config;
            Key = key;
            OldValue = oldValue;
            Label = label;
        }
    }
}