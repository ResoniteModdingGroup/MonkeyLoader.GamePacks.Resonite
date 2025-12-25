using HarmonyLib;
using MonkeyLoader.Patching;
using System.Reflection;

namespace MonkeyLoader.Resonite
{
    [HarmonyPatchCategory(nameof(FrooxEngineInitHook))]
    [HarmonyPatch(typeof(Assembly), nameof(Assembly.LoadFrom), typeof(string))]
    internal sealed class FrooxEngineInitHook : Monkey<FrooxEngineInitHook>
    {
        /// <inheritdoc/>
        public override string Name { get; } = "Init Fix";

        private static bool Prefix(string assemblyFile)
        {
            if (assemblyFile.EndsWith("ProtoFlux.Nodes.FrooxEngine.dll"))
                return false;

            if (assemblyFile.EndsWith("ProtoFluxBindings.dll"))
                return false;

            return true;
        }
    }
}