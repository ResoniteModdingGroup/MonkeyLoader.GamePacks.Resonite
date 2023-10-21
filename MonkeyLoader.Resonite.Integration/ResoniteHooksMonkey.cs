using FrooxEngine;
using HarmonyLib;
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
        protected override void OnLoaded()
        {
            Logger.Info(() => "Hello from Resonite Integration!");
            File.AppendAllText("test.log", $"[{DateTime.Now}] Hello from resonite hooks monkey!{Environment.NewLine}");
            Harmony.PatchCategory(nameof(ResoniteHooksMonkey));
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
            try
            {
                Mod.Loader.Monkeys
                    .SelectCastable<Monkey, IResoniteMonkey>()
                    .Select(resMonkey => (Delegate)resMonkey.OnEngineReady)
                    .TryInvokeAll();
            }
            catch (AggregateException ex)
            {
                Logger.Error(() => ex.Format("Some EngineReady hooks threw an Exception:"));
            }
        }

        private static void onEngineShutdown()
        {
            try
            {
                Mod.Loader.Monkeys
                    .SelectCastable<Monkey, IResoniteMonkey>()
                    .Select(resMonkey => (Delegate)resMonkey.OnEngineShutdown)
                    .TryInvokeAll();
            }
            catch (AggregateException ex)
            {
                Logger.Error(() => ex.Format("Some EngineShutdown hooks threw an Exception:"));
            }

            Mod.Loader.Shutdown();
        }

        private static void onEngineShutdownRequested(string reason)
        {
            try
            {
                Mod.Loader.Monkeys
                    .SelectCastable<Monkey, IResoniteMonkey>()
                    .Select(resMonkey => (Delegate)resMonkey.OnEngineShutdownRequested)
                    .TryInvokeAll(reason);
            }
            catch (AggregateException ex)
            {
                Logger.Error(() => ex.Format("Some EngineShutdownRequested hooks threw an Exception:"));
            }
        }
    }
}