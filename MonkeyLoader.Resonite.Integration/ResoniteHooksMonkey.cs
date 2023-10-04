using MonkeyLoader.Patching;
using MonkeyLoader.Resonite.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite
{
    [FeaturePatch<EngineInitialization>(PatchCompatibility.HookOnly)]
    internal sealed class ResoniteHooksMonkey : Monkey
    {
        protected override void OnLoaded()
        {
        }

        private static void onEngineShutdown()
        {
            base.OnEngineShutdown();
        }

        private void onEngineInitialized()
        {
            base.OnEngineInitialized();
        }
    }
}