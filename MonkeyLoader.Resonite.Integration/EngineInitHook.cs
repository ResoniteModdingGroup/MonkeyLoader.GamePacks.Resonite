﻿using FrooxEngine;
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

        private static IResoniteMonkeyInternal[] ResoniteMonkeys
        {
            get
            {
                var monkeys = Mod.Loader.Monkeys
                    .SelectCastable<IMonkey, IResoniteMonkeyInternal>()
                    .ToArray();

                Array.Sort(monkeys, Monkey.AscendingComparer);

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
            await RunEngineReadyHooksAsync();
        }

        [HarmonyPrefix]
        private static void InitializePrefix(Engine __instance)
        {
            Info(() => "Engine started initializing! Adding shutdown hooks and executing EngineInit hooks on ResoniteMonkeys!");

            Mod.Loader.LoggingHandler += ResoniteLoggingHandler.Instance;

            __instance.OnShutdownRequest += OnEngineShutdownRequested;
            __instance.OnShutdown += OnEngineShutdown;

            // Have to add 3 phases because the indicator
            // will immediately disappear upon entering the last one
            LoadProgressIndicator.AddFixedPhases(3);
        }

        private static void OnEngineShutdown()
        {
            Info(() => "Engine shutdown has been triggered! Passing shutdown through to MonkeyLoader!");

            Mod.Loader.Shutdown();
        }

        private static void OnEngineShutdownRequested(string reason)
        {
            Info(() => "Engine shutdown has been requested! Executing EngineShutdownRequested hooks on ResoniteMonkeys!");

            var resoniteMonkeys = ResoniteMonkeys;
            Logger.Trace(() => "Running EngineShutdownRequested hooks in this order:");
            Logger.Trace(resoniteMonkeys.Select(rM => new Func<object>(() => $"{rM.Mod.Title}/{rM.Name}")));

            var sw = Stopwatch.StartNew();

            foreach (var resoniteMonkey in resoniteMonkeys)
                resoniteMonkey.EngineShutdownRequested(reason);

            Info(() => $"Done executing EngineShutdownRequested hooks on ResoniteMonkeys in {sw.ElapsedMilliseconds}ms!");
        }

        private static async Task RunEngineInitHooksAsync()
        {
            var resoniteMonkeys = ResoniteMonkeys;
            Logger.Trace(() => "Running EngineInit hooks in this order:");
            Logger.Trace(resoniteMonkeys.Select(rM => new Func<object>(() => $"{rM.Mod.Title}/{rM.Name}")));

            LoadProgressIndicator.AdvanceFixedPhase("Executing EngineInit Hooks...");

            var sw = Stopwatch.StartNew();

            foreach (var resoniteMonkey in resoniteMonkeys)
            {
                LoadProgressIndicator.SetSubphase(resoniteMonkey.Name);
                await Task.Run(resoniteMonkey.EngineInit);
                LoadProgressIndicator.ExitSubphase();
            }

            Info(() => $"Done executing EngineInit hooks on ResoniteMonkeys in {sw.ElapsedMilliseconds}ms!");
        }

        private static async Task RunEngineReadyHooksAsync()
        {
            Info(() => "Engine is done initializing! Executing EngineReady hooks on ResoniteMonkeys!");

            var resoniteMonkeys = ResoniteMonkeys;
            Logger.Trace(() => "Running EngineReady hooks in this order:");
            Logger.Trace(resoniteMonkeys.Select(rM => new Func<object>(() => $"{rM.Mod.Title}/{rM.Name}")));

            LoadProgressIndicator.AdvanceFixedPhase("Executing EngineReady Hooks...");
            var sw = Stopwatch.StartNew();

            foreach (var resoniteMonkey in resoniteMonkeys)
            {
                LoadProgressIndicator.SetSubphase(resoniteMonkey.Name);
                await Task.Run(resoniteMonkey.EngineReady);
                LoadProgressIndicator.ExitSubphase();
            }

            Info(() => $"Done executing EngineReady hooks on ResoniteMonkeys in {sw.ElapsedMilliseconds}ms!");
            LoadProgressIndicator.AdvanceFixedPhase("Mods Fully Loaded");
        }
    }
}