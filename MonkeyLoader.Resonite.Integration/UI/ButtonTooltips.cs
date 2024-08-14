using FrooxEngine.UIX;
using HarmonyLib;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.UI
{
    /// <summary>
    /// Adds tooltips to <see cref="Button"/>s,
    /// similar to how they were originally done for Neos by Psychpsyo.<br/>
    /// <see href="https://github.com/Psychpsyo/Tooltippery"/>
    /// </summary>
    [HarmonyPatch(typeof(Button))]
    [HarmonyPatchCategory(nameof(ButtonTooltips))]
    internal sealed class ButtonTooltips : ConfiguredResoniteMonkey<ButtonTooltips, ButtonTooltipConfig>
    {
        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => [];
    }
}