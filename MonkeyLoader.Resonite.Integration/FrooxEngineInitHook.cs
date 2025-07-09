using HarmonyLib;
using MonkeyLoader.Patching;
using System.Reflection;

namespace MonkeyLoader.Resonite
{
    [HarmonyPatchCategory(nameof(FrooxEngineInitHook))]
    [HarmonyPatch(typeof(Assembly), nameof(Assembly.LoadFrom), typeof(string))]
    public class FrooxEngineInitHook : Monkey<FrooxEngineInitHook>
    {
        public override string Name { get; } = "Init Fix";
        
        static bool Prefix(ref Assembly __result, string assemblyFile)
        {
            if (assemblyFile.EndsWith("ProtoFlux.Nodes.FrooxEngine.dll"))
                return false;

            if (assemblyFile.EndsWith("ProtoFluxBindings.dll"))
                return false;

            return true;
        }
    }
}