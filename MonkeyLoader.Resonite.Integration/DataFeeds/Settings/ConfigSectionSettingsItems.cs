using Elements.Assets;
using Elements.Core;
using Elements.Quantity;
using EnumerableToolkit;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using MonkeyLoader.Components;
using MonkeyLoader.Configuration;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using MonkeyLoader.Resonite.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.DataFeeds.Settings
{
    internal sealed class ConfigSectionSettingsItems : DataFeedBuildingBlockMonkey<ConfigSectionSettingsItems, SettingsDataFeed>
    {
        private static readonly Type _dummyType = typeof(dummy);
        private static readonly MethodInfo _generateEnumField = AccessTools.Method(typeof(ConfigSectionSettingsItems), nameof(GenerateEnumField));
        private static readonly MethodInfo _generateFlagsEnumFields = AccessTools.Method(typeof(ConfigSectionSettingsItems), nameof(GenerateFlagsEnumFields));
        private static readonly MethodInfo _generateItemForConfigKeyMethod = AccessTools.Method(typeof(ConfigSectionSettingsItems), nameof(GenerateItemForConfigKey));
        private static readonly MethodInfo _generateQuantityField = AccessTools.Method(typeof(ConfigSectionSettingsItems), nameof(GenerateQuantityField));
        private static EnumerateDataFeedParameters<SettingsDataFeed>? _currentParameters;
        public override int Priority => 400;

        public override IAsyncEnumerable<DataFeedItem> Apply(IAsyncEnumerable<DataFeedItem> current, EnumerateDataFeedParameters<SettingsDataFeed> parameters)
        {
            _currentParameters = parameters;
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
            foreach (var configSection in config.Sections.Where(section => !section.InternalAccessOnly))
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
                    await foreach (var item in EnumerateConfigSectionAsync(parameters.Path, sectionGrouping, configSection))
                        yield return item;
                }
            }
        }

        private static async IAsyncEnumerable<DataFeedItem> EnumerateConfigSectionAsync(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, ConfigSection configSection)
        {
            await Task.CompletedTask;

            foreach (var configKey in configSection.Keys.Where(key => !key.InternalAccessOnly))
            {
                // Add check for ConfigKeyCustomDataFeedItems

                //if (setting is SettingIndicatorProperty)
                //{
                //    return (DataFeedItem)_generateIndicator.MakeGenericMethod(type).Invoke(null, new object[4] { identity, setting, path, grouping });
                //}
                if (configKey.ValueType == _dummyType)
                {
                    var dummyField = new DataFeedValueField<dummy>();
                    dummyField.InitBase(configKey.FullId, path, groupKeys, configKey.HasDescription ? configKey.GetLocaleString("Description") : " ");
                    yield return dummyField;

                    continue;
                }

                if (configKey.ValueType == typeof(bool))
                {
                    yield return GenerateToggle(path, groupKeys, (IDefiningConfigKey<bool>)configKey);

                    continue;
                }

                if (configKey.ValueType.IsEnum)
                {
                    if (configKey.ValueType.GetCustomAttribute<FlagsAttribute>() is null)
                    {
                        yield return (DataFeedItem)_generateEnumField
                            .MakeGenericMethod(configKey.ValueType)
                            .Invoke(null, [path, groupKeys, configKey]);

                        continue;
                    }

                    var items = (IEnumerable<DataFeedItem>)_generateFlagsEnumFields
                        .MakeGenericMethod(configKey.ValueType)
                        .Invoke(null, [path, groupKeys, configKey]);

                    foreach (var item in items)
                        yield return item;

                    continue;
                }

                yield return (DataFeedItem)_generateItemForConfigKeyMethod
                    .MakeGenericMethod(configKey.ValueType)
                    .Invoke(null, [path, groupKeys, configKey]);
            }
        }

        private static DataFeedEnum<T> GenerateEnumField<T>(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, IDefiningConfigKey<T> configKey)
            where T : Enum
        {
            var enumField = new DataFeedEnum<T>();
            enumField.InitBase(path, groupKeys, configKey);
            enumField.InitSetupValue(field => field.SetupConfigKeyField(configKey));

            return enumField;
        }

        private static IEnumerable<DataFeedItem> GenerateFlagsEnumFields<T>(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, IDefiningConfigKey<T> configKey)
            where T : Enum
        {
            var flagsEnumGroup = new DataFeedGroup();
            flagsEnumGroup.InitBase(configKey.FullId, path, groupKeys, configKey.GetLocaleKey("Name").AsLocaleKey());
            flagsEnumGroup.InitDescription(configKey.GetLocaleKey("Description").AsLocaleKey());
            yield return flagsEnumGroup;

            var flagsGrouping = groupKeys.Concat(configKey.FullId).ToArray();

            foreach (var value in Enum.GetValues(configKey.ValueType).Cast<T>())
            {
                var name = value.ToString();
                var longValue = Convert.ToInt64(value);

                var flagToggle = new DataFeedToggle();
                flagToggle.InitBase($"{configKey.FullId}.{name}", path, flagsGrouping, name);
                flagToggle.InitDescription(Mod.GetLocaleString("EnumToggle.Description", ("EnumName", configKey.ValueType.Name), ("FlagName", name)));
                flagToggle.InitSetupValue(field =>
                {
                    field.Value = (Convert.ToInt64(configKey.GetValue()) & longValue) == longValue;

                    void FieldChanged(IChangeable changeable)
                        => configKey.TrySetValue(Enum.ToObject(configKey.ValueType, field.Value ? Convert.ToInt64(configKey.GetValue()) | longValue : Convert.ToInt64(configKey.GetValue()) & ~longValue));

                    void KeyChanged(object sender, ConfigKeyChangedEventArgs<T> changedEvent)
                    {
                        var newValue = Convert.ToInt64(changedEvent.NewValue);
                        var isPartialCombinedValue = (newValue & longValue) != 0;

                        field.World.RunSynchronously(() =>
                        {
                            if (isPartialCombinedValue)
                                field.Changed -= FieldChanged;

                            field.Value = (newValue & longValue) == longValue;

                            if (isPartialCombinedValue)
                                field.Changed += FieldChanged;
                        });
                    }

                    field.Changed += FieldChanged;
                    configKey.Changed += KeyChanged;
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

        private static DataFeedItem GenerateItemForConfigKey<T>(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, IEntity<IDefiningConfigKey<T>> configKey)
        {
            if (configKey.Components.TryGet<IConfigKeyRange<T>>(out var range))
            {
                if (configKey.Components.TryGet<IConfigKeyQuantity<T>>(out var quantity))
                {
                    return (DataFeedItem)_generateQuantityField
                        .MakeGenericMethod(configKey.Self.ValueType, quantity.QuantityType)
                        .Invoke(null, [path, groupKeys, configKey, quantity]);
                }

                return GenerateSlider(path, groupKeys, configKey.Self, range);
            }

            return GenerateValueField(path, groupKeys, configKey.Self);
        }

        private static DataFeedQuantityField<TQuantity, T> GenerateQuantityField<T, TQuantity>(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, IDefiningConfigKey<T> configKey, IConfigKeyQuantity<T> quantity)
                    where TQuantity : unmanaged, IQuantity<TQuantity>
        {
            var quantityField = new DataFeedQuantityField<TQuantity, T>();
            quantityField.InitBase(path, groupKeys, configKey);
            quantityField.InitUnitConfiguration(quantity.DefaultConfiguration, quantity.ImperialConfiguration);
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

        private static DataFeedValueField<T> GenerateValueField<T>(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, IDefiningConfigKey<T> configKey)
        {
            var valueField = new DataFeedValueField<T>();
            valueField.InitBase(path, groupKeys, configKey);
            valueField.InitSetupValue(field => field.SetupConfigKeyField(configKey));

            var valueType = typeof(T);
            if (valueType != typeof(dummy) && (Coder<T>.IsEnginePrimitive || valueType == typeof(Type)))
            {
                if (_currentParameters!.DataFeed is SettingsDataFeed settingsDataFeed)
                {
                    var settingsViewData = SettingsHelpers.GetViewData(_currentParameters!.DataFeed);
                    settingsViewData.Mapper?.RunSynchronously(() => settingsViewData.EnsureDataFeedValueFieldTemplate(valueType));
                }
            }

            return valueField;
        }
    }
}