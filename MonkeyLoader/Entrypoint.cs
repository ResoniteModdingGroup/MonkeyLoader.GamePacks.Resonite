using MonkeyLoader;
using MonkeyLoader.Game;
using MonkeyLoader.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MonoMod.Core.Platforms;
using MonoMod.Logs;
using MonoMod.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Doorstop
{
    internal static class Entrypoint
    {
        public static void HelloMethod()
        {
            File.AppendAllText("test.log", $"[{DateTime.Now}] Hello from new FrooxEngine.Engine static constructor{Environment.NewLine}");
        }

        public static void Start()
        {
            Debugger.Break();
            var log = new FileLoggingHandler("MonkeyLog.log");

            try
            {
                var loader = new MonkeyLoader.MonkeyLoader();
                loader.LoggingHandler = log;

                loader.EnsureAllLocationsExist();
                loader.LoadAllMods();

                var frooxEngine = AssemblyDefinition.ReadAssembly("Resonite_Data\\Managed\\FrooxEngine.dll");

                log.Log($"Modules: {string.Join(", ", frooxEngine.Modules.Select(m => m.Name))}");

                var engine = frooxEngine.MainModule.Types.FirstOrDefault(t => t.Name == "Engine");
                var engineCCtor = engine.GetStaticConstructor();

                var processor = engineCCtor.Body.GetILProcessor();
                processor.InsertBefore(engineCCtor.Body.Instructions.First(), processor.Create(OpCodes.Call, typeof(Entrypoint).GetMethod(nameof(HelloMethod))));

                var ms = new MemoryStream();
                frooxEngine.Write(ms);

                Assembly.Load(ms.ToArray());

                log.Log($"Loaded FrooxEngine from Memory");

                loader.Shutdown();
            }
            catch (Exception ex)
            {
                log.Log(ex.Format());
            }
        }
    }
}