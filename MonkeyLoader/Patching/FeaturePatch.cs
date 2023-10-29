using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Patching
{
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
        /// Compares the impact of this patch to another.<br/>
        /// Patches on unrelated features or with the same <see cref="Compatibility">compatibility</see> compare equal.
        /// </summary>
        /// <inheritdoc/>
        public int CompareTo(IFeaturePatch other)
        {
            // If patches apply to the same feature, compatibility acts as tie breaker.
            // Flipped operands, because bigger compatibility = smaller impact.
            if (Feature == other.Feature)
                return other.Compatibility - Compatibility;

            return Feature.CompareTo(other.Feature);
        }

        /// <inheritdoc/>
        public bool Equals(IFeaturePatch other)
             => ReferenceEquals(other, this) || (Compatibility == other.Compatibility && Feature.Equals(other.Feature));

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is IFeaturePatch patch && Equals(patch);

        /// <inheritdoc/>
        public override int GetHashCode()
            => unchecked(Compatibility.GetHashCode() + (31 * Feature.GetHashCode()));
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