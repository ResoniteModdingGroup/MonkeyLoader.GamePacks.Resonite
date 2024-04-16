# Introduction to MonkeyLoader

The [MonkeyLoader Mod Template for Resonite](https://github.com/ResoniteModdingGroup/MonkeyLoader.ModTemplate)
provides a sample patcher, which already shows the general creation pattern for [Monkeys](xref:MonkeyLoader.Patching.Monkey\`1).

```csharp
using FrooxEngine.ProtoFlux;
using HarmonyLib;
using MonkeyLoader.Patching;
using MonkeyLoader.Resonite;
using MonkeyLoader.Resonite.Features.FrooxEngine;
using System.Collections.Generic;

namespace MonkeyLoader.ModTemplate
{
    [HarmonyPatchCategory(nameof(BasicPatcher))]
    [HarmonyPatch(typeof(ProtoFluxTool), nameof(ProtoFluxTool.OnAttach))]
    internal class BasicPatcher : ResoniteMonkey<BasicPatcher>
    {
        protected override IEnumerable<IFeaturePatch> GetFeaturePatches()
        {
            yield return new FeaturePatch<ProtofluxTool>(PatchCompatibility.HookOnly);
        }

        private static void Postfix()
        {
            Logger.Info(() => "Postfix for ProtoFluxTool.OnAttach()!");
        }
    }
}
```

Every [Monkey](xref:MonkeyLoader.Resonite.ResoniteMonkey\`1) acts as a compartmentalized patcher.
If the patching of one fails, the others are not affected by it, unless they specifically react to it.


## Patching

<xref:MonkeyLoader.MonkeyLoader> relies on Harmony for the runtime patching of code.
For an introduction to that, refer to the [Harmony Introduction](https://harmony.pardeike.net/articles/intro.html).

For your own mods, it is essential, that your class is decorated with the `[HarmonyPatch(typeof(TargetType))]`
and `[HarmonyPatchCategory(nameof(YourPatcher))]` attributes, if you want them to be automatically executed.
You can also have additional patch classes connected to a Monkey, as long as they also have these attributes.

Your patch methods can further be decorated with extra `[HarmonyPatch]` attributes,
as well as `[HarmonyPrefix]`, `[HarmonyPostfix]`, and [others](https://harmony.pardeike.net/articles/annotations.html).
This allows patching multiple methods of the same TargetType or even methods of different TargetTypes in one class.


## Hot-Reloading

If you follow this pattern, or manually override the <xref:MonkeyLoader.Resonite.ResoniteMonkey`1.OnEngineReady>
and <xref:MonkeyLoader.Patching.MonkeyBase.OnShutdown(System.Boolean)> methods for it,
Monkeys are automatically setup for hot-reloading.
This facilitates fast iteration by allowing code changes to be applied ingame using only a rebuild,
rather than having to restart the whole game as well.


## Feature Patches

The <xref:MonkeyLoader.Patching.MonkeyBase.GetFeaturePatches> method is there to let
MonkeyLoader know which of a game's features your patcher affects how much.
It is not strictly necessary to provide them, but especially for higher impact patches,
it ensures that your patcher runs before any less impactfull ones.
If you don't want to or can't provide the features, simply implement it as:
`GetFeaturePatches() => Enumerable.Empty<IFeaturePatch>();`.