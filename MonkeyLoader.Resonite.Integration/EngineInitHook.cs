using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using MonkeyLoader.Resonite.Features.FrooxEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite
{
    [HarmonyPatchCategory(nameof(EngineInitHook))]
    [HarmonyPatch(typeof(Engine), nameof(Engine.Initialize))]
    internal sealed class EngineInitHook : Monkey<EngineInitHook>
    {
        public override string Name { get; } = "Engine Init Hook";

        private static IEnumerable<IResoniteMonkeyInternal> ResoniteMonkeys
            => Mod.Loader.Monkeys.SelectCastable<IMonkey, IResoniteMonkeyInternal>();

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches()
        {
            yield return new FeaturePatch<EngineInitialization>(PatchCompatibility.HookOnly);
        }

        [HarmonyPrefix]
        private static void InitializePrefix(Engine __instance)
        {
            Info(() => "Adding ResoniteMonkey hooks!");

            Mod.Loader.LoggingHandler += ResoniteLoggingHandler.Instance;

            __instance.OnReady += OnEngineReady;
            __instance.OnShutdownRequest += OnEngineShutdownRequested;
            __instance.OnShutdown += OnEngineShutdown;

            LoadProgressIndicator.AddFixedPhases(3);
            LoadProgressIndicator.AdvanceFixedPhase("Executing EngineInit Hooks...");

            foreach (var resoniteMonkey in ResoniteMonkeys)
            {
                LoadProgressIndicator.SetSubphase(resoniteMonkey.Name);
                resoniteMonkey.EngineInit();
                LoadProgressIndicator.ExitSubphase();
            }
        }

        private static void OnEngineReady()
        {
            LoadProgressIndicator.AdvanceFixedPhase("Executing EngineReady Hooks...");

            foreach (var resoniteMonkey in ResoniteMonkeys)
            {
                LoadProgressIndicator.SetSubphase(resoniteMonkey.Name);
                resoniteMonkey.EngineReady();
                LoadProgressIndicator.ExitSubphase();
            }

            LoadProgressIndicator.AdvanceFixedPhase("Mods Fully Loaded");
        }

        private static void OnEngineShutdown() => Mod.Loader.Shutdown();

        private static void OnEngineShutdownRequested(string reason)
        {
            foreach (var resoniteMonkey in Mod.Loader.Monkeys.SelectCastable<IMonkey, IResoniteMonkeyInternal>())
                resoniteMonkey.EngineShutdownRequested(reason);
        }
    }
}