using Elements.Core;
using EnumerableToolkit;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Configuration;
using MonkeyLoader.Meta;
using MonkeyLoader.Resonite.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.DataFeeds.Settings
{
    internal sealed class ConfigSectionSettingsItems : DataFeedBuildingBlockMonkey<ConfigSectionSettingsItems, SettingsDataFeed>
    {
        private static readonly Type _dummyType = typeof(dummy);

        private static readonly MethodInfo _generateItemsForConfigKey = AccessTools.Method(typeof(ConfigSectionSettingsItems), nameof(GenerateItemsForConfigKeyAsync));

        public override int Priority => HarmonyLib.Priority.Normal;

        public override IAsyncEnumerable<DataFeedItem> Apply(IAsyncEnumerable<DataFeedItem> current, EnumerateDataFeedParameters<SettingsDataFeed> parameters)
        {
            var path = parameters.Path;

            if (path.Count is < 2 or > 3 || path[0] is not SettingsHelpers.MonkeyLoader)
                return current;

            if (path.Count == 3 && path[2] is not SettingsHelpers.ConfigSections)
                return current;

            // Format: MonkeyLoader / modId / [page]
            if (!((INestedIdentifiableCollection<Config>)Mod.Loader).TryGet().ByFullId($"{path[1]}.Config", out var config))
            {
                Logger.Error(() => $"Tried to access non-existant config: {path[1]}.Config");
                return current;
            }

            parameters.IncludeOriginalResult = false;

            return current.Concat(EnumerateConfigAsync(parameters, config));
        }

        private static async IAsyncEnumerable<DataFeedItem> EnumerateConfigAsync(EnumerateDataFeedParameters<SettingsDataFeed> parameters, Config config)
        {
            foreach (var configSection in config.Sections.Where(section => !section.InternalAccessOnly && section.Keys.Any(key => !key.InternalAccessOnly)))
            {
                var sectionGroup = new DataFeedGroup();
                sectionGroup.InitBase(configSection.Id, parameters.Path, parameters.GroupKeys, configSection.GetLocaleString("Name"));
                yield return sectionGroup;

                var sectionGrouping = parameters.GroupKeys.Concat(configSection.Id).ToArray();

                if (configSection is ICustomDataFeedItems customItems)
                {
                    await foreach (var item in customItems.Enumerate(parameters.Path, sectionGrouping, parameters.SearchPhrase, parameters.ViewData))
                        yield return item;
                }
                else
                {
                    foreach (var configKey in configSection.Keys.Where(key => !key.InternalAccessOnly))
                    {
                        var configKeyItems = (IAsyncEnumerable<DataFeedItem>)_generateItemsForConfigKey
                            .MakeGenericMethod(configKey.ValueType)
                            .Invoke(null, [configKey, parameters, sectionGrouping]);

                        await foreach (var item in configKeyItems)
                            yield return item;
                    }
                }
            }
        }

        private static async IAsyncEnumerable<DataFeedItem> GenerateItemsForConfigKeyAsync<T>(IDefiningConfigKey<T> configKey, EnumerateDataFeedParameters<SettingsDataFeed> parameters, IReadOnlyList<string> groupKeys)
        {
            await foreach (var feedItem in configKey.GetDataFeedItems(parameters.Path, groupKeys, parameters.SearchPhrase, parameters.ViewData))
            {
                // Todo: This check doesn't really capture all possibly returned ValueField items
                if (feedItem is DataFeedValueField<T>)
                {
                    if (configKey.ValueType.IsInjectableEditorType())
                        parameters.DataFeed.RunSynchronously(() => parameters.DataFeed.GetViewData().EnsureDataFeedValueFieldTemplate(configKey.ValueType));
                }

                yield return feedItem;
            }
        }
    }
}