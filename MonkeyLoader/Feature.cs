using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader
{
    /// <summary>
    /// Contains static access to <see cref="Feature"/> instances and their properties.
    /// </summary>
    /// <typeparam name="TFeature">The feature to get the properties of.</typeparam>
    public static class Feature<TFeature> where TFeature : Feature, new()
    {
        /// <summary>
        /// Gets the names of all assemblies that this feature is implemented in.
        /// </summary>
        public static IEnumerable<AssemblyName> Assemblies => Instance.Assemblies;

        /// <summary>
        /// Gets the name of the (main) assembly that this feature is implemented in.
        /// </summary>
        public static AssemblyName Assembly => Instance.Assembly;

        /// <summary>
        /// Gets the description of the feature.
        /// </summary>
        public static string Description => Instance.Description;

        /// <summary>
        /// Gets the cached instance of the feature.
        /// </summary>
        public static TFeature Instance { get; } = new TFeature();

        /// <summary>
        /// Gets the name of the feature.
        /// </summary>
        public static string Name => Instance.Name;

        /// <summary>
        /// Gets whether this feature is spread over multiple assemblies, i.e. whether
        /// <see cref="Assemblies">Assemblies</see>.<see cref="Enumerable.Count{TSource}(IEnumerable{TSource})">Count</see>() is <c>&gt; 1</c>.
        /// </summary>
        public static bool SpreadOverMultipleAssemblies => Instance.SpreadOverMultipleAssemblies;
    }

    /// <summary>
    /// Base type for all feature definitions.<br/>
    /// Features can be compared / sorted - earlier in sort order means smaller.
    /// If features aren't super- or sub-features of eachother, they compare for the same position.
    /// </summary>
    /// <remarks>
    /// These provide a non-exhaustive list of different features.<br/>
    /// Game data packs should strive to provide at least the most common ones,
    /// but mods can also add their own (sub-)features.
    /// <para/>
    /// Game data packs and mods should use inheritance to indicate sub-features.<br/>
    /// For example <c>ControllerInput : InputSystem</c> would mean that
    /// <c>ControllerInput</c> is a sub-feature of the <c>InputSystem</c>.
    /// </remarks>
    public abstract class Feature : IEquatable<Feature>, IComparable<Feature>
    {
        private static readonly Dictionary<TypeSet, int> _knownComparisons = new();
        private readonly Lazy<bool> _multipleAssemblies;
        private readonly Type _type;

        /// <summary>
        /// Gets the names of all assemblies that this feature is implemented in.
        /// </summary>
        /// <remarks>
        /// Should usually just be one assembly per feature, defined using <see cref="Assembly"/>.<br/>
        /// Defaults to returning just that single assembly's name.
        /// </remarks>
        public virtual IEnumerable<AssemblyName> Assemblies => Assembly.Yield();

        /// <summary>
        /// Gets the name of the (main) assembly that this feature is implemented in.
        /// </summary>
        /// <remarks>
        /// If a feature is spread over multiple assemblies, override <see cref="Assemblies"/>.
        /// </remarks>
        public abstract AssemblyName Assembly { get; }

        /// <summary>
        /// Gets the description of the feature.
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Gets the name of the feature.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets whether this feature is spread over multiple assemblies, i.e. whether
        /// <see cref="Assemblies">Assemblies</see>.<see cref="Enumerable.Count{TSource}(IEnumerable{TSource})">Count</see>() is <c>&gt; 1</c>.
        /// </summary>
        public bool SpreadOverMultipleAssemblies => _multipleAssemblies.Value;

        /// <summary>
        /// Creates a new game feature instance.
        /// </summary>
        protected Feature()
        {
            _type = GetType();
            _multipleAssemblies = new Lazy<bool>(() => Assemblies.Count() > 1);
        }

        /// <summary>
        /// Checks if the left feature is unequal to the right one.
        /// </summary>
        /// <param name="left">The first feature.</param>
        /// <param name="right">The second feature.</param>
        /// <returns>Whether the features are unequal.</returns>
        public static bool operator !=(Feature left, Feature right) => !(left == right);

        /// <summary>
        /// Checks if the left feature comes after the right one in the sort order.
        /// </summary>
        /// <param name="left">The first feature.</param>
        /// <param name="right">The second feature.</param>
        /// <returns>Whether the left features comes first in the sort order.</returns>
        public static bool operator <(Feature left, Feature right)
            => left.CompareTo(right) < 0;

        /// <summary>
        /// Checks if the left feature is equal to the right one.
        /// </summary>
        /// <param name="left">The first feature.</param>
        /// <param name="right">The second feature.</param>
        /// <returns>Whether the features are equal.</returns>
        public static bool operator ==(Feature left, Feature right)
            => ReferenceEquals(left, right) || left._type == right._type;

        /// <summary>
        /// Checks if the right feature comes after the left one in the sort order.
        /// </summary>
        /// <param name="left">The first feature.</param>
        /// <param name="right">The second feature.</param>
        /// <returns>Whether the right features comes first in the sort order.</returns>
        public static bool operator >(Feature left, Feature right)
            => left.CompareTo(right) > 0;

        /// <inheritdoc/>
        public int CompareTo(Feature other)
        {
            var types = new TypeSet(_type, other._type);

            if (!_knownComparisons.TryGetValue(types, out var comparison))
            {
                // Have to check equal first, because both assignable directions are true then
                if (_type == other._type)
                    comparison = 0;
                // Maybe the other derives from this?
                else if (_type.IsAssignableFrom(other._type))
                    comparison = 1;
                // Maybe this derives from the other?
                else if (other._type.IsAssignableFrom(_type))
                    comparison = -1;
                // Independent
                else
                    comparison = 0;

                _knownComparisons.Add(types, comparison);
            }

            return comparison;
        }

        /// <inheritdoc/>
        public bool Equals(Feature other) => other == this;

        /// <inheritdoc/>
        public override bool Equals(object obj)
            => obj is Feature feature && Equals(feature);

        /// <inheritdoc/>
        public override int GetHashCode() => _type.GetHashCode();
    }
}