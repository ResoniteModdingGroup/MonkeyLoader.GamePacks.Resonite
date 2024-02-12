using HarmonyLib;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite
{
    [HarmonyPatch]
    [HarmonyPatchCategory(nameof(BrotliPatcher))]
    internal sealed class BrotliPatcher : Monkey<BrotliPatcher>
    {
        public override string Name { get; } = "Brotli Fix";

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => Enumerable.Empty<IFeaturePatch>();

        [HarmonyPrefix]
        [HarmonyPatch("Brotli.NativeLibraryLoader, Brotli.Core", "GetPossibleRuntimeDirectories")]
        private static bool GetPossibleRuntimeDirectoriesPrefix(ref string[] __result)
        {
            __result = new[] { Mod.Loader.GameAssemblyPath };
            return false;
        }
    }
}