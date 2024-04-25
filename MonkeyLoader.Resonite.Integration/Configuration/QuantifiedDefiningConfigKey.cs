using Elements.Quantity;
using FrooxEngine;
using MonkeyLoader.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.Configuration
{
    /// <summary>
    /// Defines the typed definition for a quantified config item.
    /// </summary>
    /// <typeparam name="T">The type of the config item's value.</typeparam>
    /// <typeparam name="TQuantity">The type of the config item's value's quantity.</typeparam>
    public interface IQuantifiedDefiningConfigKey<T, TQuantity> : IDefiningConfigKeyWrapper<T>, IQuantifiedDefiningConfigKey
        where TQuantity : unmanaged, IQuantity<TQuantity>
    { }

    /// <summary>
    /// Defines the interface for a quantified <see cref="IDefiningConfigKey"/> wrapper.
    /// </summary>
    public interface IQuantifiedDefiningConfigKey : IDefiningConfigKeyWrapper
    {
        /// <summary>
        /// Gets the default unit configuration for this config item's value.
        /// </summary>
        public UnitConfiguration DefaultConfiguration { get; }

        /// <summary>
        /// Gets whether this config item's value has an imperial configuration.
        /// </summary>
        /// <value>
        /// <c>true</c>, if <see cref="ImperialConfiguration">ImperialConfiguration</see> is not <c>null</c>; otherwise, <c>false</c>.
        /// </value>
        [MemberNotNullWhen(true, nameof(ImperialConfiguration))]
        public bool HasImperialConfiguration { get; }

        /// <summary>
        /// Gets the imperial unit configuration for this config item's value.
        /// </summary>
        public UnitConfiguration? ImperialConfiguration { get; }

        /// <summary>
        /// Gets the <see cref="Type"/> of this config item's value's quantity.
        /// </summary>
        public Type QuantityType { get; }
    }

    /// <summary>
    /// Represents the typed definition for a quantified config item.
    /// </summary>
    /// <typeparam name="T">The type of the config item's value.</typeparam>
    /// <typeparam name="TQuantity">The type of the config item's value's quantity.</typeparam>
    public class QuantifiedDefiningConfigKey<T, TQuantity> : DefiningConfigKeyWrapper<T>, IQuantifiedDefiningConfigKey<T, TQuantity>
        where TQuantity : unmanaged, IQuantity<TQuantity>
    {
        /// <inheritdoc/>
        public UnitConfiguration DefaultConfiguration { get; }

        /// <inheritdoc/>
        [MemberNotNullWhen(true, nameof(ImperialConfiguration))]
        public bool HasImperialConfiguration => ImperialConfiguration is not null;

        /// <inheritdoc/>
        public UnitConfiguration? ImperialConfiguration { get; }

        /// <inheritdoc/>
        public Type QuantityType { get; } = typeof(TQuantity);

        /// <summary>
        /// Wraps the given <see cref="IDefiningConfigKey{T}"/> with the given unit configurations.
        /// </summary>
        /// <param name="definingKey">The defining key to wrap.</param>
        /// <param name="defaultConfiguration">The default unit configuration for this config item's value.</param>
        /// <param name="imperialConfiguration">The imperial unit configuration for this config item's value.</param>
        public QuantifiedDefiningConfigKey(IDefiningConfigKey<T> definingKey,
            UnitConfiguration defaultConfiguration, UnitConfiguration? imperialConfiguration = null)
            : base(definingKey)
        {
            DefaultConfiguration = defaultConfiguration;
            ImperialConfiguration = imperialConfiguration;
        }
    }
}