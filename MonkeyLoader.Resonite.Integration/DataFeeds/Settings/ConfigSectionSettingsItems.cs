using Elements.Assets;
using Elements.Core;
using Elements.Quantity;
using EnumerableToolkit;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Components;
using MonkeyLoader.Configuration;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using MonkeyLoader.Resonite.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.DataFeeds.Settings
{
    internal sealed class ConfigSectionSettingsItems : DataFeedBuildingBlockMonkey<ConfigSectionSettingsItems, SettingsDataFeed>
    {
        private static readonly Type _dummyType = typeof(dummy);
        private static readonly MethodInfo _generateEnumItemsAsync = AccessTools.Method(typeof(ConfigSectionSettingsItems), nameof(GenerateEnumItemsAsync));
        private static readonly MethodInfo _generateFlagsEnumItems = AccessTools.Method(typeof(ConfigSectionSettingsItems), nameof(GenerateFlagsEnumFields));
        private static readonly MethodInfo _generateItemsForConfigKey = AccessTools.Method(typeof(ConfigSectionSettingsItems), nameof(GenerateItemsForConfigKey));
        private static readonly MethodInfo _generateNullableEnumItemsAsync = AccessTools.Method(typeof(ConfigSectionSettingsItems), nameof(GenerateNullableEnumItemsAsync));
        private static readonly MethodInfo _generateQuantityField = AccessTools.Method(typeof(ConfigSectionSettingsItems), nameof(GenerateQuantityField));

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

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => [];

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
                            .Invoke(null, [parameters, sectionGrouping, configKey]);

                        await foreach (var item in configKeyItems)
                            yield return item;
                    }
                }
            }
        }

        private static async IAsyncEnumerable<DataFeedItem> GenerateEnumItemsAsync<T>(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, IDefiningConfigKey configKey)
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

            var items = (IAsyncEnumerable<DataFeedItem>)_generateFlagsEnumItems
                    .MakeGenericMethod(typeof(T))
                    .Invoke(null, [path, groupKeys, configKey]);

            await foreach (var item in items)
                yield return item;
        }

        private static async IAsyncEnumerable<DataFeedItem> GenerateFlagsEnumFields<T>(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, IDefiningConfigKey configKey)
            where T : unmanaged, Enum
        {
            await Task.CompletedTask;

            var flagsEnumGroup = new DataFeedGroup();
            flagsEnumGroup.InitBase(configKey.FullId + ".Flags", path, groupKeys, Mod.GetLocaleString("EnumFlags.Name", ("KeyId", configKey.Id)));
            flagsEnumGroup.InitDescription(configKey.GetLocaleKey("Description").AsLocaleKey());
            yield return flagsEnumGroup;

            var flagsGrouping = groupKeys.Concat([configKey.FullId + ".Flags"]).ToArray();

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

        private static DataFeedIndicator<T> GenerateIndicator<T>(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, IDefiningConfigKey<T> configKey)
        {
            var indicator = new DataFeedIndicator<T>();
            indicator.InitBase(path, groupKeys, configKey);
            indicator.InitSetupValue(field => field.SetupConfigKeyField(configKey));

            return indicator;
        }

        private static IAsyncEnumerable<DataFeedItem> GenerateItemsForConfigKey<T>(EnumerateDataFeedParameters<SettingsDataFeed> parameters, IReadOnlyList<string> groupKeys, IEntity<IDefiningConfigKey<T>> configKeyEntity)
        {
            var configKey = configKeyEntity.Self;
            var path = parameters.Path;

            if (configKeyEntity.Components.TryGet<IConfigKeyCustomDataFeedItems<T>>(out var customItems))
                return customItems.Enumerate(path, groupKeys, parameters.SearchPhrase, parameters.ViewData);

            //if (setting is SettingIndicatorProperty)
            //{
            //    return (DataFeedItem)_generateIndicator.MakeGenericMethod(type).Invoke(null, new object[4] { identity, setting, path, grouping });
            //}

            if (configKey.ValueType == _dummyType)
            {
                var dummyField = new DataFeedValueField<dummy>();
                dummyField.InitBase(configKey.FullId, path, groupKeys, configKey.HasDescription ? configKey.GetLocaleString("Description") : " ");
                return dummyField.YieldAsync();
            }

            if (configKey.ValueType == typeof(bool))
                return GenerateToggle(path, groupKeys, (IDefiningConfigKey<bool>)configKey).YieldAsync();

            if (configKey.ValueType.IsEnum)
            {
                var enumItems = (IAsyncEnumerable<DataFeedItem>)_generateEnumItemsAsync
                    .MakeGenericMethod(configKey.ValueType)
                    .Invoke(null, [path, groupKeys, configKey]);

                return enumItems;
            }

            if (configKey.ValueType.IsNullable())
            {
                var nullableType = configKey.ValueType.GetGenericArguments()[0];
                if (nullableType.IsEnum)
                {
                    var nullableEnumItems = (IAsyncEnumerable<DataFeedItem>)_generateNullableEnumItemsAsync
                    .MakeGenericMethod(nullableType)
                    .Invoke(null, [path, groupKeys, configKey]);

                    return nullableEnumItems;
                }
            }

            if (configKeyEntity.Components.TryGet<IConfigKeyRange<T>>(out var range))
            {
                if (configKeyEntity.Components.TryGet<IConfigKeyQuantity<T>>(out var quantity))
                {
                    var quantityField = (DataFeedItem)_generateQuantityField
                        .MakeGenericMethod(configKey.ValueType, quantity.QuantityType)
                        .Invoke(null, [parameters.Path, groupKeys, configKey, quantity]);

                    return quantityField.YieldAsync();
                }

                var slider = GenerateSlider(parameters.Path, groupKeys, configKey, range);
                return slider.YieldAsync();
            }

            var valueField = GenerateValueField(parameters, groupKeys, configKey);
            return valueField.YieldAsync();
        }

        private static async IAsyncEnumerable<DataFeedItem> GenerateNullableEnumItemsAsync<T>(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, IDefiningConfigKey<T?> configKey)
            where T : unmanaged, Enum
        {
            await Task.CompletedTask;

            var nullableEnumGroup = new DataFeedGroup();
            nullableEnumGroup.InitBase(configKey.FullId + ".NullableGroup", path, groupKeys, configKey.GetLocaleKey("Name").AsLocaleKey());
            nullableEnumGroup.InitDescription(configKey.GetLocaleKey("Description").AsLocaleKey());
            yield return nullableEnumGroup;

            var nullableGroupKeys = groupKeys.Concat([configKey.FullId + ".NullableGroup"]).ToArray();

            var nullableToggle = new DataFeedToggle();

            nullableToggle.InitBase(configKey.FullId + ".HasValue", path, nullableGroupKeys, Mod.GetLocaleString("NullableEnumHasValue.Name"));
            nullableToggle.InitDescription(configKey.GetLocaleKey("HasValue").AsLocaleKey());
            nullableToggle.InitSetupValue(field =>
            {
                var slot = field.FindNearestParent<Slot>();

                if (slot.GetComponentInParents<FeedItemInterface>() is FeedItemInterface feedItemInterface)
                {
                    // Adding the config key's full id to make it easier to create standalone facets
                    feedItemInterface.Slot.AttachComponent<Comment>().Text.Value = configKey.FullId;
                }

                field.SyncWithNullableConfigKeyHasValue(configKey);
            });
            yield return nullableToggle;

            var enumItems = (IAsyncEnumerable<DataFeedItem>)_generateEnumItemsAsync
                    .MakeGenericMethod(typeof(T))
                    .Invoke(null, [path, nullableGroupKeys, configKey]);

            await foreach (var item in enumItems)
                yield return item;
        }

        private static DataFeedQuantityField<TQuantity, T> GenerateQuantityField<T, TQuantity>(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, IDefiningConfigKey<T> configKey, IConfigKeyQuantity<T> quantity)
            where TQuantity : unmanaged, IQuantity<TQuantity>
        {
            var quantityField = new DataFeedQuantityField<TQuantity, T>();
            quantityField.InitBase(path, groupKeys, configKey);
            quantityField.InitUnitConfiguration(quantity.DefaultConfiguration, quantity.ImperialConfiguration!);
            quantityField.InitSetup(quantityField => quantityField.SetupConfigKeyField(configKey), quantity.Min, quantity.Max);

            return quantityField;
        }

        private static DataFeedSlider<T> GenerateSlider<T>(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, IDefiningConfigKey<T> configKey, IConfigKeyRange<T> range)
        {
            var slider = new DataFeedSlider<T>();
            slider.InitBase(path, groupKeys, configKey);
            slider.InitSetup(field => field.SetupConfigKeyField(configKey), range.Min, range.Max);

            //if (!string.IsNullOrWhiteSpace(configKey.TextFormat))
            //    slider.InitFormatting(configKey.TextFormat);

            return slider;
        }

        private static DataFeedToggle GenerateToggle(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, IDefiningConfigKey<bool> configKey)
        {
            var toggle = new DataFeedToggle();
            toggle.InitBase(path, groupKeys, configKey);
            toggle.InitSetupValue(field => field.SetupConfigKeyField(configKey));

            return toggle;
        }

        private static DataFeedValueField<T> GenerateValueField<T>(EnumerateDataFeedParameters<SettingsDataFeed> parameters, IReadOnlyList<string> groupKeys, IDefiningConfigKey<T> configKey)
        {
            var valueField = new DataFeedValueField<T>();
            valueField.InitBase(parameters.Path, groupKeys, configKey);
            valueField.InitSetupValue(field => field.SetupConfigKeyField(configKey));

            if (configKey.ValueType.IsInjectableEditorType())
                parameters.DataFeed.RunSynchronously(() => parameters.DataFeed.GetViewData().EnsureDataFeedValueFieldTemplate(configKey.ValueType));

            return valueField;
        }
    }
}