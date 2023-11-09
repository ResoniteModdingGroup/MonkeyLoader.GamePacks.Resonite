using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Patching
{
    /// <summary>
    /// Contains (equality) comparers for <see cref="IFeaturePatch"/>es / <see cref="FeaturePatch{TFeature}"/> instances.
    /// </summary>
    public static class FeaturePatch
    {
        /// <summary>
        /// Gets an <see cref="IFeaturePatch"/>-comparer, that sorts patches with lower impact first.
        /// </summary>
        public static IComparer<IFeaturePatch> AscendingComparer { get; } = new FeaturePatchComparer();

        /// <summary>
        /// Gets an <see cref="IFeaturePatch"/>-comparer, that sorts patches with higher impact first.
        /// </summary>
        public static IComparer<IFeaturePatch> DescendingComparer { get; } = new FeaturePatchComparer(false);

        /// <summary>
        /// Gets an equality comparer that uses the patch's feature and impact.
        /// </summary>
        public static IEqualityComparer<IFeaturePatch> EqualityComparer { get; } = new FeaturePatchComparer();

        private sealed class FeaturePatchComparer : IEqualityComparer<IFeaturePatch>, IComparer<IFeaturePatch>
        {
            private readonly int _factor;

            public FeaturePatchComparer(bool ascending = true)
            {
                _factor = ascending ? 1 : -1;
            }

            public int Compare(IFeaturePatch x, IFeaturePatch y)
            {
                var featureComparison = _factor * x.Feature.CompareTo(y.Feature);

                // If patched features compare the same, compatibility acts as tie breaker.
                // Flipped operands, because bigger compatibility = smaller impact.
                if (featureComparison == 0)
                    return _factor * (x.Compatibility - y.Compatibility);

                return featureComparison;
            }

            public bool Equals(IFeaturePatch x, IFeaturePatch y)
                => ReferenceEquals(x, y) || (x.Compatibility == y.Compatibility && x.Feature.Equals(y.Feature));

            public int GetHashCode(IFeaturePatch obj)
                => unchecked(obj.Compatibility.GetHashCode() + (31 * obj.Feature.GetHashCode()));
        }
    }

    /// <summary>
    /// Specifies how much a (pre-)patcher affects a particular <see cref="global::MonkeyLoader.Feature"/>.
    /// </summary>
    /// <typeparam name="TFeature">The patched game feature.</typeparam>
    public sealed class FeaturePatch<TFeature> : IFeaturePatch,
        IEquatable<IFeaturePatch>, IComparable<IFeaturePatch>
        where TFeature : Feature, new()
    {
        /// <inheritdoc/>
        public PatchCompatibility Compatibility { get; }

        /// <summary>
        /// Gets the affected feature.
        /// </summary>
        public TFeature Feature { get; }

        Feature IFeaturePatch.Feature => Feature;

        /// <summary>
        /// Creates a new feature patch with the given <see cref="PatchCompatibility"/>.
        /// </summary>
        /// <param name="patchCompatibility">How compatible the patch is with others.</param>
        public FeaturePatch(PatchCompatibility patchCompatibility)
        {
            Compatibility = patchCompatibility;
            Feature = Feature<TFeature>.Instance;
        }

        /// <summary>
        /// Compares the impact of this patch to another.
        /// </summary>
        /// <inheritdoc/>
        public int CompareTo(IFeaturePatch other)
            => FeaturePatch.AscendingComparer.Compare(this, other);

        /// <inheritdoc/>
        public bool Equals(IFeaturePatch other)
             => FeaturePatch.EqualityComparer.Equals(this, other);

        /// <inheritdoc/>
        public override bool Equals(object obj)
            => obj is IFeaturePatch patch && Equals(patch);

        /// <inheritdoc/>
        public override int GetHashCode()
            => FeaturePatch.EqualityComparer.GetHashCode(this);
    }

    /// <summary>
    /// Specifies how much a (pre-)patcher affects a particular <see cref="global::MonkeyLoader.Feature"/>.
    /// </summary>
    public interface IFeaturePatch : IEquatable<IFeaturePatch>, IComparable<IFeaturePatch>
    {
        /// <summary>
        /// Gets how compatible the patch is with others.
        /// </summary>
        public PatchCompatibility Compatibility { get; }

        /// <summary>
        /// Gets the affected feature.
        /// </summary>
        public Feature Feature { get; }
    }
}