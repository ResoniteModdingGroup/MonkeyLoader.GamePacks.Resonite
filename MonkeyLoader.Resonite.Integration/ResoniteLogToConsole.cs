using Elements.Core;
using MonkeyLoader.Logging;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite
{
    internal sealed class ResoniteLogToConsole : Monkey<ResoniteLogToConsole>
    {
        private static Task _lastLogTask = Task.CompletedTask;

        public override bool CanBeDisabled => true;

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => [];

        protected override bool OnLoaded()
        {
            UniLog.OnError += message => ToConsoleLoggingHandler(LoggingLevel.Error, message);
            UniLog.OnWarning += message => ToConsoleLoggingHandler(LoggingLevel.Warn, message);
            UniLog.OnLog += message => ToConsoleLoggingHandler(LoggingLevel.Info, message);

            return true;
        }

        private static void HandleLogging(LoggingLevel level, string message)
        {
            string Producer()
            {
                var fpsIndex = message.IndexOf(" FPS)");
                if (fpsIndex >= 0)
                {
                    var openIndex = message.LastIndexOf('(', fpsIndex - 4);
                    message = message[openIndex..];
                }

                return message.TrimEnd('\r', '\n');
            }

            switch (level)
            {
                case LoggingLevel.Error:
                    ConsoleLoggingHandler.Instance.Error(Producer);
                    break;

                case LoggingLevel.Warn:
                    ConsoleLoggingHandler.Instance.Warn(Producer);
                    break;

                default:
                    ConsoleLoggingHandler.Instance.Info(Producer);
                    break;
            }
        }

        private static void ToConsoleLoggingHandler(LoggingLevel level, string message)
        {
            // Log when enabled, Console is open, level is supported, and message isn't from the ModLoader
            if (!Enabled || !ConsoleLoggingHandler.Instance.Connected || !Logger.ShouldLog(level) || message.Length <= 25 || message.Contains("[MonkeyLoader"))
                return;

            lock (Instance)
            {
                _lastLogTask = _lastLogTask.ContinueWith(_ => HandleLogging(level, message),
                    TaskContinuationOptions.RunContinuationsAsynchronously);
            }
        }
    }
}