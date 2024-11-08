using FrooxEngine;
using MonkeyLoader.Configuration;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using MonkeyLoader.Resonite.DataFeeds;
using MonkeyLoader.Resonite.DataFeeds.Settings;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

namespace MonkeyLoader.Resonite.Locale
{
    internal sealed class ModLocaleFileExporter : DataFeedBuildingBlockMonkey<ModLocaleFileExporter, SettingsDataFeed>
    {
        public const string ExportLocaleFile = "ExportLocaleFile";
        public override int Priority => HarmonyLib.Priority.Low;

        public override IAsyncEnumerable<DataFeedItem> Apply(IAsyncEnumerable<DataFeedItem> current, EnumerateDataFeedParameters<SettingsDataFeed> parameters)
        {
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
                Logger.Info(() => $"Exporting locale file for mod: {mod.Id}");
                // Add export

                parameters.MoveUpFromCategory();

                return current;
            }

            return InjectExportButton(current, parameters, mod);
        }

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => [];

        private async IAsyncEnumerable<DataFeedItem> InjectExportButton(IAsyncEnumerable<DataFeedItem> current, EnumerateDataFeedParameters<SettingsDataFeed> parameters, Mod mod)
        {
            await foreach (var item in current)
            {
                yield return item;

                if (item is DataFeedIndicator<string> && item.ItemKey == "Description")
                {
                    var exportLocaleFileButton = new DataFeedCategory();
                    exportLocaleFileButton.InitBase(ExportLocaleFile, parameters.Path, [.. parameters.GroupKeys, "Metadata"], Mod.GetLocaleString(ExportLocaleFile));
                    yield return exportLocaleFileButton;
                }
            }
        }
    }
}