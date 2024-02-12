using HarmonyLib;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite
{
    [HarmonyPatchCategory(nameof(InstantResoniteLog))]
    [HarmonyPatch("FrooxEngineBootstrap+<>c, Assembly-CSharp", "<Start>b__10_1")]
    internal class InstantResoniteLog : Monkey<InstantResoniteLog>
    {
        public override string Name { get; } = "Instant Resonite Log";

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => Enumerable.Empty<IFeaturePatch>();

        [HarmonyPostfix]
        private static void WriteLinePostfix()
            => FrooxEngineBootstrap.LogStream.Flush();
    }
}