using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using MonkeyLoader.Resonite.Features.FrooxEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Info(() => "Engine started initializing! Adding engine hooks and executing EngineInit hooks on ResoniteMonkeys!");

            Mod.Loader.LoggingHandler += ResoniteLoggingHandler.Instance;

            __instance.OnReady += OnEngineReady;
            __instance.OnShutdownRequest += OnEngineShutdownRequested;
            __instance.OnShutdown += OnEngineShutdown;

            var resoniteMonkeys = ResoniteMonkeys.ToArray();
            Logger.Trace(() => "Running EngineInit hooks in this order:");
            Logger.Trace(resoniteMonkeys.Select(rM => new Func<object>(() => $"{rM.Mod.Title}/{rM.Name}")));

            // Have to add 3 phases because the indicator
            // will immediately disappear upon entering the last one
            LoadProgressIndicator.AddFixedPhases(3);
            LoadProgressIndicator.AdvanceFixedPhase("Executing EngineInit Hooks...");

            var sw = Stopwatch.StartNew();

            foreach (var resoniteMonkey in ResoniteMonkeys)
            {
                LoadProgressIndicator.SetSubphase(resoniteMonkey.Name);
                resoniteMonkey.EngineInit();
                LoadProgressIndicator.ExitSubphase();
            }

            Info(() => $"Done executing EngineInit hooks on ResoniteMonkeys in {sw.ElapsedMilliseconds}ms!");
        }

        private static void OnEngineReady()
        {
            // Potentially move this to be a postfix of init or run as Task as otherwise it's blocking.
            Info(() => "Engine is ready! Executing EngineReady hooks on ResoniteMonkeys!");

            var resoniteMonkeys = ResoniteMonkeys.ToArray();
            Logger.Trace(() => "Running EngineReady hooks in this order:");
            Logger.Trace(resoniteMonkeys.Select(rM => new Func<object>(() => $"{rM.Mod.Title}/{rM.Name}")));

            LoadProgressIndicator.AdvanceFixedPhase("Executing EngineReady Hooks...");
            var sw = Stopwatch.StartNew();

            foreach (var resoniteMonkey in ResoniteMonkeys)
            {
                LoadProgressIndicator.SetSubphase(resoniteMonkey.Name);
                resoniteMonkey.EngineReady();
                LoadProgressIndicator.ExitSubphase();
            }

            Info(() => $"Done executing EngineReady hooks on ResoniteMonkeys in {sw.ElapsedMilliseconds}ms!");
            LoadProgressIndicator.AdvanceFixedPhase("Mods Fully Loaded");
        }

        private static void OnEngineShutdown()
        {
            Info(() => "Engine shutdown has been triggered! Passing shutdown through to MonkeyLoader!");

            Mod.Loader.Shutdown();
        }

        private static void OnEngineShutdownRequested(string reason)
        {
            Info(() => "Engine shutdown has been requested! Executing EngineShutdownRequested hooks on ResoniteMonkeys!");

            var resoniteMonkeys = ResoniteMonkeys.ToArray();
            Logger.Trace(() => "Running EngineShutdownRequested hooks in this order:");
            Logger.Trace(resoniteMonkeys.Select(rM => new Func<object>(() => $"{rM.Mod.Title}/{rM.Name}")));

            var sw = Stopwatch.StartNew();

            foreach (var resoniteMonkey in resoniteMonkeys)
                resoniteMonkey.EngineShutdownRequested(reason);

            Info(() => $"Done executing EngineShutdownRequested hooks on ResoniteMonkeys in {sw.ElapsedMilliseconds}ms!");
        }
    }
}