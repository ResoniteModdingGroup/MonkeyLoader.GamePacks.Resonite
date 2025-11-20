using FrooxEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.UI
{
    // This is necessary because of: https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/4701
    // Otherwise, most users wouldn't have the template for the resettable section.
    [HarmonyPatchCategory(nameof(ForceUpdateFacets))]
    [HarmonyPatch(typeof(FacetPreset), nameof(FacetPreset.ForceUpgrade), MethodType.Getter)]
    internal sealed class ForceUpdateFacets : ResoniteMonkey<ForceUpdateFacets>
    {
        public override bool CanBeDisabled => true;

        private static bool Prefix(out bool __result)
        {
            __result = Enabled;
            return !Enabled;
        }
    }
}