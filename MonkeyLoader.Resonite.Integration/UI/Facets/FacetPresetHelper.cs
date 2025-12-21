using FrooxEngine;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkeyLoader.Resonite.UI.Facets
{
    /// <summary>
    /// Contains some helper data to work with <see cref="FacetPreset"/>s.
    /// </summary>
    public static class FacetPresetHelper
    {
        /// <summary>
        /// Gets a set containing all concrete <see cref="FacetPreset"/> <see cref="Type"/>s.
        /// </summary>
        public static FrozenSet<Type> AllFacetPresetTypes { get; }

        /// <summary>
        /// Gets the <see cref="FacetPreset"/> <see cref="Type"/>.
        /// </summary>
        public static Type FacetPresetType { get; } = typeof(FacetPreset);

        /// <summary>
        /// Gets a set containing all concrete <see cref="FacetPreset"/> <see cref="Type"/>s
        /// that are not derived from <see cref="TemplateFacetPreset"/>.
        /// </summary>
        public static FrozenSet<Type> NonTemplateFacetPresetTypes { get; }

        /// <summary>
        /// Gets the <see cref="TemplateFacetPreset"/> <see cref="Type"/>.
        /// </summary>
        public static Type TemplateFacetPresetType { get; } = typeof(TemplateFacetPreset);

        /// <summary>
        /// Gets a set containing all concrete <see cref="TemplateFacetPreset"/> <see cref="Type"/>s.
        /// </summary>
        public static FrozenSet<Type> TemplateFacetPresetTypes { get; }

        static FacetPresetHelper()
        {
            AllFacetPresetTypes = GlobalTypeRegistry.DataModelAssemblies
                .SelectMany(registry => registry.Types)
                .Where(static type => !type.IsAbstract && FacetPresetType.IsAssignableFrom(type))
                .ToFrozenSet();

            TemplateFacetPresetTypes = AllFacetPresetTypes
                .Where(TemplateFacetPresetType.IsAssignableFrom)
                .ToFrozenSet();

            NonTemplateFacetPresetTypes = AllFacetPresetTypes
                .Where(type => !TemplateFacetPresetTypes.Contains(type))
                .ToFrozenSet();
        }
    }
}