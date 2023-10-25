// Adapted from the NeosModLoader project.

using System.Diagnostics.CodeAnalysis;

namespace MonkeyLoader.Configuration
{
    /// <summary>
    /// The delegate that is called for configuration change events.
    /// </summary>
    /// <param name="sender">The object that sent the event.</param>
    /// <param name="configKeyChangedEventArgs">The event containing details about the change.</param>
    public delegate void ConfigKeyChangedEventHandler(object sender, IConfigKeyChangedEventArgs configKeyChangedEventArgs);

    /// <summary>
    /// The delegate that is called for a <see cref="DefiningConfigKey{T}"/>'s <see cref="DefiningConfigKey{T}.Changed">changed event</see>.
    /// </summary>
    /// <typeparam name="T">The type of the key's value.</typeparam>
    /// <param name="sender">The object that sent the event.</param>
    /// <param name="configKeyChangedEventArgs">The event containing details about the change.</param>
    public delegate void ConfigKeyChangedEventHandler<T>(object sender, ConfigKeyChangedEventArgs<T> configKeyChangedEventArgs);

    /// <summary>
    /// Represents the data for the <see cref="Config.Changed"/> and <see cref="MonkeyLoader.AnyConfigChanged"/> events.
    /// </summary>
    /// <typeparam name="T">The type of the key's value.</typeparam>
    public sealed class ConfigKeyChangedEventArgs<T> : IConfigKeyChangedEventArgs
    {
        /// <inheritdoc/>
        public Config Config { get; }

        /// <inheritdoc/>
        [MemberNotNullWhen(true, nameof(Label))]
        public bool HasLabel => Label is not null;

        /// <summary>
        /// Gets the <see cref="DefiningConfigKey{T}"/> who's value changed.
        /// </summary>
        public DefiningConfigKey<T> Key { get; }

        DefiningConfigKey IConfigKeyChangedEventArgs.Key => Key;

        /// <inheritdoc/>
        public string? Label { get; }

        /// <summary>
        /// Gets the new value of the <see cref="DefiningConfigKey{T}"/>.<br/>
        /// This can be the default value.
        /// </summary>
        public T? NewValue { get; }

        object? IConfigKeyChangedEventArgs.NewValue => NewValue;

        /// <summary>
        /// Gets the old value of the <see cref="DefiningConfigKey{T}"/>.<br/>
        /// This can be the default value.
        /// </summary>
        public T? OldValue { get; }

        object? IConfigKeyChangedEventArgs.OldValue => OldValue;

        internal ConfigKeyChangedEventArgs(Config config, DefiningConfigKey<T> key, T? oldValue, T? newValue, string? label)
        {
            Config = config;
            Key = key;
            OldValue = oldValue;
            NewValue = newValue;
            Label = label;
        }
    }

    /// <summary>
    /// Represents a non-generic <see cref="ConfigKeyChangedEventArgs{T}"/>.
    /// </summary>
    public interface IConfigKeyChangedEventArgs
    {
        /// <summary>
        /// Gets the <see cref="Configuration.Config"/> in which the change occured.
        /// </summary>
        public Config Config { get; }

        /// <summary>
        /// Gets whether a custom label was set by whoever changed the configuration.
        /// <see cref="Label">Label</see> won't be <c>null</c>, if this is <c>true</c>.
        /// </summary>
        [MemberNotNullWhen(true, nameof(Label))]
        public bool HasLabel { get; }

        /// <summary>
        /// Gets the <see cref="DefiningConfigKey"/> who's value changed.
        /// </summary>
        public DefiningConfigKey Key { get; }

        /// <summary>
        /// Gets a custom label that may be set by whoever changed the configuration.
        /// </summary>
        public string? Label { get; }

        /// <summary>
        /// Gets the new value of the <see cref="DefiningConfigKey"/>.<br/>
        /// This can be the default value.
        /// </summary>
        public object? NewValue { get; }

        /// <summary>
        /// Gets the old value of the <see cref="DefiningConfigKey"/>.<br/>
        /// This can be the default value.
        /// </summary>
        public object? OldValue { get; }
    }
}