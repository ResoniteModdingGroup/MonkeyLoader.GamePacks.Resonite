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
    /// <summary>su
    /// Defines the typed definition for a quantified config item.
    /// </summary>
    /// <typeparam name="T">The type of the config item's value.</typeparam>
    /// <typeparam name="TQuantity">The type of the config item's value's quantity.</typeparam>
    public interface IQuantifiedDefiningConfigKey<T, TQuantity> : IRangedDefiningKey<T>, IQuantifiedDefiningConfigKey
        where TQuantity : unmanaged, IQuantity<TQuantity>
    { }

    /// <summary>
    /// Defines the interface for a quantified <see cref="IDefiningConfigKey"/> wrapper.
    /// </summary>
    public interface IQuantifiedDefiningConfigKey : IRangedDefiningKey
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
    public class QuantifiedDefiningConfigKey<T, TQuantity> : RangedDefiningConfigKey<T>, IQuantifiedDefiningConfigKey<T, TQuantity>
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
        /// Creates a new instance of the <see cref="QuantifiedDefiningConfigKey{T, TQuantity}"/> class with the given unit and parameters.
        /// </summary>
        /// <param name="id">The mod-unique identifier of this config item. Must not be null or whitespace.</param>
        /// <param name="defaultConfiguration">The default unit configuration for this config item's value.</param>
        /// <param name="imperialConfiguration">The imperial unit configuration for this config item's value.</param>
        /// <param name="description">The human-readable description of this config item.</param>
        /// <param name="computeDefault">The function that computes a default value for this key. Otherwise <c>default(<typeparamref name="T"/>)</c> will be used.</param>
        /// <param name="min">The lower bound of the value range.</param>
        /// <param name="max">The upper bound of the value range.</param>
        /// <param name="comparer">The comparer to use to determine whether values fall into the range of this config item.</param>
        /// <param name="internalAccessOnly">If <c>true</c>, only the owning mod should have access to this config item.</param>
        /// <param name="valueValidator">The function that checks if the given value is valid for this config item. Otherwise everything will be accepted.</param>
        /// <exception cref="ArgumentNullException">When the <paramref name="id"/> is null or whitespace; or when <paramref name="min"/> or <paramref name="max"/> are null.</exception>
        /// <exception cref="NotSupportedException">When <paramref name="comparer"/> is null while <typeparamref name="T"/> is not <see cref="IComparable{T}"/></exception>
        public QuantifiedDefiningConfigKey(string id, UnitConfiguration defaultConfiguration,
            UnitConfiguration? imperialConfiguration = null, string? description = null, Func<T>? computeDefault = null,
            T? min = default, T? max = default, IComparer<T?>? comparer = null,
            bool internalAccessOnly = false, Predicate<T?>? valueValidator = null)
           : base(id, description, computeDefault, min, max, comparer, internalAccessOnly, valueValidator)
        {
            DefaultConfiguration = defaultConfiguration;
            ImperialConfiguration = imperialConfiguration;
        }
    }
}