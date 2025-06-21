using Elements.Assets;
using FrooxEngine;
using MonkeyLoader.Events;
using MonkeyLoader.Meta;
using MonkeyLoader.Resonite.DataFeeds;
using MonkeyLoader.Resonite.DataFeeds.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.Locale
{
    internal sealed class ModFallbackLocaleFileExporter
        : DataFeedBuildingBlockMonkey<ModFallbackLocaleFileExporter, SettingsDataFeed>,
            IAsyncEventSource<FallbackLocaleGenerationEvent>
    {
        public const string ExportLocaleFile = "ExportLocaleFile";

        private const string ExportLocaleFileLabel = $"{ExportLocaleFile}.Label";
        private const string ExportLocaleFilePath = $"{ExportLocaleFile}.Path";

        private static AsyncEventDispatching<FallbackLocaleGenerationEvent>? _eventDispatching;
        private static string _localeExportDirectory = Path.Combine("MonkeyLoader", "LocaleExport");

        public override bool CanBeDisabled => true;

        public override int Priority => HarmonyLib.Priority.Low;

        public override IAsyncEnumerable<DataFeedItem> Apply(IAsyncEnumerable<DataFeedItem> current, EnumerateDataFeedParameters<SettingsDataFeed> parameters)
        {
            if (!Enabled)
                return current;

            var path = parameters.Path;

            if (path.Count is < 2 or > 3 || path[0] is not SettingsHelpers.MonkeyLoader)
                return current;

            if (path.Count == 3 && path[2] is not ExportLocaleFile)
                return current;

            // Format: MonkeyLoader / modId / [page]
            if (!Mod.Loader.TryGet<Mod>().ById(path[1], out var mod))
                return current;

            if (path.Count == 3)
            {
                parameters.DataFeed.StartTask(ExportLocaleFileAsync, mod);

                parameters.MoveUpFromCategory();
                return current;
            }

            return InjectExportButtonAsync(current, parameters, mod);
        }

        internal static async Task ExportLocaleFileAsync(Mod mod)
        {
            await default(ToBackground);

            Logger.Info(() => $"Exporting locale file for mod: {mod.Id}");

            var eventData = new FallbackLocaleGenerationEvent([]);
            await (_eventDispatching?.Invoke(eventData) ?? Task.CompletedTask);

            LocaleData localeData = new()
            {
                Authors = [.. mod.Authors],
                LocaleCode = FallbackLocaleGenerator.LocaleCode,
                Messages = eventData.Messages.Where(message => message.Key.StartsWith(mod.Id))
                    .ToDictionary(message => message.Key, message => message.Value.messagePattern)
            };

            Directory.CreateDirectory(_localeExportDirectory);
            var fileName = $"{mod.Id}-{FallbackLocaleGenerator.LocaleCode}.json";
            using var fileStream = File.Open(Path.Combine(_localeExportDirectory, fileName), FileMode.Create, FileAccess.Write);

            JsonSerializerOptions serializerOptions = new()
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            await JsonSerializer.SerializeAsync(fileStream, localeData, options: serializerOptions);

            Logger.Info(() => $"Exported locale file with {localeData.Messages.Count} messages to: {fileName}");
        }

        protected override bool OnComputeDefaultEnabledState() => false;

        protected override bool OnEngineReady()
        {
            Mod.RegisterEventSource(this);

            return base.OnEngineReady();
        }

        protected override bool OnShutdown(bool applicationExiting)
        {
            if (!applicationExiting)
                Mod.UnregisterEventSource(this);

            return base.OnShutdown(applicationExiting);
        }

        private async IAsyncEnumerable<DataFeedItem> InjectExportButtonAsync(IAsyncEnumerable<DataFeedItem> current, EnumerateDataFeedParameters<SettingsDataFeed> parameters, Mod mod)
        {
            await foreach (var item in current)
            {
                yield return item;

                if (item is DataFeedIndicator<string> && item.ItemKey == "Description")
                {
                    var exportLocaleFileButton = new DataFeedCategory();
                    exportLocaleFileButton.InitBase(ExportLocaleFileLabel, parameters.Path, item.GroupingParameters, Mod.GetLocaleString(ExportLocaleFileLabel));
                    exportLocaleFileButton.SetOverrideSubpath(ExportLocaleFile);
                    yield return exportLocaleFileButton;

                    var fileName = $"{mod.Id}-{FallbackLocaleGenerator.LocaleCode}.json";

                    var exportLocaleFilePath = new DataFeedIndicator<string>();
                    exportLocaleFilePath.InitBase(ExportLocaleFilePath, parameters.Path, item.GroupingParameters, Mod.GetLocaleString(ExportLocaleFilePath));
                    exportLocaleFilePath.InitSetupValue(field => field.Value = Path.Combine("Resonite", _localeExportDirectory, fileName));
                    yield return exportLocaleFilePath;
                }
            }
        }

        event AsyncEventDispatching<FallbackLocaleGenerationEvent>? IAsyncEventSource<FallbackLocaleGenerationEvent>.Dispatching
        {
            add => _eventDispatching += value;
            remove => _eventDispatching -= value;
        }
    }
}