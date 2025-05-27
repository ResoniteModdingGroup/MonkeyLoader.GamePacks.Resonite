using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using MonkeyLoader.Resonite.Configuration;
using MonkeyLoader.Resonite.DataFeeds;
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
    internal sealed class EngineInitHook : ConfiguredMonkey<EngineInitHook, EngineInitHookConfig>
    {
        private static IResoniteMonkeyInternal[] ResoniteMonkeys
        {
            get
            {
                var monkeys = Mod.Loader.Mods
                    .GetMonkeysAscending()
                    .OfType<IResoniteMonkeyInternal>()
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

            Mod.Loader.Logging.Controller.Handler += ResoniteLoggingHandler.Instance;

            __instance.OnShutdownRequest += OnEngineShutdownRequested;
            __instance.OnShutdown += OnEngineShutdown;

            // Have to add 6 phases because the indicator
            // will immediately disappear upon entering the last one
            LoadProgressReporter.AddFixedPhases(6);
        }

        private static void LateRunEngineHooks(MonkeyLoader loader, IEnumerable<Mod> mods)
        {
            var resoniteMonkeys = mods.GetMonkeysAscending()
                .OfType<IResoniteMonkeyInternal>()
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

            Task.Run(async () =>
            {
                Mod.Loader.LogPotentialConflicts();

                Logger.Info(() => "Triggering reloading of fallback locale data for mods!");
                await LocaleExtensions.ReloadLocalesAsync();
            });
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
            // This won't work if injectors get added in hot-loaded mods
            var dynamicModBuilder = new DynamicMod.Builder(Mod.GetLocaleKey("DataFeedInjectors"), new Version(1, 1, 0))
            {
                Description = "Contains the dynamically collected Monkeys that handle injecting elements into their respective DataFeed.",
                Title = "DataFeed Injectors",
                ProjectUrl = Mod.ProjectUrl,
                // IconPath = Mod.IconPath,
                IconUrl = Mod.IconUrl,
                Authors = ["Banane9"],
                IsGamePack = true
            };
            dynamicModBuilder.AddMonkeys(DataFeedInjectorManager.MonkeyTypes);

            dynamicModBuilder.CreateAndRunFor(Mod.Loader);

            var resoniteMonkeys = ResoniteMonkeys;
            Logger.Trace(() => "Running EngineInit hooks in this order:");
            Logger.Trace(resoniteMonkeys);

            LoadProgressReporter.AdvanceFixedPhase("Executing EngineInit Hooks...");

            var sw = Stopwatch.StartNew();

            foreach (var resoniteMonkey in resoniteMonkeys)
            {
                LoadProgressReporter.SetSubphase(resoniteMonkey.Name);
                await PrettyAwaitAsync(50, Task.Run(resoniteMonkey.EngineInit));
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
                await PrettyAwaitAsync(50, Task.Run(resoniteMonkey.EngineReady));
                LoadProgressReporter.ExitSubphase();
            }

            LoadProgressReporter.AdvanceFixedPhase("Determining potential Mod conflicts...");
            await PrettyAwaitAsync(200, Task.Run(Mod.Loader.LogPotentialConflicts));

            Logger.Info(() => $"Done executing EngineReady hooks on ResoniteMonkeys in {sw.ElapsedMilliseconds}ms!");

            LoadProgressReporter.AdvanceFixedPhase("Loading Fallback Locale Data for Mods...");
            sw.Restart();

            await PrettyAwaitAsync(200, Task.Run(LocaleExtensions.LoadFallbackLocaleAsync));
            Logger.Info(() => $"Done loading fallback locale data for mods in {sw.ElapsedMilliseconds}ms!");

            LoadProgressReporter.AdvanceFixedPhase("Mods Fully Loaded");
        }

        private static async Task PrettyAwaitAsync(int milliseconds, Task task)
        {
            if (ConfigSection.PrettySplashEnabled)
            {
                await Task.WhenAll(Task.Delay(milliseconds), task);
            }
            else
            {
                await task;
            }
        }
    }
}
