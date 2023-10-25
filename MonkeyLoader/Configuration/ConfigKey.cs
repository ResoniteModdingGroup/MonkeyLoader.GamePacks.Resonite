// Adapted from the NeosModLoader project.

using MonkeyLoader.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace MonkeyLoader.Configuration
{
    /// <summary>
    /// Represents a typed configuration key.
    /// </summary>
    /// <typeparam name="T">The type of this key's value.</typeparam>
    public class ConfigKey<T> : ConfigKey
    {
        private readonly Func<T>? _computeDefault;

        private readonly Predicate<T?>? _isValueValid;

        private T? _value;

        /// <inheritdoc/>
        public override Type ValueType { get; } = typeof(T);

        /// <summary>
        /// Gets the defining key for this config item.
        /// </summary>
        /// <remarks>
        /// Each configuration item has exactly ONE defining key, and that is the key defined by the mod.
        /// Duplicate keys can be created (they only need to share the same Name) and they'll still work
        /// for reading configs.
        /// <para/>
        /// This is a non-null self-reference for the defining key itself as soon it is tied to a <see cref="ConfigSection"/>.
        /// </remarks>
        internal new ConfigKey<T>? DefiningKey
        {
            get => (ConfigKey<T>?)base.DefiningKey;
            set => base.DefiningKey = value;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ConfigKey{T}"/> class with the given parameters.
        /// </summary>
        /// <param name="name">The mod-unique name of this config item.</param>
        /// <param name="description">The human-readable description of this config item.</param>
        /// <param name="computeDefault">The function that computes a default value for this key. Otherwise <c>default(<typeparamref name="T"/>)</c> will be used.</param>
        /// <param name="internalAccessOnly">If <c>true</c>, only the owning mod should have access to this config item.</param>
        /// <param name="valueValidator">The function that checks if the given value is valid for this configuration item. Otherwise everything will be accepted.</param>
        public ConfigKey(string name, string? description = null, Func<T>? computeDefault = null, bool internalAccessOnly = false, Predicate<T?>? valueValidator = null)
            : base(name, description, internalAccessOnly)
        {
            _computeDefault = computeDefault;
            _isValueValid = valueValidator;
        }

        /// <inheritdoc/>
        public override bool TryComputeDefault(out object? defaultValue)
        {
            if (TryComputeDefault(out T? defaultTypedValue))
            {
                defaultValue = defaultTypedValue;
                return true;
            }

            defaultValue = null;
            if (!Validate(defaultValue))
                throw new InvalidOperationException($"(Computed) default value for key [{Name}] did not pass validation!");

            return false;
        }

        /// <summary>
        /// Tries to compute the default value for this key, if a default provider was set.
        /// </summary>
        /// <param name="defaultValue">The computed default value if the return value is <c>true</c>. Otherwise <c>default(T)</c>.</param>
        /// <returns>Whether the default value was successfully computed.</returns>
        public bool TryComputeDefault(out T? defaultValue)
        {
            bool success;
            if (_computeDefault is null)
            {
                success = false;
                defaultValue = default;
            }
            else
            {
                success = true;
                defaultValue = _computeDefault();
            }

            if (!Validate((object?)defaultValue))
                throw new InvalidOperationException($"(Computed) default value for key [{Name}] did not pass validation!");

            return success;
        }

        /// <inheritdoc/>
        public override bool Validate(object? value)
            => (value is T || (value is null && Util.CanBeNull(ValueType))) && Validate((T)value!);

        /// <summary>
        /// Checks if a given value is valid for this configuration item.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns><c>true</c> if the value is valid.</returns>
        public bool Validate(T value) => _isValueValid?.Invoke(value) ?? true;

        internal override void Set(object? value, string? eventLabel = null) => Set((T)value!, eventLabel);

        internal void Set(T value, string? eventLabel = null)
        {
            var oldValue = _value;
            _value = value;
            HasValue = true;

            OnChanged(new ConfigKeyChangedEventArgs<T>(Config!, this, oldValue, _value, eventLabel));
        }

        internal override bool TryGetValue(out object? value)
        {
            if (HasValue)
            {
                value = _value;
                return true;
            }

            value = null;
            return false;
        }

        internal bool TryGetValue(out T? value)
        {
            if (HasValue)
            {
                value = _value;
                return true;
            }

            value = default;
            return false;
        }

        internal override bool Unset()
        {
            var hadValue = HasValue;
            var oldValue = _value;

            _value = default;
            HasValue = false;

            OnChanged(new ConfigKeyChangedEventArgs<T>(Config!, this, oldValue, _value, nameof(Unset)));

            return hadValue;
        }

        /// <summary>
        /// Triggers this config item's <see cref="Changed">Changed</see> event.
        /// </summary>
        /// <param name="configKeyChangedEventArgs">The event containing details about the change.</param>
        protected virtual void OnChanged(ConfigKeyChangedEventArgs<T> configKeyChangedEventArgs)
        {
            try
            {
                Changed?.TryInvokeAll(this, configKeyChangedEventArgs);
            }
            catch (AggregateException ex)
            {
                Logger!.Error(() => ex.Format($"Some typed {nameof(Changed)} event subscriber(s) of key [{Name}] threw an exception:"));
            }

            base.OnChanged(configKeyChangedEventArgs);
        }

        /// <summary>
        /// Triggered when the internal value of this config item changes.
        /// </summary>
        public new event ConfigKeyChangedEventHandler<T>? Changed;
    }

    /// <summary>
    /// Represents an untyped config item.<br/>
    /// All deriving implementations must go through <see cref="ConfigKey{T}"/>.
    /// </summary>
    public abstract class ConfigKey
    {
        /// <summary>
        /// Gets the config this item belongs to if it's a <see cref="IsDefiningKey">defining key</see>.
        /// </summary>
        public Config? Config { get; internal set; }

        /// <summary>
        /// Gets the human-readable description of this config item. Should be specified by the defining mod.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Gets whether this config item has a useable <see cref="Description">description</see>.
        /// </summary>
        [MemberNotNullWhen(true, nameof(Description))]
        public bool HasDescription { get; }

        /// <summary>
        /// Gets whether only the owning mod should have access to this config item.
        /// </summary>
        public bool InternalAccessOnly { get; }

        /// <summary>
        /// Gets whether this config item is the definition for the <see cref="Config"/>.
        /// </summary>
        [MemberNotNullWhen(true, nameof(DefiningKey), nameof(Config), nameof(Logger))]
        public bool IsDefiningKey => ReferenceEquals(DefiningKey, this);

        /// <summary>
        /// Gets the mod-unique name of this config item. Must be present.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Get the <see cref="Type"/> of this config item's value.
        /// </summary>
        public abstract Type ValueType { get; }

        /// <summary>
        /// Gets the defining key for this config item.
        /// </summary>
        /// <remarks>
        /// Each configuration item has exactly ONE defining key, and that is the key defined by the mod.
        /// Duplicate keys can be created (they only need to share the same Name) and they'll still work
        /// for reading configs.
        /// <para/>
        /// This is a non-null self-reference for the defining key itself as soon it is tied to a <see cref="ConfigSection"/>.
        /// </remarks>
        internal ConfigKey? DefiningKey { get; set; }

        /// <summary>
        /// Gets whether this config item has a set value.
        /// </summary>
        internal bool HasValue { get; set; }

        internal ConfigSection Section { get; set; }

        /// <summary>
        /// Gets the logger of the config this item belongs to if it's a <see cref="IsDefiningKey">defining key</see>.
        /// </summary>
        protected MonkeyLogger? Logger => Config?.Logger;

        internal ConfigKey(string name, string? description = null, bool internalAccessOnly = false)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("Configuration key name must not be null or whitespace!");

            Name = name;
            Description = description;
            HasDescription = !string.IsNullOrWhiteSpace(description);
            InternalAccessOnly = internalAccessOnly;
        }

        /// <summary>
        /// Checks if two <see cref="ConfigKey"/>s are unequal.
        /// </summary>
        /// <param name="left">The first key.</param>
        /// <param name="right">The second key.</param>
        /// <returns><c>true</c> if they're considered unequal.</returns>
        public static bool operator !=(ConfigKey? left, ConfigKey? right)
            => !(left == right);

        /// <summary>
        /// Checks if two <see cref="ConfigKey"/>s are equal.
        /// </summary>
        /// <param name="left">The first key.</param>
        /// <param name="right">The second key.</param>
        /// <returns><c>true</c> if they're considered equal.</returns>
        public static bool operator ==(ConfigKey? left, ConfigKey? right)
            => ReferenceEquals(left, right)
            || (left is not null && right is not null && left.Name == right.Name);

        /// <summary>
        /// Checks if the given object can be considered equal to this one.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns><c>true</c> if the other object is considered equal.</returns>
        public override bool Equals(object obj) => obj is ConfigKey key && key == this;

        /// <inheritdoc/>
        public override int GetHashCode() => Name.GetHashCode();

        /// <summary>
        /// Tries to compute the default value for this key, if a default provider was set.
        /// </summary>
        /// <param name="defaultValue">The computed default value if the return value is <c>true</c>. Otherwise <c>default</c>.</param>
        /// <returns><c>true</c> if the default value was successfully computed.</returns>
        public abstract bool TryComputeDefault(out object? defaultValue);

        /// <summary>
        /// Checks if a value is valid for this configuration item.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns><c>true</c> if the value is valid.</returns>
        public abstract bool Validate(object? value);

        internal abstract void Set(object? value, string? eventLabel = null);

        internal abstract bool TryGetValue(out object? value);

        internal abstract bool Unset();

        /// <summary>
        /// Triggers this config item's <see cref="Changed">Changed</see> event.
        /// </summary>
        /// <param name="configKeyChangedEventArgs">The event containing details about the change.</param>
        protected virtual void OnChanged(IConfigKeyChangedEventArgs configKeyChangedEventArgs)
        {
            try
            {
                Changed?.TryInvokeAll(this, configKeyChangedEventArgs);
            }
            catch (AggregateException ex)
            {
                Logger!.Error(() => ex.Format($"Some untyped {nameof(Changed)} event subscriber(s) of key [{Name}] threw an exception:"));
            }

            Config!.OnChanged(configKeyChangedEventArgs);
        }

        /// <summary>
        /// Triggered when the internal value of this config item changes.
        /// </summary>
        public event ConfigKeyChangedEventHandler? Changed;
    }
}