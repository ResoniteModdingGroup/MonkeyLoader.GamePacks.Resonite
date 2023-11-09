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
        private static readonly Type _featureType = typeof(Feature);
        private readonly int _depth;
        private readonly Lazy<bool> _multipleAssemblies;
        private readonly Type _type;

        /// <summary>
        /// Gets a <see cref="Feature"/>-comparer, that sorts smaller / deeper features first.
        /// </summary>
        public static IComparer<Feature> AscendingComparer { get; } = new FeatureComparer();

        /// <summary>
        /// Gets a <see cref="Feature"/>-comparer, that sorts larger / shallower features first.
        /// </summary>
        public static IComparer<Feature> DescendingComparer { get; } = new FeatureComparer(false);

        /// <summary>
        /// Gets an equality comparer that uses the feature's <see cref="Type"/>.
        /// </summary>
        public static IEqualityComparer<Feature> EqualityComparer { get; } = new FeatureComparer();

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
            _depth = GetDepth(_type);
            _multipleAssemblies = new Lazy<bool>(() => Assemblies.Count() > 1);
        }

        /// <summary>
        /// Checks if the left feature is unequal to the right one.
        /// </summary>
        /// <param name="left">The first feature.</param>
        /// <param name="right">The second feature.</param>
        /// <returns>Whether the features are unequal.</returns>
        public static bool operator !=(Feature left, Feature right)
            => !EqualityComparer.Equals(left, right);

        /// <summary>
        /// Checks if the left feature sorts smaller than the right one.
        /// </summary>
        /// <param name="left">The first feature.</param>
        /// <param name="right">The second feature.</param>
        /// <returns>Whether the left features comes first in the sort order.</returns>
        public static bool operator <(Feature left, Feature right)
            => AscendingComparer.Compare(left, right) < 0;

        /// <summary>
        /// Checks if the left feature is equal to the right one.
        /// </summary>
        /// <param name="left">The first feature.</param>
        /// <param name="right">The second feature.</param>
        /// <returns>Whether the features are equal.</returns>
        public static bool operator ==(Feature left, Feature right)
            => EqualityComparer.Equals(left, right);

        /// <summary>
        /// Checks if the left feature sorts bigger than the right one.
        /// </summary>
        /// <param name="left">The first feature.</param>
        /// <param name="right">The second feature.</param>
        /// <returns>Whether the right features comes first in the sort order.</returns>
        public static bool operator >(Feature left, Feature right)
            => left.CompareTo(right) > 0;

        /// <inheritdoc/>
        public int CompareTo(Feature other) => AscendingComparer.Compare(this, other);

        /// <inheritdoc/>
        public bool Equals(Feature other) => EqualityComparer.Equals(this, other);

        /// <inheritdoc/>
        public override bool Equals(object obj)
            => obj is Feature feature && Equals(feature);

        /// <inheritdoc/>
        public override int GetHashCode() => _type.GetHashCode();

        private static int GetDepth(Type type)
        {
            var depth = 0;

            while (type != _featureType)
            {
                depth++;
                type = type.BaseType;
            }

            return depth;
        }

        private sealed class FeatureComparer : IEqualityComparer<Feature>, IComparer<Feature>
        {
            private readonly int _factor;

            public FeatureComparer(bool ascending = true)
            {
                _factor = ascending ? 1 : -1;
            }

            public int Compare(Feature x, Feature y) => _factor * (x._depth - y._depth);

            public bool Equals(Feature x, Feature y) => ReferenceEquals(x, y) || x._type == y._type;

            public int GetHashCode(Feature obj) => obj._type.GetHashCode();
        }
    }
}