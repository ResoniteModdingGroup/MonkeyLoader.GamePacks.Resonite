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
    [HarmonyPatchCategory(nameof(ResoniteHooksMonkey))]
    [HarmonyPatch(typeof(Engine), nameof(Engine.Initialize))]
    internal sealed class ResoniteHooksMonkey : Monkey<ResoniteHooksMonkey>
    {
        public override string Name { get; } = "Hooks";

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches()
        {
            yield return new FeaturePatch<EngineInitialization>(PatchCompatibility.HookOnly);
        }

        [HarmonyPrefix]
        private static void InitializePrefix(Engine __instance)
        {
            Info(() => "Adding ResoniteMonkey hooks!");

            Mod.Loader.LoggingHandler += ResoniteLoggingHandler.Instance;

            // add extra OnEngineInit phase to resonite monkeys for doing this or other things.
            LoadProgressIndicator.AddFixedPhase();

            __instance.OnReady += OnEngineReady;
            __instance.OnShutdownRequest += OnEngineShutdownRequested;
            __instance.OnShutdown += OnEngineShutdown;
        }

        private static void OnEngineReady()
        {
            LoadProgressIndicator.AdvanceFixedPhase();
            LoadProgressIndicator.EnterCustomPhase("Executing EngineReady Hooks");

            foreach (var resoniteMonkey in Mod.Loader.Monkeys.SelectCastable<IMonkey, IResoniteMonkeyInternal>())
            {
                LoadProgressIndicator.EnterCustomPhase($"EngineReady for Resonite Monkey: {resoniteMonkey.Name}");
                resoniteMonkey.EngineReady();
                LoadProgressIndicator.ExitCustomPhase();
            }

            LoadProgressIndicator.ExitCustomPhase();
        }

        private static void OnEngineShutdown() => Mod.Loader.Shutdown();

        private static void OnEngineShutdownRequested(string reason)
        {
            foreach (var resoniteMonkey in Mod.Loader.Monkeys.SelectCastable<IMonkey, IResoniteMonkeyInternal>())
                resoniteMonkey.EngineShutdownRequested(reason);
        }
    }
}