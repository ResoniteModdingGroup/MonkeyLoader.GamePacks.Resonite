using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using MonkeyLoader.Resonite.Configuration;
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
    // TODO: Add an event after InitializePostfixAsync is done?
    [HarmonyPatchCategory(nameof(EngineInitHook))]
    [HarmonyPatch(typeof(Engine), nameof(Engine.Initialize))]
    internal sealed class EngineInitHook : Monkey<EngineInitHook>
    {
        private static IResoniteMonkeyInternal[] ResoniteMonkeys
        {
            get
            {
                var monkeys = Mod.Loader.Mods
                    .GetMonkeysAscending()
                    .SelectCastable<IMonkey, IResoniteMonkeyInternal>()
                    .ToArray();

                return monkeys;
            }
        }

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches()
        {
            yield return new FeaturePatch<EngineInitialization>(PatchCompatibility.HookOnly);
        }

        [HarmonyPostfix]
        private static async Task InitializePostfixAsync(Task __result)
        {
            await RunEngineInitHooksAsync();
            await __result;
            SharedConfig.Initialize();
            await RunEngineReadyHooksAsync();

            Mod.Loader.ModsRan += LateRunEngineHooks;
        }

        [HarmonyPrefix]
        private static void InitializePrefix(Engine __instance)
        {
            Logger.Info(() => "Engine started initializing! Adding shutdown hooks and executing EngineInit hooks on ResoniteMonkeys!");

            Mod.Loader.LoggingController.Handler += ResoniteLoggingHandler.Instance;

            __instance.OnShutdownRequest += OnEngineShutdownRequested;
            __instance.OnShutdown += OnEngineShutdown;

            // Have to add 3 phases because the indicator
            // will immediately disappear upon entering the last one
            LoadProgressReporter.AddFixedPhases(3);
        }

        private static void LateRunEngineHooks(MonkeyLoader loader, IEnumerable<Mod> mods)
        {
            var resoniteMonkeys = mods.GetMonkeysAscending()
                .SelectCastable<IMonkey, IResoniteMonkeyInternal>()
                .ToArray();

            Logger.Trace(() => "Late-running EngineInit hooks in this order:");
            Logger.Trace(resoniteMonkeys);

            var sw = Stopwatch.StartNew();

            foreach (var resoniteMonkey in resoniteMonkeys)
                resoniteMonkey.EngineInit();

            Logger.Info(() => $"Done late-executing EngineInit hooks on ResoniteMonkeys in {sw.ElapsedMilliseconds}ms!");

            // ------------------------------------------------------------------------------------------------ //

            Logger.Trace(() => "Late-running EngineReady hooks in this order:");
            Logger.Trace(resoniteMonkeys);

            sw.Restart();

            foreach (var resoniteMonkey in resoniteMonkeys)
                resoniteMonkey.EngineReady();

            Logger.Info(() => $"Done late-executing EngineReady hooks on ResoniteMonkeys in {sw.ElapsedMilliseconds}ms!");
        }

        private static void OnEngineShutdown()
        {
            Logger.Info(() => "Engine shutdown has been triggered! Passing shutdown through to MonkeyLoader!");

            Mod.Loader.Shutdown();
        }

        private static void OnEngineShutdownRequested(string reason)
        {
            Logger.Info(() => "Engine shutdown has been requested! Executing EngineShutdownRequested hooks on ResoniteMonkeys!");

            var resoniteMonkeys = ResoniteMonkeys;
            Logger.Trace(() => "Running EngineShutdownRequested hooks in this order:");
            Logger.Trace(resoniteMonkeys);

            var sw = Stopwatch.StartNew();

            foreach (var resoniteMonkey in resoniteMonkeys)
                resoniteMonkey.EngineShutdownRequested(reason);

            Logger.Info(() => $"Done executing EngineShutdownRequested hooks on ResoniteMonkeys in {sw.ElapsedMilliseconds}ms!");
        }

        private static async Task RunEngineInitHooksAsync()
        {
            var resoniteMonkeys = ResoniteMonkeys;
            Logger.Trace(() => "Running EngineInit hooks in this order:");
            Logger.Trace(resoniteMonkeys);

            LoadProgressReporter.AdvanceFixedPhase("Executing EngineInit Hooks...");

            var sw = Stopwatch.StartNew();

            foreach (var resoniteMonkey in resoniteMonkeys)
            {
                LoadProgressReporter.SetSubphase(resoniteMonkey.Name);
                await Task.WhenAll(Task.Delay(50), Task.Run(resoniteMonkey.EngineInit));
                LoadProgressReporter.ExitSubphase();
            }

            Logger.Info(() => $"Done executing EngineInit hooks on ResoniteMonkeys in {sw.ElapsedMilliseconds}ms!");
        }

        private static async Task RunEngineReadyHooksAsync()
        {
            Logger.Info(() => "Engine is done initializing! Executing EngineReady hooks on ResoniteMonkeys!");

            var resoniteMonkeys = ResoniteMonkeys;
            Logger.Trace(() => "Running EngineReady hooks in this order:");
            Logger.Trace(resoniteMonkeys);

            LoadProgressReporter.AdvanceFixedPhase("Executing EngineReady Hooks...");
            var sw = Stopwatch.StartNew();

            foreach (var resoniteMonkey in resoniteMonkeys)
            {
                LoadProgressReporter.SetSubphase(resoniteMonkey.Name);
                await Task.WhenAll(Task.Delay(50), Task.Run(resoniteMonkey.EngineReady));
                LoadProgressReporter.ExitSubphase();
            }

            Logger.Info(() => $"Done executing EngineReady hooks on ResoniteMonkeys in {sw.ElapsedMilliseconds}ms!");
            LoadProgressReporter.AdvanceFixedPhase("Mods Fully Loaded");
        }
    }
}