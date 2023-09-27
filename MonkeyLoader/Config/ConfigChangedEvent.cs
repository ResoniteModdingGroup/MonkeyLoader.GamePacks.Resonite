// Adapted from the NeosModLoader project.

namespace MonkeyLoader.Config
{
    /// <summary>
    /// Represents the data for the <see cref="ModConfig.OnThisConfigurationChanged"/> and <see cref="ModConfig.OnAnyConfigurationChanged"/> events.
    /// </summary>
    public class ConfigChangedEvent
    {
        /// <summary>
        /// The <see cref="ModConfig"/> in which the change occured.
        /// </summary>
        public ModConfig Config { get; private set; }

        /// <summary>
        /// The specific <see cref="ModConfigKey{T}"/> who's value changed.
        /// </summary>
        public ModConfigKey Key { get; private set; }

        /// <summary>
        /// A custom label that may be set by whoever changed the configuration.
        /// </summary>
        public string? Label { get; private set; }

        internal ConfigChangedEvent(ModConfig config, ModConfigKey key, string? label)
        {
            Config = config;
            Key = key;
            Label = label;
        }
    }
}