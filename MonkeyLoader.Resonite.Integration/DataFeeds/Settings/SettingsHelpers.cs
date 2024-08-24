using FrooxEngine;
using MonkeyLoader.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLoader.Resonite.DataFeeds.Settings
{
    public static class SettingsHelpers
    {
        public const string ConfigKeyChangeLabel = "Settings";
        public const string EarlyMonkeys = "EarlyMonkeys";
        public const string MonkeyLoader = "MonkeyLoader";
        public const string Monkeys = "Monkeys";
        public const string MonkeyToggles = "MonkeyToggles";

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
    }
}