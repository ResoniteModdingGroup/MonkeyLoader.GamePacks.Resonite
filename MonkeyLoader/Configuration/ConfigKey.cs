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
    }

    /// <inheritdoc/>
    /// <typeparam name="T">The type of this config item's value.</typeparam>
    public class ConfigKey<T> : ConfigKey, IConfigKey<T>
    {
        /// <inheritdoc/>
        public Type ValueType { get; } = typeof(T);

        /// <inheritdoc/>
        public ConfigKey(string name) : base(name)
        { }
    }

    /// <summary>
    /// Represents a name-only configuration item, which can be used to get or set the values of defining keys with the same name.
    /// </summary>
    public interface IConfigKey
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
    public interface IConfigKey<T> : IConfigKey
    {
        /// <summary>
        /// Get the <see cref="Type"/> of this config item's value.
        /// </summary>
        public Type ValueType { get; }
    }
}