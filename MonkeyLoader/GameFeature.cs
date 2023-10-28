using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader
{
    /// <summary>
    /// Base type for all feature definitions.
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
    public abstract class GameFeature : IEquatable<GameFeature>, IComparable<GameFeature>
    {
        private static readonly Dictionary<Type, GameFeature> _gameFeatures = new();
        private static readonly Dictionary<TypeSet, int> _knownComparisons = new();
        private readonly Type _type;

        /// <summary>
        /// Gets the description of the feature.
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Gets the name of the feature.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Creates a new game feature instance.
        /// </summary>
        protected GameFeature()
        {
            _type = GetType();
        }

        /// <summary>
        /// Gets the cached instance of the given <typeparamref name="TFeature">feature</typeparamref>.
        /// </summary>
        /// <typeparam name="TFeature">The type of the feature.</typeparam>
        /// <returns>The cached instance of the feature.</returns>
        public static TFeature GetInstance<TFeature>() where TFeature : GameFeature, new()
        {
            var featureType = typeof(TFeature);

            if (!_gameFeatures.TryGetValue(featureType, out var feature))
            {
                feature = new TFeature();
                _gameFeatures.Add(featureType, feature);
            }

            return (TFeature)feature;
        }

        /// <summary>
        /// Checks if the left feature is unequal to the right one.
        /// </summary>
        /// <param name="left">The first feature.</param>
        /// <param name="right">The second feature.</param>
        /// <returns>Whether the features are unequal.</returns>
        public static bool operator !=(GameFeature left, GameFeature right) => !(left == right);

        /// <summary>
        /// Checks if the left feature comes after the right one in the sort order.
        /// </summary>
        /// <param name="left">The first feature.</param>
        /// <param name="right">The second feature.</param>
        /// <returns>Whether the left features comes first in the sort order.</returns>
        public static bool operator <(GameFeature left, GameFeature right)
            => left.CompareTo(right) < 0;

        /// <summary>
        /// Checks if the left feature is equal to the right one.
        /// </summary>
        /// <param name="left">The first feature.</param>
        /// <param name="right">The second feature.</param>
        /// <returns>Whether the features are equal.</returns>
        public static bool operator ==(GameFeature left, GameFeature right)
            => ReferenceEquals(left, right) || left._type == right._type;

        /// <summary>
        /// Checks if the right feature comes after the left one in the sort order.
        /// </summary>
        /// <param name="left">The first feature.</param>
        /// <param name="right">The second feature.</param>
        /// <returns>Whether the right features comes first in the sort order.</returns>
        public static bool operator >(GameFeature left, GameFeature right)
            => left.CompareTo(right) > 0;

        /// <inheritdoc/>
        public int CompareTo(GameFeature other)
        {
            var types = new TypeSet(_type, other._type);

            if (!_knownComparisons.TryGetValue(types, out var comparison))
            {
                // Have to check equal first, because both assignable directions are true otherwise
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
        public bool Equals(GameFeature other) => other == this;

        /// <inheritdoc/>
        public override bool Equals(object obj)
            => obj is GameFeature feature && Equals(feature);

        /// <inheritdoc/>
        public override int GetHashCode() => _type.GetHashCode();
    }
}