using FrooxEngine;
using MonkeyLoader.Configuration;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.DataFeeds.Settings
{
    internal sealed class SaveConfigSettingsItems : DataFeedBuildingBlockMonkey<SaveConfigSettingsItems, SettingsDataFeed>
    {
        public override int Priority => 300;

        public override IAsyncEnumerable<DataFeedItem> Apply(IAsyncEnumerable<DataFeedItem> current, EnumerateDataFeedParameters<SettingsDataFeed> parameters)
        {
            var path = parameters.Path;

            if (path.Count is < 2 or > 4 || path[0] is not SettingsHelpers.MonkeyLoader)
                return current;

            if (path.Count == 4 && path[2] is not SettingsHelpers.ConfigSections)
                return current;

            // Format: MonkeyLoader / modId / [page]
            if (!((INestedIdentifiableCollection<Config>)Mod.Loader).TryGet().ByFullId($"{path[1]}.Config", out var config))
            {
                Logger.Error(() => $"Tried to access non-existant config: {path[1]}.Config");
                return current;
            }

            if (path.Count >= 3)
            {
                switch (path[^1])
                {
                    case SettingsHelpers.SaveConfig:
                        Logger.Info(() => $"Triggering saving of config: {config.FullId}");

                        config.Save();
                        MoveUpFromCategory(parameters);

                        return current;

                    case SettingsHelpers.ResetConfig:
                        Logger.Info(() => $"Triggering reset of config: {config.FullId}");

                        config.Reset();
                        MoveUpFromCategory(parameters);

                        return current;
                }
            }

            parameters.IncludeOriginalResult = false;

            if (!config.Sections.Any(section => section.Keys.Any(key => !key.InternalAccessOnly)))
                return current;

            return current.Concat(EnumerateSaveButtonsAsync(parameters));
        }

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => [];

        private static async IAsyncEnumerable<DataFeedItem> EnumerateSaveButtonsAsync(EnumerateDataFeedParameters<SettingsDataFeed> parameters)
        {
            await Task.CompletedTask;

            var saveConfigButton = new DataFeedCategory();
            saveConfigButton.InitBase(SettingsHelpers.SaveConfig, parameters.Path, parameters.GroupKeys, Mod.GetLocaleString("SaveConfig"));
            yield return saveConfigButton;

            var resetConfigButton = new DataFeedCategory();
            resetConfigButton.InitBase(SettingsHelpers.ResetConfig, parameters.Path, parameters.GroupKeys, Mod.GetLocaleString("ResetConfig"));
            yield return resetConfigButton;
        }

        private static void MoveUpFromCategory(EnumerateDataFeedParameters<SettingsDataFeed> parameters)
            => parameters.DataFeed.RunSynchronously(() => parameters.DataFeed.GetViewData().MoveUpFromCategory(parameters.Path[^1]));
    }
}