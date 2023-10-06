// Adapted from the NeosModLoader project.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MonkeyLoader.Configuration
{
    /// <summary>
    /// Represents an untyped configuration key.<br/>
    /// All derivatives must go through <see cref="ConfigKey{T}"/>.
    /// </summary>
    public abstract class ConfigKey
    {
        /// <summary>
        /// Each configuration item has exactly ONE defining key, and that is the key defined by the mod.
        /// Duplicate keys can be created (they only need to share the same Name) and they'll still work
        /// for reading configs.
        /// <para/>
        /// This is a non-null self-reference for the defining key itself as soon it is tied to a <see cref="ConfigSection"/>.
        /// </summary>
        internal ConfigKey? DefiningKey;

        internal bool HasValue;

        private object? value;

        /// <summary>
        /// Gets the human-readable description of this config item. Should be specified by the defining mod.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Gets whether only the owning mod should have access to this config item.
        /// </summary>
        public bool InternalAccessOnly { get; }

        /// <summary>
        /// Gets the mod-unique name of this config item. Must be present.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Get the <see cref="Type"/> of this key's value.
        /// </summary>
        public abstract Type ValueType { get; }

        internal ConfigKey(string name, string? description, bool internalAccessOnly)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("Configuration key name must not be null or whitespace");

            Name = name;
            Description = description;
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

        internal void Set(object? value)
        {
            this.value = value;
            HasValue = true;
        }

        internal bool TryGetValue(out object? value)
        {
            if (HasValue)
            {
                value = this.value;
                return true;
            }

            value = null;
            return false;
        }

        internal bool Unset()
        {
            var hadValue = HasValue;
            HasValue = false;

            return hadValue;
        }
    }

    /// <summary>
    /// Represents a typed configuration key.
    /// </summary>
    /// <typeparam name="T">The type of this key's value.</typeparam>
    public class ConfigKey<T> : ConfigKey
    {
        private readonly Func<T>? computeDefault;

        private readonly Predicate<T?>? isValueValid;

        /// <inheritdoc/>
        public override Type ValueType { get; } = typeof(T);

        /// <summary>
        /// Creates a new instance of the <see cref="ConfigKey{T}"/> class with the given parameters.
        /// </summary>
        /// <param name="name">The mod-unique name of this config item.</param>
        /// <param name="description">The human-readable description of this config item.</param>
        /// <param name="computeDefault">The function that computes a default value for this key. Otherwise <c>default(<typeparamref name="T"/>)</c> will be used.</param>
        /// <param name="internalAccessOnly">If <c>true</c>, only the owning mod should have access to this config item.</param>
        /// <param name="valueValidator">The function that checks if the given value is valid for this configuration item. Otherwise everything will be accepted.</param>
        public ConfigKey(string name, string? description = null, Func<T>? computeDefault = null, bool internalAccessOnly = false, Predicate<T?>? valueValidator = null) : base(name, description, internalAccessOnly)
        {
            this.computeDefault = computeDefault;
            isValueValid = valueValidator;
        }

        /// <inheritdoc/>
        public override bool TryComputeDefault(out object? defaultValue)
        {
            if (TryComputeDefaultTyped(out T? defaultTypedValue))
            {
                defaultValue = defaultTypedValue;
                return true;
            }

            defaultValue = null;
            return false;
        }

        /// <summary>
        /// Tries to compute the default value for this key, if a default provider was set.
        /// </summary>
        /// <param name="defaultValue">The computed default value if the return value is <c>true</c>. Otherwise <c>default(T)</c>.</param>
        /// <returns><c>true</c> if the default value was successfully computed.</returns>
        public bool TryComputeDefaultTyped([NotNullWhen(true)] out T? defaultValue)
        {
            if (computeDefault is null)
            {
                defaultValue = default;
                return false;
            }

            defaultValue = computeDefault()!;
            return true;
        }

        /// <inheritdoc/>
        public override bool Validate(object? value)
        {
            // value is of the correct type
            if (value is T typedValue)
                return ValidateTyped(typedValue);

            if (value is null)
            {
                // null is valid for T
                if (Util.CanBeNull(ValueType))
                    return ValidateTyped((T?)value);

                // null is not valid for T
                return false;
            }

            // value is of the wrong type
            return false;
        }

        /// <summary>
        /// Checks if a given value is valid for this configuration item.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns><c>true</c> if the value is valid.</returns>
        public bool ValidateTyped(T? value) => isValueValid?.Invoke(value) ?? true;
    }
}