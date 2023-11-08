using MonkeyLoader;
using MonkeyLoader.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Doorstop
{
    internal static class Entrypoint
    {
        public static void Start()
        {
            Debugger.Break();
            var log = new FileLoggingHandler("MonkeyLog.log");

            try
            {
                AppDomain.CurrentDomain.UnhandledException += (sender, e) => log.Fatal(() => (e.ExceptionObject as Exception)?.Format("Unhandled Exception!") ?? "Unhandled Exception!");

                var loader = new MonkeyLoader.MonkeyLoader(loggingLevel: LoggingLevel.Trace);
                loader.LoggingHandler += log;

                log.Info(() => $".NET Runtime Version: {Environment.Version}");
                log.Info(() => $".NET Runtime: {RuntimeInformation.FrameworkDescription}");
                log.Info(() => $"Domain Target Framework: {AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName}");

                log.Info(() => $"Base Directory: {AppDomain.CurrentDomain.BaseDirectory}");
                log.Info(() => $"Relative Search Directory: {AppDomain.CurrentDomain.RelativeSearchPath}");
                log.Info(() => $"Private Bin Path: {AppDomain.CurrentDomain.SetupInformation.PrivateBinPath}");
                log.Info(() => $"Entry Assembly: {Assembly.GetEntryAssembly()?.Location}");
                log.Info(() => "CMD Args: " + string.Join(" ", Environment.GetCommandLineArgs()));

                loader.FullLoad();

                //log.Log($"Loaded Assemblies:{Environment.NewLine}{string.Join(Environment.NewLine, AppDomain.CurrentDomain.GetAssemblies().Select(assembly => new MonkeyLoader.AssemblyName(assembly.GetName().Name)))}");
            }
            catch (Exception ex)
            {
                log.Log(ex.Format());
            }
        }
    }
}