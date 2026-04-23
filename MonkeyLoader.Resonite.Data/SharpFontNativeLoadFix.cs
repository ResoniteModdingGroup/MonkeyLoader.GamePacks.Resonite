using MonkeyLoader.Patching;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyLoader.Resonite
{
    internal sealed class SharpFontNativeLoadFix : EarlyMonkey<SharpFontNativeLoadFix>
    {
        protected override IEnumerable<PrePatchTarget> GetPrePatchTargets()
        {
            yield return new PrePatchTarget(new("SharpFont"), "SharpFont.FT");
            yield return new PrePatchTarget(new("SoundFlow"), "SoundFlow.Backends.MiniAudio.Native");
        }

        protected override bool Patch(PatchJob patchJob)
        {
            var type = patchJob.Types.First();
            var staticConstructor = type.GetStaticConstructor();

            var processor = staticConstructor.Body.GetILProcessor();
            processor.Clear();
            processor.Append(Instruction.Create(OpCodes.Ret));

            patchJob.Changes = true;
            return true;
        }
    }
}