using Elements.Core;
using EnumerableToolkit;
using FrooxEngine;
using MonkeyLoader.Configuration;
using MonkeyLoader.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLoader.Resonite.DataFeeds.Settings
{
    public static class SettingsHelpers
    {
        public const string ConfigKeyChangeLabel = "Settings";
        public const string ConfigSections = "ConfigSections";
        public const string EarlyMonkeys = "EarlyMonkeys";
        public const string MetaData = "MetaData";
        public const string MonkeyLoader = "MonkeyLoader";
        public const string Monkeys = "Monkeys";
        public const string MonkeyToggles = "MonkeyToggles";
        public const string ResetConfig = "ResetConfig";
        public const string SaveConfig = "SaveConfig";
        private static readonly Dictionary<SettingsDataFeed, SettingsViewData> _settingsViewsByFeed = [];

        private static Logger Logger => MonkeyLoaderRootCategorySettingsItems.Logger;

        public static SettingsViewData GetViewData(this SettingsDataFeed dataFeed)
            => _settingsViewsByFeed.GetOrCreateValue(dataFeed, () => CreateViewData(dataFeed));

        public static void InitBase(this DataFeedItem item, IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, IDefiningConfigKey configKey)
            => item.InitBase(configKey.FullId, path, groupKeys, configKey.GetLocaleString("Name"), configKey.GetLocaleString("Description"));

        public static bool IsInjectableEditorType(this Type type)
            // Check with nameof for dummy, because there's also dummy<>
            => type.Name != nameof(dummy) && (Coder.IsEnginePrimitive(type) || type == typeof(Type));

        public static bool IsInjectableEditorType<T>() => IsInjectableEditorType(typeof(T));

        public static void SetupConfigKeyField<T>(this IField<T> field, IDefiningConfigKey<T> configKey)
        {
            var slot = field.FindNearestParent<Slot>();

            if (slot.GetComponentInParents<FeedItemInterface>() is FeedItemInterface feedItemInterface)
            {
                // Adding the config key's full id to make it easier to create standalone facets
                feedItemInterface.Slot.AttachComponent<Comment>().Text.Value = configKey.FullId;
            }

            field.SyncWithConfigKey(configKey, ConfigKeyChangeLabel);
        }

        private static SettingsViewData CreateViewData(SettingsDataFeed dataFeed)
        {
            static void OnDestroyed(IDestroyable destroyable)
            {
                destroyable.Destroyed -= OnDestroyed;
                _settingsViewsByFeed.Remove((SettingsDataFeed)destroyable);

                Logger.Debug(() => $"Removed ViewData for SettingsDataFeed ({destroyable.ReferenceID})");
            }

            var viewData = new SettingsViewData(dataFeed);
            dataFeed.Destroyed += OnDestroyed;

            return viewData;
        }
    }
}