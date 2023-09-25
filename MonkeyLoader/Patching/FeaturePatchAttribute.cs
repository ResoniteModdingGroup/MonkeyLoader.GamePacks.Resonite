using MonkeyLoader.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Patching
{
    /// <summary>
    /// Specifies which <see cref="GameFeature"/>(s) a patcher affects.
    /// </summary>
    /// <typeparam name="TFeature">The game feature's definition type.</typeparam>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class FeaturePatchAttribute<TFeature> : MonkeyLoaderAttribute where TFeature : GameFeature, new()
    {
        /// <summary>
        /// Gets how severely the feature is affected by the patch.
        /// </summary>
        public PatchSeverity PatchSeverity { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeaturePatchAttribute{TFeature}"/> attribute
        /// with the given <see cref="Patching.PatchSeverity"/>.
        /// </summary>
        /// <param name="patchSeverity">How severely the feature is affected by the patch.</param>
        public FeaturePatchAttribute(PatchSeverity patchSeverity)
        {
            PatchSeverity = patchSeverity;
        }
    }
}