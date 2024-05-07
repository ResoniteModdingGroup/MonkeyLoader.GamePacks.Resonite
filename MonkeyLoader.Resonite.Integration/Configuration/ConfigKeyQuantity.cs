using Elements.Quantity;
using FrooxEngine;
using MonkeyLoader.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.Configuration
{
    /// <summary>
    /// Represents the typed definition for a quantified config item.
    /// </summary>
    /// <typeparam name="T">The type of the config item's value.</typeparam>
    /// <typeparam name="TQuantity">The type of the config item's value's quantity.</typeparam>
    public class ConfigKeyQuantity<T, TQuantity> : ConfigKeyRange<T>, IConfigKeyQuantity<T>
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
        /// Creates a new instance of the <see cref="ConfigKeyQuantity{T, TQuantity}"/> component with the given unit and parameters.
        /// </summary>
        /// <param name="defaultConfiguration">The default unit configuration for this config item's value.</param>
        /// <param name="imperialConfiguration">The imperial unit configuration for this config item's value.</param>
        /// <param name="min">The lower bound of the value range.</param>
        /// <param name="max">The upper bound of the value range.</param>
        /// <param name="comparer">The comparer to use to determine whether values fall into the range of this config item.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="min"/> or <paramref name="max"/> are null.</exception>
        /// <exception cref="NotSupportedException">When <paramref name="comparer"/> is null while <typeparamref name="T"/> is not <see cref="IComparable{T}"/></exception>
        public ConfigKeyQuantity(UnitConfiguration defaultConfiguration, UnitConfiguration? imperialConfiguration = null,
                T? min = default, T? max = default, IComparer<T?>? comparer = null)
           : base(min, max, comparer)
        {
            DefaultConfiguration = defaultConfiguration;
            ImperialConfiguration = imperialConfiguration;
        }
    }

    /// <summary>su
    /// Defines the typed definition for a quantified config item.
    /// </summary>
    /// <typeparam name="T">The type of the config item's value.</typeparam>
    public interface IConfigKeyQuantity<T> : IConfigKeyRange<T>
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
}