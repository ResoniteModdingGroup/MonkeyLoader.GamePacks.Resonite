using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using MonkeyLoader.Resonite.Features;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite
{
    [HarmonyPatchCategory(nameof(ResoniteHooksMonkey))]
    [HarmonyPatch(typeof(Engine), nameof(Engine.Initialize))]
    [FeaturePatch<EngineInitialization>(PatchCompatibility.HookOnly)]
    internal sealed class ResoniteHooksMonkey : Monkey<ResoniteHooksMonkey>
    {
        public override string Name { get; } = "Hooks";

        protected override sealed bool onLoaded()
        {
            Logger.Info(() => "Integrating with Resonite!");
            Harmony.PatchCategory(nameof(ResoniteHooksMonkey));
            return true;
        }

        [HarmonyPrefix]
        private static void initializePrefix(Engine __instance)
        {
            Mod.Loader.LoggingHandler = new ResoniteLoggingHandler();

            __instance.OnReady += onEngineReady;
            __instance.OnShutdownRequest += onEngineShutdownRequested;
            __instance.OnShutdown += onEngineShutdown;
        }

        private static void onEngineReady()
        {
            foreach (var resoniteMonkey in Mod.Loader.Monkeys.SelectCastable<IMonkey, IResoniteMonkeyInternal>())
                resoniteMonkey.EngineReady();
        }

        private static void onEngineShutdown() => Mod.Loader.Shutdown();

        private static void onEngineShutdownRequested(string reason)
        {
            foreach (var resoniteMonkey in Mod.Loader.Monkeys.SelectCastable<IMonkey, IResoniteMonkeyInternal>())
                resoniteMonkey.EngineShutdownRequested(reason);
        }
    }
}