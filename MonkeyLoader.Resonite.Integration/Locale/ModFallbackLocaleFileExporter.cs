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
        public const string ExportLocaleFileLoaded = $"{ExportLocaleFile}.Loaded";
        public const string ExportLocaleFileRaw = $"{ExportLocaleFile}.Raw";

        private const string ExportLocaleFilePath = $"{ExportLocaleFile}.Path";

        private static readonly string _localeExportDirectory = Path.Combine("MonkeyLoader", "LocaleExport");

        private static AsyncEventDispatching<FallbackLocaleGenerationEvent>? _eventDispatching;
        public override bool CanBeDisabled => true;

        public override int Priority => HarmonyLib.Priority.Low;

        public override IAsyncEnumerable<DataFeedItem> Apply(IAsyncEnumerable<DataFeedItem> current, EnumerateDataFeedParameters<SettingsDataFeed> parameters)
        {
            if (!Enabled)
                return current;

            var path = parameters.Path;

            if (path.Count is < 2 or > 3 || path[0] is not SettingsHelpers.MonkeyLoader)
                return current;

            if (path.Count == 3 && path[2] is not ExportLocaleFileLoaded and not ExportLocaleFileRaw)
                return current;

            // Format: MonkeyLoader / modId / [page]
            if (!Mod.Loader.TryGet<Mod>().ById(path[1], out var mod) && path[1] is not SettingsHelpers.MonkeyLoader)
                return current;

            var exportId = mod?.Id ?? SettingsHelpers.MonkeyLoader;
            var authors = mod?.Authors ?? Mod.Authors;

            if (path.Count == 3)
            {
                Engine.Current.GlobalCoroutineManager.StartBackgroundTask(()
                    => ExportLocaleFileAsync(exportId, authors, path[2] is ExportLocaleFileLoaded));

                parameters.MoveUpFromCategory();
                return current;
            }

            return current.Concat(MakeExportItems(parameters, exportId).ToAsyncEnumerable());
        }

        internal static async Task ExportLocaleFileAsync(string exportId, IEnumerable<string> authors, bool useLoadedMessages)
        {
            Logger.Info(() => $"Exporting locale file for mod: {exportId}");

            var eventData = new FallbackLocaleGenerationEvent(new());
            await (_eventDispatching?.Invoke(eventData) ?? Task.CompletedTask);

            Func<KeyValuePair<string, Elements.Assets.LocaleResource.Message>, string> messageValueSelector = useLoadedMessages
                ? GetMessagePatternFromLoadedFallback
                : static fallbackMessage => fallbackMessage.Value.messagePattern;

            LocaleData localeData = new()
            {
                Authors = [.. authors],
                LocaleCode = FallbackLocaleGenerator.LocaleCode,
                Messages = eventData.Messages.Where(message => message.Key.StartsWith(exportId))
                    .ToDictionary(static message => message.Key, messageValueSelector)
            };

            Directory.CreateDirectory(_localeExportDirectory);
            var fileName = $"{exportId}-{FallbackLocaleGenerator.LocaleCode}.json";
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

        private static string GetMessagePatternFromLoadedFallback(KeyValuePair<string, Elements.Assets.LocaleResource.Message> fallbackMessage)
            => LocaleExtensions.FallbackLocale._formatMessages.TryGetValue(fallbackMessage.Key, out var loadedMessage)
                ? loadedMessage.messagePattern
                : fallbackMessage.Value.messagePattern;

        private IEnumerable<DataFeedItem> MakeExportItems(EnumerateDataFeedParameters<SettingsDataFeed> parameters, string exportId)
        {
            var exportLocaleGroup = new DataFeedGroup();
            exportLocaleGroup.InitBase(ExportLocaleFile, parameters.Path, parameters.GroupKeys, Mod.GetLocaleString(ExportLocaleFile));
            yield return exportLocaleGroup;

            var fileName = $"{exportId}-{FallbackLocaleGenerator.LocaleCode}.json";
            IReadOnlyList<string> groupKeys = [.. parameters.GroupKeys, ExportLocaleFile];

            var exportLocaleFilePath = new DataFeedIndicator<string>();
            exportLocaleFilePath.InitBase(ExportLocaleFilePath, parameters.Path, groupKeys, Mod.GetLocaleString(ExportLocaleFilePath));
            exportLocaleFilePath.InitSetupValue(field => field.Value = Path.Combine("Resonite", _localeExportDirectory, fileName));
            yield return exportLocaleFilePath;

            var exportLocaleFileLoadedButton = new DataFeedCategory();
            exportLocaleFileLoadedButton.InitBase(ExportLocaleFileLoaded, parameters.Path, groupKeys, Mod.GetLocaleString(ExportLocaleFileLoaded));
            yield return exportLocaleFileLoadedButton;

            var exportLocaleFileRawButton = new DataFeedCategory();
            exportLocaleFileRawButton.InitBase(ExportLocaleFileRaw, parameters.Path, groupKeys, Mod.GetLocaleString(ExportLocaleFileRaw));
            yield return exportLocaleFileRawButton;
        }

        event AsyncEventDispatching<FallbackLocaleGenerationEvent>? IAsyncEventSource<FallbackLocaleGenerationEvent>.Dispatching
        {
            add => _eventDispatching += value;
            remove => _eventDispatching -= value;
        }
    }
}