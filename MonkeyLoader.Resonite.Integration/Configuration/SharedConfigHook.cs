using FrooxEngine;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.Configuration
{
    internal sealed class SharedConfigHook : ResoniteMonkey<SharedConfigHook>
    {
        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => Enumerable.Empty<IFeaturePatch>();

        protected override bool OnEngineReady() => SharedConfig.Initialize();
    }
}