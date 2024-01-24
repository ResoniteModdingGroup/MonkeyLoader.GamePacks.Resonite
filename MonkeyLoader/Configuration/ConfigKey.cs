using MonkeyLoader.Logging;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace MonkeyLoader.Configuration
{
    /// <summary>
    /// Represents a name-only configuration item, which can be used to get or set the values of defining keys with the same name.
    /// </summary>
    public class ConfigKey : IConfigKey
    {
        /// <summary>
        /// The event label used when a config item's value is set from getting the computed default.
        /// </summary>
        public const string SetFromDefaultEventLabel = "Default";

        /// <summary>
        /// Gets the custom <see cref="IEqualityComparer{T}"/> for <see cref="IConfigKey"/>s.
        /// </summary>
        public static readonly IEqualityComparer<IConfigKey> EqualityComparer = new ConfigKeyEqualityComparer();

        /// <summary>
        /// Gets whether this instance defines the config item with this <see cref="Name">Name</see>.
        /// </summary>
        public virtual bool IsDefiningKey => false;

        /// <summary>
        /// Gets the mod-unique name of this config item.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Creates a new name-only configuration item with the given name.
        /// </summary>
        /// <param name="name">The mod-unique name of the configuration item. Must not be null or whitespace.</param>
        /// <exception cref="ArgumentNullException">If the <paramref name="name"/> is null or whitespace.</exception>
        public ConfigKey(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("Config key name must not be null or whitespace!");

            Name = name;
        }

        /// <summary>
        /// Creates a new <see cref="ConfigKey"/> instance from the given name.
        /// </summary>
        /// <param name="name">The mod-unique name of the config item.</param>
        public static implicit operator ConfigKey(string name) => new(name);

        /// <summary>
        /// Checks if two <see cref="ConfigKey"/>s are unequal.
        /// </summary>
        /// <param name="left">The first key.</param>
        /// <param name="right">The second key.</param>
        /// <returns><c>true</c> if they're considered unequal.</returns>
        public static bool operator !=(ConfigKey? left, ConfigKey? right)
            => !EqualityComparer.Equals(left!, right!);

        /// <summary>
        /// Checks if two <see cref="ConfigKey"/>s are equal.
        /// </summary>
        /// <param name="left">The first key.</param>
        /// <param name="right">The second key.</param>
        /// <returns><c>true</c> if they're considered equal.</returns>
        public static bool operator ==(ConfigKey? left, ConfigKey? right)
            => EqualityComparer.Equals(left!, right!);

        /// <summary>
        /// Checks if the given object can be considered equal to this one.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns><c>true</c> if the other object is considered equal.</returns>
        public override bool Equals(object? obj) => obj is IConfigKey otherKey && Equals(otherKey);

        /// <inheritdoc/>
        public bool Equals(IConfigKey? other) => EqualityComparer.Equals(this, other!);

        /// <inheritdoc/>
        public override int GetHashCode() => EqualityComparer.GetHashCode(this);

        /// <summary>
        /// <see cref="IEqualityComparer{T}"/> for <see cref="IConfigKey"/>s.
        /// </summary>
        private sealed class ConfigKeyEqualityComparer : IEqualityComparer<IConfigKey>
        {
            /// <inheritdoc/>
            public bool Equals(IConfigKey? x, IConfigKey? y)
            {
                if (ReferenceEquals(x, y))
                    return true;

                if (x is ITypedConfigKey typedX && y is ITypedConfigKey typedY)
                    return typedX.ValueType == typedY.ValueType && typedX.Name == typedY.Name;

                return x is not null && y is not null && x.Name == y.Name;
            }

            /// <inheritdoc/>
            public int GetHashCode(IConfigKey? obj)
            {
                if (obj is ITypedConfigKey typedKey)
                    return unchecked((31 * typedKey.ValueType.GetHashCode()) + obj.Name.GetHashCode());

                return obj?.Name.GetHashCode() ?? 0;
            }
        }
    }

    /// <inheritdoc/>
    /// <typeparam name="T">The type of this config item's value.</typeparam>
    public class ConfigKey<T> : ConfigKey, ITypedConfigKey<T>
    {
        /// <inheritdoc/>
        public Type ValueType { get; } = typeof(T);

        /// <inheritdoc/>
        public ConfigKey(string name) : base(name)
        { }

        /// <summary>
        /// Creates a new <see cref="ConfigKey{T}"/> instance from the given name.
        /// </summary>
        /// <param name="name">The mod-unique name of the config item.</param>
        public static implicit operator ConfigKey<T>(string name) => new(name);
    }

    /// <summary>
    /// Represents a name-only configuration item, which can be used to get or set the values of defining keys with the same name.
    /// </summary>
    public interface IConfigKey : IEquatable<IConfigKey>
    {
        /// <summary>
        /// Gets whether this instance defines the config item with this <see cref="Name">Name</see>.
        /// </summary>
        public bool IsDefiningKey { get; }

        /// <summary>
        /// Gets the mod-unique name of this config item.
        /// </summary>
        public string Name { get; }
    }

    /// <summary>
    /// Represents a name-only typed configuration item, which can be used to get or set the values of defining keys with the same name.
    /// </summary>
    /// <remarks>
    /// Generally <see cref="ITypedConfigKey{T}"/> should be used instead, unless the generic type gets in the way.
    /// </remarks>
    public interface ITypedConfigKey : IConfigKey
    {
        /// <summary>
        /// Get the <see cref="Type"/> of this config item's value.
        /// </summary>
        public Type ValueType { get; }
    }

    /// <summary>
    /// Represents a name-only typed configuration item, which can be used to get or set the values of defining keys with the same name.
    /// </summary>
    public interface ITypedConfigKey<T> : IConfigKey
    { }
}