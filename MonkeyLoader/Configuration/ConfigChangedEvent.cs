// Adapted from the NeosModLoader project.

namespace MonkeyLoader.Configuration
{
    /// <summary>
    /// Represents the data for the <see cref="Config.OnThisConfigurationChanged"/> and <see cref="Config.OnAnyConfigurationChanged"/> events.
    /// </summary>
    public sealed class ConfigChangedEvent
    {
        /// <summary>
        /// Gets the <see cref="Configuration.Config"/> in which the change occured.
        /// </summary>
        public Config Config { get; private set; }

        /// <summary>
        /// Gets the specific <see cref="ConfigKey"/> who's value changed.
        /// </summary>
        public ConfigKey Key { get; private set; }

        /// <summary>
        /// Gets a custom label that may be set by whoever changed the configuration.
        /// </summary>
        public string? Label { get; private set; }

        internal ConfigChangedEvent(Config config, ConfigKey key, string? label)
        {
            Config = config;
            Key = key;
            Label = label;
        }
    }
}