using Elements.Core;
using Elements.Quantity;
using EnumerableToolkit;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader;
using MonkeyLoader.Components;
using MonkeyLoader.Configuration;
using MonkeyLoader.Meta;
using MonkeyLoader.Resonite.DataFeeds;
using MonkeyLoader.Resonite.DataFeeds.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.Configuration
{
    /// <summary>
    /// Contains extension methods to generate <see cref="DataFeedItem"/>(s)
    /// for <see cref="IDefiningConfigKey{T}">config keys</see> to represent them
    /// in <see cref="IDataFeed">data feeds</see> - in particular, the <see cref="SettingsDataFeed"/>.
    /// </summary>
    public static class ConfigKeyDataFeedItems
    {
        private static readonly Type _dummyType = typeof(dummy);

        private static readonly MethodInfo _getEnumItemsAsync = AccessTools.Method(typeof(ConfigSectionSettingsItems), nameof(GetEnumItemsAsync));
        private static readonly MethodInfo _getFlagsEnumItems = AccessTools.Method(typeof(ConfigSectionSettingsItems), nameof(GetFlagsEnumFieldsAsync));
        private static readonly MethodInfo _getNullableEnumItemsAsync = AccessTools.Method(typeof(ConfigSectionSettingsItems), nameof(GetNullableEnumItemsAsync));
        private static readonly MethodInfo _getQuantityField = AccessTools.Method(typeof(ConfigSectionSettingsItems), nameof(GetQuantityFieldItem));

        private static Mod Mod => EngineInitHook.Mod;

        /// <summary>
        /// Gets the <see cref="DataFeedItem"/>(s) that represent this <see cref="IDefiningConfigKey{T}">config key</see>.
        /// </summary>
        /// <remarks>
        /// <b>Do not</b> use this method on the <see cref="IDefiningConfigKey{T}">config key</see>
        /// that you are implementing a <see cref="IConfigKeyCustomDataFeedItems{T}">custom items component</see> for.<br/>
        /// Use <c><see cref="GetDefaultDataFeedItems{T}(IDefiningConfigKey{T}, IReadOnlyList{string},
        /// IReadOnlyList{string}, string?, object?)">GetDefaultDataFeedItems</see>()</c> instead
        /// to avoid recursion and a stack overflow.
        /// </remarks>
        /// <typeparam name="T">The type of the config item's value.</typeparam>
        /// <param name="configKey">The config key to get the <see cref="DataFeedItem"/>(s) for.</param>
        /// <param name="path">The path for this enumeration request.</param>
        /// <param name="groupKeys">The group keys for this enumeration request.</param>
        /// <param name="searchPhrase">The search phrase for this enumeration request.</param>
        /// <param name="viewData">The custom view data for this enumeration request.</param>
        /// <returns>
        /// The <see cref="ICustomDataFeedItems">custom items</see> for this <see cref="IDefiningConfigKey{T}">config key</see>
        /// if it has a <see cref="IConfigKeyCustomDataFeedItems{T}">custom items component</see>;
        /// otherwise, the <see cref="GetDefaultDataFeedItems{T}(IDefiningConfigKey{T}, IReadOnlyList{string},
        /// IReadOnlyList{string}, string?, object?)">default items</see>.
        /// </returns>
        public static IAsyncEnumerable<DataFeedItem> GetDataFeedItems<T>(this IDefiningConfigKey<T> configKey,
            IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, string? searchPhrase = null, object? viewData = null)
        {
            IEntity<IDefiningConfigKey<T>> configKeyEntity = configKey;

            // If the config key has a custom items component, use only that.
            if (configKeyEntity.Components.TryGet<IConfigKeyCustomDataFeedItems<T>>(out var customItems))
                return customItems.Enumerate(path, groupKeys, searchPhrase, viewData);

            // Otherwise, use the default items.
            return configKey.GetDefaultDataFeedItems(path, groupKeys, searchPhrase, viewData);
        }

        /// <summary>
        /// Gets the <i>default</i> <see cref="DataFeedItem"/>(s) that represent this <see cref="IDefiningConfigKey{T}">config key</see>.
        /// </summary>
        /// <remarks>
        /// Use this method on the <see cref="IDefiningConfigKey{T}">config key</see> that you are implementing
        /// a <see cref="IConfigKeyCustomDataFeedItems{T}">custom items component</see> for.
        /// </remarks>
        /// <returns>The default <see cref="DataFeedItem"/>(s) representing this <see cref="IDefiningConfigKey{T}">config key</see>.</returns>
        /// <inheritdoc cref="GetDataFeedItems{T}(IDefiningConfigKey{T}, IReadOnlyList{string}, IReadOnlyList{string}, string?, object?)"/>
        public static IAsyncEnumerable<DataFeedItem> GetDefaultDataFeedItems<T>(this IDefiningConfigKey<T> configKey,
            IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, string? searchPhrase = null, object? viewData = null)
        {
            IEntity<IDefiningConfigKey<T>> configKeyEntity = configKey;

            //if (setting is SettingIndicatorProperty)
            //{
            //    return (DataFeedItem)_generateIndicator.MakeGenericMethod(type).Invoke(null, new object[4] { identity, setting, path, grouping });
            //}

            // If the config key is a dummy value, it's a spacer from an RML mod
            // ResoniteModSettings displays the description instead of the name,
            // so we do the same for spacers that have one, as they're used to indicate sections.
            if (configKey.ValueType == _dummyType)
            {
                var dummyField = new DataFeedValueField<dummy>();
                dummyField.InitBase(configKey.FullId, path, groupKeys, configKey.HasDescription ? configKey.GetLocaleString("Description") : " ");
                return dummyField.YieldAsync();
            }

            // If the config key is a boolean value, create a toggle item.
            if (configKey is IDefiningConfigKey<bool> configKeyToggle)
                return configKeyToggle.GetToggleItem(path, groupKeys).YieldAsync();

            // If the config key is an enum value, create an enum selector.
            if (configKey.ValueType.IsEnum)
            {
                // Have to make the generic method manually, as the constraints don't allow it otherwise.
                var enumItems = (IAsyncEnumerable<DataFeedItem>)_getEnumItemsAsync
                    .MakeGenericMethod(configKey.ValueType)
                    .Invoke(null, [configKey, path, groupKeys]);

                return enumItems;
            }

            // If the config key is a nullable ...
            if (configKey.ValueType.IsNullable())
            {
                var nullableType = configKey.ValueType.GetGenericArguments()[0];

                // ... enum value, create a selector for nullable enums.
                if (nullableType.IsEnum)
                {
                    var nullableEnumItems = (IAsyncEnumerable<DataFeedItem>)_getNullableEnumItemsAsync
                        .MakeGenericMethod(nullableType)
                        .Invoke(null, [configKey, path, groupKeys]);

                    return nullableEnumItems;
                }
            }

            // If the config key has a range component ...
            if (configKeyEntity.Components.TryGet<IConfigKeyRange<T>>(out var range))
            {
                // ... and that is also a quantity component, create a quantity item.
                if (configKeyEntity.Components.TryGet<IConfigKeyQuantity<T>>(out var quantity))
                {
                    var quantityField = (DataFeedItem)_getQuantityField
                        .MakeGenericMethod(configKey.ValueType, quantity.QuantityType)
                        .Invoke(null, [configKey, quantity, path, groupKeys]);

                    return quantityField.YieldAsync();
                }

                // Otherwise, create a normal slider item.
                var slider = GetSliderItem(configKey, range, path, groupKeys);
                return slider.YieldAsync();
            }

            // Finally, fall back to generic value field item.
            // It's the callers responsibility to ensure that there's a suitable item template.
            var valueField = configKey.GetValueFieldItem(path, groupKeys);
            return valueField.YieldAsync();
        }

        public static async IAsyncEnumerable<DataFeedItem> GetEnumItemsAsync<T>(this IDefiningConfigKey configKey, IReadOnlyList<string> path, IReadOnlyList<string> groupKeys)
            where T : unmanaged, Enum
        {
            await Task.CompletedTask;

            if (typeof(T).GetCustomAttribute<FlagsAttribute>() is null)
            {
                var enumField = new DataFeedEnum<T>();
                enumField.InitBase(path, groupKeys, configKey);
                enumField.InitSetupValue(field => field.SetupConfigKeyField(configKey));

                yield return enumField;
                yield break;
            }

            var items = (IAsyncEnumerable<DataFeedItem>)_getFlagsEnumItems
                    .MakeGenericMethod(typeof(T))
                    .Invoke(null, [configKey, path, groupKeys]);

            await foreach (var item in items)
                yield return item;
        }

        public static async IAsyncEnumerable<DataFeedItem> GetFlagsEnumFieldsAsync<T>(this IDefiningConfigKey configKey, IReadOnlyList<string> path, IReadOnlyList<string> groupKeys)
            where T : unmanaged, Enum
        {
            await Task.CompletedTask;

            var flagsEnumGroup = new DataFeedGroup();
            flagsEnumGroup.InitBase(configKey.FullId + ".Flags", path, groupKeys, Mod.GetLocaleString("EnumFlags.Name", ("KeyId", configKey.Id)));
            flagsEnumGroup.InitDescription(configKey.GetLocaleString("Description"));
            yield return flagsEnumGroup;

            var flagsGrouping = groupKeys.Concat([$"{configKey.FullId}.Flags"]).ToArray();

            var enumType = typeof(T);

            foreach (var value in Enum.GetValues(enumType).Cast<T>())
            {
                var name = value.ToString();
                var longValue = Convert.ToInt64(value);

                var flagToggle = new DataFeedToggle();
                flagToggle.InitBase($"{configKey.FullId}.{name}", path, flagsGrouping, name);
                flagToggle.InitDescription(Mod.GetLocaleString("EnumToggle.Description", ("EnumName", enumType.Name), ("FlagName", name)));
                flagToggle.InitSetupValue(field =>
                {
                    var slot = field.FindNearestParent<Slot>();

                    if (slot.GetComponentInParents<FeedItemInterface>() is FeedItemInterface feedItemInterface)
                    {
                        // Adding the config key's full id to make it easier to create standalone facets
                        var comment = feedItemInterface.Slot.AttachComponent<Comment>();
                        comment.Text.Value = configKey.FullId;

                        var longField = feedItemInterface.Slot.AttachComponent<ValueField<long>>();
                        longField.Value.Value = longValue;
                    }

                    field.SyncWithConfigKeyEnumFlagUntyped(configKey, value);
                });

                yield return flagToggle;
            }
        }

        public static DataFeedIndicator<T> GetIndicatorItem<T>(this IDefiningConfigKey<T> configKey, IReadOnlyList<string> path, IReadOnlyList<string> groupKeys)
        {
            var indicator = new DataFeedIndicator<T>();
            indicator.InitBase(path, groupKeys, configKey);
            indicator.InitSetupValue(field => field.SetupConfigKeyField(configKey));

            return indicator;
        }

        public static async IAsyncEnumerable<DataFeedItem> GetNullableEnumItemsAsync<T>(this IDefiningConfigKey<T?> configKey, IReadOnlyList<string> path, IReadOnlyList<string> groupKeys)
            where T : unmanaged, Enum
        {
            await Task.CompletedTask;

            var nullableEnumGroup = new DataFeedGroup();
            nullableEnumGroup.InitBase(configKey.FullId + ".NullableGroup", path, groupKeys, configKey.GetLocaleString("Name"));
            nullableEnumGroup.InitDescription(configKey.GetLocaleString("Description"));
            yield return nullableEnumGroup;

            var nullableGroupKeys = groupKeys.Concat([configKey.FullId + ".NullableGroup"]).ToArray();

            var nullableToggle = new DataFeedToggle();

            nullableToggle.InitBase(configKey.FullId + ".HasValue", path, nullableGroupKeys, Mod.GetLocaleString("NullableEnumHasValue.Name"));
            nullableToggle.InitDescription(configKey.GetLocaleString("HasValue"));
            nullableToggle.InitSetupValue(field =>
            {
                var slot = field.FindNearestParent<Slot>();

                if (slot.GetComponentInParents<FeedItemInterface>() is FeedItemInterface feedItemInterface)
                    // Adding the config key's full id to make it easier to create standalone facets
                    feedItemInterface.Slot.AttachComponent<Comment>().Text.Value = configKey.FullId;

                field.SyncWithNullableConfigKeyHasValue(configKey);
            });
            yield return nullableToggle;

            var enumItems = (IAsyncEnumerable<DataFeedItem>)_getEnumItemsAsync
                    .MakeGenericMethod(typeof(T))
                    .Invoke(null, [path, nullableGroupKeys, configKey]);

            await foreach (var item in enumItems)
                yield return item;
        }

        public static DataFeedQuantityField<TQuantity, T> GetQuantityFieldItem<T, TQuantity>(IDefiningConfigKey<T> configKey, IConfigKeyQuantity<T> quantity, IReadOnlyList<string> path, IReadOnlyList<string> groupKeys)
            where TQuantity : unmanaged, IQuantity<TQuantity>
        {
            var quantityField = new DataFeedQuantityField<TQuantity, T>();
            quantityField.InitBase(path, groupKeys, configKey);
            quantityField.InitUnitConfiguration(quantity.DefaultConfiguration, quantity.ImperialConfiguration!);
            quantityField.InitSetup(quantityField => quantityField.SetupConfigKeyField(configKey), quantity.Min, quantity.Max);

            return quantityField;
        }

        public static DataFeedSlider<T> GetSliderItem<T>(this IDefiningConfigKey<T> configKey, IConfigKeyRange<T> range, IReadOnlyList<string> path, IReadOnlyList<string> groupKeys)
        {
            var slider = new DataFeedSlider<T>();
            slider.InitBase(path, groupKeys, configKey);
            slider.InitSetup(field => field.SetupConfigKeyField(configKey), range.Min, range.Max);

            //if (!string.IsNullOrWhiteSpace(configKey.TextFormat))
            //    slider.InitFormatting(configKey.TextFormat);

            return slider;
        }

        public static DataFeedToggle GetToggleItem(this IDefiningConfigKey<bool> configKey, IReadOnlyList<string> path, IReadOnlyList<string> groupKeys)
        {
            var toggle = new DataFeedToggle();
            toggle.InitBase(path, groupKeys, configKey);
            toggle.InitSetupValue(field => field.SetupConfigKeyField(configKey));

            return toggle;
        }

        public static DataFeedValueField<T> GetValueFieldItem<T>(this IDefiningConfigKey<T> configKey, IReadOnlyList<string> path, IReadOnlyList<string> groupKeys)
        {
            var valueField = new DataFeedValueField<T>();
            valueField.InitBase(path, groupKeys, configKey);
            valueField.InitSetupValue(field => field.SetupConfigKeyField(configKey));

            return valueField;
        }

        public static TDataFeedItem WithEnabledSource<TDataFeedItem>(this TDataFeedItem feedItem, IDefiningConfigKey<bool> enabledSource)
            where TDataFeedItem : DataFeedItem
        {
            feedItem.InitEnabled(enabledField => enabledField.SyncWithConfigKey(enabledSource, allowWriteBack: false));
            return feedItem;
        }
    }
}