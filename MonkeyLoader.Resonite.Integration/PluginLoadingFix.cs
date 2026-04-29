using EnumerableToolkit;
using FrooxEngine.Weaver;
using HarmonyLib;
using MonkeyLoader.Patching;
using MonkeyLoader.Resonite.DataFeeds;
using Mono.Cecil;
using System.Reflection.Emit;

namespace MonkeyLoader.Resonite
{
    [HarmonyPatchCategory(nameof(PluginLoadingFix))]
    [HarmonyPatch(typeof(AssemblyPostProcessor), nameof(AssemblyPostProcessor.Process),
        [typeof(string), typeof(string), typeof(string)], [ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal])]
    internal sealed class PluginLoadingFix : Monkey<PluginLoadingFix>, ISubgroupedDataFeedItem
    {
        public Sequence<string> SubgroupPath => SubgroupDefinitions.GamePack;

        private static void HandleAssemblyResolver(ReaderParameters readerParameters, DefaultAssemblyResolver assemblyResolver)
        {
            assemblyResolver.AddSearchDirectory(MonkeyLoader.GameAssemblyPath);
            readerParameters.AssemblyResolver = assemblyResolver;
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var handleAssemblyResolverMethod = AccessTools.Method(typeof(PluginLoadingFix), nameof(HandleAssemblyResolver));
            var setAssemblyResolverMethod = AccessTools.PropertySetter(typeof(ReaderParameters), nameof(ReaderParameters.AssemblyResolver));

            foreach (var instruction in instructions)
            {
                if (instruction.Calls(setAssemblyResolverMethod))
                    yield return new CodeInstruction(OpCodes.Call, handleAssemblyResolverMethod);
                else
                    yield return instruction;
            }
        }
    }
}