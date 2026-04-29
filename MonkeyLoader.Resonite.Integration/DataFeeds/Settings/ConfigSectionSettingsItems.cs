using Elements.Core;
using EnumerableToolkit;
using FrooxEngine;
using FrooxEngine.UIX;
using MonkeyLoader.Configuration;
using MonkeyLoader.Meta;

namespace MonkeyLoader.Resonite.DataFeeds.Settings
{
    internal sealed class ConfigSectionSettingsItems : DataFeedBuildingBlockMonkey<ConfigSectionSettingsItems, SettingsDataFeed>
    {
        public override int Priority => HarmonyLib.Priority.Normal;

        public override Sequence<string> SubgroupPath => SubgroupDefinitions.Settings;

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
                foreach (var configKey in configSection.Keys)
                {
                    // Do this here since the actual item generation code doesn't have access to the data feed anymore
                    if (configKey.ValueType.IsInjectableEditorType())
                        parameters.DataFeed.RunSynchronously(() => parameters.DataFeed.GetViewData().EnsureDataFeedValueFieldTemplate(configKey.ValueType));
                }

                var sectionGroup = new DataFeedResettableGroup();
                sectionGroup.InitBase(configSection.Id, parameters.Path, parameters.GroupKeys, configSection.GetLocaleString("Name"));
                sectionGroup.InitResetAction(syncDelegate =>
                {
                    if (syncDelegate.Parent is not ButtonActionTrigger actionTrigger)
                        return;

                    if (syncDelegate.Slot.GetComponent<Button>() is not Button button)
                        return;

                    button.LocalPressed += (_, _) =>
                    {
                        if (actionTrigger.Enabled)
                            configSection.Reset();
                    };
                });
                yield return sectionGroup;

                var sectionGrouping = parameters.GroupKeys.Concat(configSection.Id).ToArray();
                var sectionItems = configSection.EnumerateItemsAsync(parameters.Path, sectionGrouping, parameters.SearchPhrase, parameters.ViewData);

                await foreach (var item in sectionItems)
                    yield return item;
            }
        }
    }
}