using EnumerableToolkit;
using HarmonyLib;
using MonkeyLoader.Patching;
using MonkeyLoader.Resonite.DataFeeds;

namespace MonkeyLoader.Resonite
{
    [HarmonyPatch]
    [HarmonyPatchCategory(nameof(BrotliPatcher))]
    internal sealed class BrotliPatcher : Monkey<BrotliPatcher>, ISubgroupedDataFeedItem
    {
        public override string Name { get; } = "Brotli Fix";

        public Sequence<string> SubgroupPath => SubgroupDefinitions.GamePack;

        [HarmonyPrefix]
        [HarmonyPatch("Brotli.NativeLibraryLoader, Brotli.Core", "GetPossibleRuntimeDirectories")]
        private static bool GetPossibleRuntimeDirectoriesPrefix(ref string[] __result)
        {
            __result = [MonkeyLoader.GameAssemblyPath];
            return false;
        }
    }
}