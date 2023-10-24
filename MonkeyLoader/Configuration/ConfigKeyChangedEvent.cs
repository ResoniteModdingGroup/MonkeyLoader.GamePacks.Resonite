// Adapted from the NeosModLoader project.

using System.Diagnostics.CodeAnalysis;

namespace MonkeyLoader.Configuration
{
    /// <summary>
    /// The delegate that is called for configuration change events.
    /// </summary>
    /// <param name="configChangedEvent">The event containing details about the configuration change</param>
    public delegate void ConfigChangedEventHandler(IConfigChangedEvent configChangedEvent);

    /// <summary>
    /// The delegate that is called for configuration change events.
    /// </summary>
    /// <typeparam name="T">The type of the key's value.</typeparam>
    /// <param name="configChangedEvent">The event containing details about the configuration change</param>
    public delegate void ConfigKeyChangedEventHandler<T>(ConfigChangedEvent<T> configChangedEvent);

    /// <summary>
    /// Represents the data for the <see cref="Config.OnChanged"/> and <see cref="MonkeyLoader.OnAnyConfigChanged"/> events.
    /// </summary>
    /// <typeparam name="T">The type of the key's value.</typeparam>
    public sealed class ConfigChangedEvent<T> : IConfigChangedEvent
    {
        /// <inheritdoc/>
        public Config Config { get; }

        /// <inheritdoc/>
        public bool HasLabel => Label is not null;

        /// <summary>
        /// Gets the <see cref="ConfigKey{T}"/> who's value changed.
        /// </summary>
        public ConfigKey<T> Key { get; }

        ConfigKey IConfigChangedEvent.Key => Key;

        /// <inheritdoc/>
        [MemberNotNullWhen(true, nameof(Label))]
        public string? Label { get; }

        /// <summary>
        /// Gets the new value of the <see cref="ConfigKey{T}"/>.<br/>
        /// This can be the default value.
        /// </summary>
        public T NewValue { get; }

        object? IConfigChangedEvent.NewValue => NewValue;

        /// <summary>
        /// Gets the old value of the <see cref="ConfigKey{T}"/>.<br/>
        /// This can be the default value.
        /// </summary>
        public T OldValue { get; }

        /// <summary>
        /// Gets the new value of the <see cref="ConfigKey"/>.<br/>
        /// This can be the default value.
        /// </summary>
        object? IConfigChangedEvent.OldValue => OldValue;

        internal ConfigChangedEvent(Config config, ConfigKey<T> key, T oldValue, T newValue, string? label)
        {
            Config = config;
            Key = key;
            OldValue = oldValue;
            NewValue = newValue;
            Label = label;
        }
    }

    /// <summary>
    /// Represents a non-generic <see cref="ConfigChangedEvent{T}"/>.
    /// </summary>
    public interface IConfigChangedEvent
    {
        /// <summary>
        /// Gets the <see cref="Configuration.Config"/> in which the change occured.
        /// </summary>
        public Config Config { get; }

        /// <summary>
        /// Gets whether a custom label was set by whoever changed the configuration.
        /// <see cref="Label">Label</see> won't be <c>null</c>, if this is <c>true</c>.
        /// </summary>
        public bool HasLabel { get; }

        /// <summary>
        /// Gets the <see cref="ConfigKey"/> who's value changed.
        /// </summary>
        public ConfigKey Key { get; }

        /// <summary>
        /// Gets a custom label that may be set by whoever changed the configuration.
        /// </summary>
        [MemberNotNullWhen(true, nameof(Label))]
        public string? Label { get; }

        /// <summary>
        /// Gets the new value of the <see cref="ConfigKey"/>.<br/>
        /// This can be the default value.
        /// </summary>
        public object? NewValue { get; }

        /// <summary>
        /// Gets the old value of the <see cref="ConfigKey"/>.<br/>
        /// This can be the default value.
        /// </summary>
        public object? OldValue { get; }
    }
}