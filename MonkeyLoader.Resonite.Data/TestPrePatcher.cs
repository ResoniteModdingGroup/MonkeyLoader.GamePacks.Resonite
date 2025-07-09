using MonkeyLoader.Patching;
using MonkeyLoader.Resonite.Features.FrooxEngine;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite
{
    // .NET Core enforces access checks, so this needs to be public for HelloMethod() to be callable from FrooxEngine
    public sealed class TestPrePatcher : EarlyMonkey<TestPrePatcher>
    {
        public override string Name { get; } = "Test";

        public static void HelloMethod()
            => Logger.Info(() => $"Hello from pre-patched-in FrooxEngine.Engine static constructor!");

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches()
        {
            yield return new FeaturePatch<FrooxEngine>(PatchCompatibility.HookOnly);
        }

        protected override IEnumerable<PrePatchTarget> GetPrePatchTargets()
        {
            yield return new PrePatchTarget(Feature<FrooxEngine>.Assembly, "FrooxEngine.Engine");
        }

        protected override bool Patch(PatchJob patchJob)
        {
            var engine = patchJob.Types.First();
            var engineCCtor = engine.GetStaticConstructor();

            var processor = engineCCtor.Body.GetILProcessor(); // using MonoMod.Utils; is important for this to work v
            processor.InsertBefore(engineCCtor.Body.Instructions.First(), processor.Create(OpCodes.Call, typeof(TestPrePatcher).GetMethod(nameof(HelloMethod))));

            patchJob.Changes = true;
            return true;
        }
    }
}