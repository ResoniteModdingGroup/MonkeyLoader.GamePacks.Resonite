using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.DataFeeds.Settings
{
    // This is necessary because of: https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/4701
    // Otherwise, most users wouldn't have the template for the resettable section.
    [HarmonyPatchCategory(nameof(ForceUpdateSettingsFacet))]
    [HarmonyPatch(typeof(SettingsFacetPreset), nameof(SettingsFacetPreset.Version), MethodType.Getter)]
    internal sealed class ForceUpdateSettingsFacet : ResoniteMonkey<ForceUpdateSettingsFacet>
    {
        private static int Postfix(int __result)
            => MathX.Max(__result, 6);
    }
}