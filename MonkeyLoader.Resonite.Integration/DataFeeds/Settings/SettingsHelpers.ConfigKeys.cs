using Elements.Core;
using Elements.Quantity;
using EnumerableToolkit;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Components;
using MonkeyLoader.Configuration;
using MonkeyLoader.Meta;
using MonkeyLoader.Resonite.Configuration;
using System.Reflection;

#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

namespace MonkeyLoader.Resonite.DataFeeds.Settings
{
    public static partial class SettingsHelpers
    {
        private static readonly Type _dummyType = typeof(dummy);

        private static readonly MethodInfo _enumerateConfigKeyItemsAsyncMethod = AccessTools.Method(typeof(SettingsHelpers), nameof(EnumerateConfigKeyItemsAsync));
        private static readonly MethodInfo _enumerateEnumDefaultItemsAsyncMethod = AccessTools.Method(typeof(SettingsHelpers), nameof(EnumerateEnumDefaultItemsAsync));
        private static readonly MethodInfo _enumerateNullableEnumDefaultItemsAsyncMethod = AccessTools.Method(typeof(SettingsHelpers), nameof(EnumerateNullableEnumDefaultItemsAsync));

        private static readonly MethodInfo _makeQuantityFieldMethod = AccessTools.FirstMethod(typeof(SettingsHelpers), method => method.GetParameters().Length is 4 && method.Name is nameof(MakeQuantityField));

        /// <summary>
        /// Enumerates the default data feed items - as opposed to the
        /// <see cref="ICustomDataFeedItems">custom items</see> - for this config section.
        /// </summary>
        /// <remarks>
        /// <see cref="IDefiningConfigKey{T}">Config keys</see> in <see cref="IConfigKeySubgroup">subgroups</see>
        /// are handled automatically using the <see cref="SubgroupGenerator"/>,
        /// and those with <see cref="IConfigKeyCustomDataFeedItems{T}">custom data feed items</see> will use those.
        /// </remarks>
        /// <param name="configSection">The config section to enumerate the default items for.</param>
        /// <param name="path">The path of the items being enumerated.</param>
        /// <param name="groupKeys">The group keys for the outer container that the items should be placed in.</param>
        /// <param name="searchPhrase">The search phrase for the enumeration request.</param>
        /// <param name="viewData">The custom view data for the enumeration request.</param>
        /// <returns>A sequence of the default data feed items for this config section.</returns>
        public static async IAsyncEnumerable<DataFeedItem> EnumerateDefaultItemsAsync(this ConfigSection configSection, IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, string? searchPhrase = null, object? viewData = null)
        {
            var subgroupGenerator = new SubgroupGenerator(configSection, path, groupKeys);

            foreach (var configKey in configSection.Keys.Where(key => !key.InternalAccessOnly))
            {
                var keyGroupKeys = groupKeys;

                if (configKey.Components.TryGet<IConfigKeySubgroup>(out var subgroupComponent))
                {
                    await foreach (var subgroupItem in subgroupGenerator.GetSubgroupEnumerator(subgroupComponent, out keyGroupKeys))
                        yield return subgroupItem;
                }

                var configKeyItems = (IAsyncEnumerable<DataFeedItem>)_enumerateConfigKeyItemsAsyncMethod
                    .MakeGenericMethod(configKey.ValueType)
                    .Invoke(null, [configKey, path, keyGroupKeys, searchPhrase, viewData])!;

                await foreach (var item in configKeyItems)
                    yield return item;
            }
        }

        /// <summary>
        /// Enumerates the default data feed items - as opposed to the
        /// <see cref="IConfigKeyCustomDataFeedItems{T}">custom items</see> - for this config key.
        /// </summary>
        /// <remarks>
        /// This will automatically pick the right <see cref="DataFeedItem">data feed item</see>(s) to represent the config key.
        /// </remarks>
        /// <param name="configKey">The config key to enumerate the default items for.</param>
        /// <returns>A sequence of the default data feed items for this config key.</returns>
        /// <inheritdoc cref="EnumerateDefaultItemsAsync(ConfigSection, IReadOnlyList{string}, IReadOnlyList{string}, string?, object?)"/>
        public static IAsyncEnumerable<DataFeedItem> EnumerateDefaultItemsAsync<T>(this IDefiningConfigKey<T> configKey, IReadOnlyList<string> path, IReadOnlyList<string> groupKeys)
        {
            IEntity<IDefiningConfigKey<T>> configKeyEntity = configKey;

            //if (setting is SettingIndicatorProperty)
            //{
            //    return (DataFeedItem)_generateIndicator.MakeGenericMethod(type).Invoke(null, new object[4] { identity, setting, path, grouping });
            //}

            if (configKey.ValueType == _dummyType)
            {
                var dummyField = new DataFeedValueField<dummy>();
                dummyField.InitBase(configKey.FullId, path, groupKeys, configKey.HasDescription ? configKey.GetLocaleString("Description") : " ");
                dummyField.InitSorting(-configKey.Priority);
                return dummyField.YieldAsync();
            }

            if (configKey.ValueType == typeof(bool))
                return ((IDefiningConfigKey<bool>)configKey).MakeToggle(path, groupKeys).YieldAsync();

            if (configKey.ValueType.IsEnum)
            {
                var enumItems = (IAsyncEnumerable<DataFeedItem>)_enumerateEnumDefaultItemsAsyncMethod
                    .MakeGenericMethod(configKey.ValueType)
                    .Invoke(null, [configKey, path, groupKeys])!;

                return enumItems;
            }

            if (configKey.ValueType.IsNullable())
            {
                var nullableType = configKey.ValueType.GetGenericArguments()[0];

                if (nullableType.IsEnum)
                {
                    var nullableEnumItems = (IAsyncEnumerable<DataFeedItem>)_enumerateNullableEnumDefaultItemsAsyncMethod
                    .MakeGenericMethod(nullableType)
                    .Invoke(null, [configKey, path, groupKeys])!;

                    return nullableEnumItems;
                }
            }

            if (configKeyEntity.Components.TryGet<IConfigKeyRange<T>>(out var range))
            {
                if (configKeyEntity.Components.TryGet<IConfigKeyQuantity<T>>(out var quantity))
                {
                    var quantityField = (DataFeedItem)_makeQuantityFieldMethod
                        .MakeGenericMethod(configKey.ValueType, quantity.QuantityType)
                        .Invoke(null, [configKey, quantity, path, groupKeys])!;

                    return quantityField.YieldAsync();
                }

                var slider = configKey.MakeSlider(range, path, groupKeys);
                return slider.YieldAsync();
            }

            var valueField = configKey.MakeValueField(path, groupKeys);
            return valueField.YieldAsync();
        }

#pragma warning disable IDE1006 // Naming Styles

        /// <summary>
        /// Creates an <see cref="Enum">enum</see> item for this config key.
        /// </summary>
        /// <remarks>
        /// This is not suitable for <see cref="FlagsAttribute">flags</see>-enums.<br/>
        /// Use <c><paramref name="configKey"/>.<see cref="EnumerateDefaultItemsAsync{T}(IDefiningConfigKey{T},
        /// IReadOnlyList{string}, IReadOnlyList{string})">EnumerateDefaultItemsAsync</see>(<paramref name="path"/>,
        /// <paramref name="groupKeys"/>)</c> instead.
        /// </remarks>
        /// <typeparam name="E">The type of the enum.</typeparam>
        /// <param name="configKey">The config key to create an <see cref="Enum">enum</see> item for.</param>
        /// <returns>The <see cref="Enum">enum</see> item for this config key.</returns>
        /// <inheritdoc cref="MakeToggle(IDefiningConfigKey{bool}, IReadOnlyList{string}, IReadOnlyList{string})"/>
        public static DataFeedEnum<E> MakeEnum<E>(this IDefiningConfigKey<E> configKey, IReadOnlyList<string> path, IReadOnlyList<string> groupKeys)
                where E : unmanaged, Enum
            => configKey.MakeEnumCore<E>(path, groupKeys);

        /// <inheritdoc cref="MakeEnum{E}(IDefiningConfigKey{E}, IReadOnlyList{string}, IReadOnlyList{string})"/>
        public static DataFeedEnum<E> MakeEnum<E>(this IDefiningConfigKey<E?> configKey, IReadOnlyList<string> path, IReadOnlyList<string> groupKeys)
                where E : unmanaged, Enum
            => configKey.MakeEnumCore<E>(path, groupKeys);

#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// Creates an indicator item for this config key.
        /// </summary>
        /// <remarks>
        /// So far, these only work for <see langword="string"/>s.
        /// </remarks>
        /// <typeparam name="T">The type of the config key and indicator.</typeparam>
        /// <param name="configKey">The config key to create a indicator item for.</param>
        /// <returns>The indicator item for this config key.</returns>
        /// <inheritdoc cref="MakeToggle(IDefiningConfigKey{bool}, IReadOnlyList{string}, IReadOnlyList{string})"/>
        public static DataFeedIndicator<T> MakeIndicator<T>(this IDefiningConfigKey<T> configKey, IReadOnlyList<string> path, IReadOnlyList<string> groupKeys)
        {
            var indicator = new DataFeedIndicator<T>();
            indicator.InitBase(path, groupKeys, configKey);
            indicator.InitSorting(-configKey.Priority);
            indicator.InitSetupValue(field => field.SetupConfigKeyField(configKey));

            return indicator;
        }

        /// <summary>
        /// Creates an quantity field item for this config key, using the given
        /// <see cref="IConfigKeyQuantity{T}">quantity component</see> for the configuration.
        /// </summary>
        /// <remarks>
        /// So far, these only work with numeric values,
        /// i.e. <see langword="int"/>, <see langword="float"/> and <see langword="double"/>.
        /// </remarks>
        /// <typeparam name="T">The type of the config key and quantity field's input.</typeparam>
        /// <typeparam name="TQuantity">The kind of quantity.</typeparam>
        /// <param name="configKey">The config key to create a quantity field item for.</param>
        /// <param name="quantity">The quantity component defining the configuration.</param>
        /// <returns>The quantity field item for this config key.</returns>
        /// <inheritdoc cref="MakeToggle(IDefiningConfigKey{bool}, IReadOnlyList{string}, IReadOnlyList{string})"/>
        public static DataFeedQuantityField<TQuantity, T> MakeQuantityField<T, TQuantity>(this IDefiningConfigKey<T> configKey, IConfigKeyQuantity<T> quantity, IReadOnlyList<string> path, IReadOnlyList<string> groupKeys)
                where TQuantity : unmanaged, IQuantity<TQuantity>
            => configKey.MakeQuantityField<T, TQuantity>(path, groupKeys, quantity.Min, quantity.Max, quantity.DefaultConfiguration, quantity.ImperialConfiguration);

        /// <summary>
        /// Creates an quantity field item for this config key,
        /// using the given parameters for the configuration.
        /// </summary>
        /// <param name="min">The lower bound for the quantity field.</param>
        /// <param name="max">The upper bound for the quantity field.</param>
        /// <param name="defaultConfiguration">The default unit configuration for the quantity field.</param>
        /// <param name="imperialConfiguration">The optional imperial unit configuration for the quantity field.</param>
        /// <inheritdoc cref="MakeQuantityField{T, TQuantity}(IDefiningConfigKey{T}, IConfigKeyQuantity{T}, IReadOnlyList{string}, IReadOnlyList{string})"/>
        public static DataFeedQuantityField<TQuantity, T> MakeQuantityField<T, TQuantity>(this IDefiningConfigKey<T> configKey, IReadOnlyList<string> path, IReadOnlyList<string> groupKeys,
                T min, T max, UnitConfiguration defaultConfiguration, UnitConfiguration? imperialConfiguration = null)
            where TQuantity : unmanaged, IQuantity<TQuantity>
        {
            var quantityField = new DataFeedQuantityField<TQuantity, T>();
            quantityField.InitBase(path, groupKeys, configKey);
            quantityField.InitSorting(-configKey.Priority);
            quantityField.InitUnitConfiguration(defaultConfiguration, imperialConfiguration!);
            quantityField.InitSetup(quantityField => quantityField.SetupConfigKeyField(configKey), min!, max!);

            return quantityField;
        }

        /// <summary>
        /// Creates a slider item for this config key, using the given
        /// <see cref="IConfigKeyRange{T}">range component</see> for the configuration.
        /// </summary>
        /// <remarks>
        /// So far, these only work with numeric values,
        /// i.e. <see langword="int"/>, <see langword="float"/> and <see langword="double"/>.
        /// </remarks>
        /// <typeparam name="T">The type of the config key and slider.</typeparam>
        /// <param name="configKey">The config key to create a slider item for.</param>
        /// <param name="range">The range component defining the configuration.</param>
        /// <returns>The slider item for this config key.</returns>
        /// <inheritdoc cref="MakeToggle(IDefiningConfigKey{bool}, IReadOnlyList{string}, IReadOnlyList{string})"/>
        public static DataFeedSlider<T> MakeSlider<T>(this IDefiningConfigKey<T> configKey, IConfigKeyRange<T> range, IReadOnlyList<string> path, IReadOnlyList<string> groupKeys)
            => configKey.MakeSlider(path, groupKeys, range.Min, range.Max);

        /// <summary>
        /// Creates a slider item for this config key,
        /// using the given parameters for the configuration.
        /// </summary>
        /// <param name="min">The lower bound for the slider.</param>
        /// <param name="max">The upper bound for the slider.</param>
        /// <inheritdoc cref="MakeSlider{T}(IDefiningConfigKey{T}, IConfigKeyRange{T}, IReadOnlyList{string}, IReadOnlyList{string})"/>
        public static DataFeedSlider<T> MakeSlider<T>(this IDefiningConfigKey<T> configKey, IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, T min, T max)
        {
            var slider = new DataFeedSlider<T>();
            slider.InitBase(path, groupKeys, configKey);
            slider.InitSorting(-configKey.Priority);
            slider.InitSetup(field => field.SetupConfigKeyField(configKey), min, max);

            //if (!string.IsNullOrWhiteSpace(configKey.TextFormat))
            //    slider.InitFormatting(configKey.TextFormat);

            return slider;
        }

        /// <summary>
        /// Creates a toggle item for this config key.
        /// </summary>
        /// <param name="configKey">The config key to create a toggle item for.</param>
        /// <param name="path">The path of the item being created.</param>
        /// <param name="groupKeys">The group keys for the outer container that the item should be placed in.</param>
        /// <returns>The toggle item for this config key.</returns>
        public static DataFeedToggle MakeToggle(this IDefiningConfigKey<bool> configKey, IReadOnlyList<string> path, IReadOnlyList<string> groupKeys)
        {
            var toggle = new DataFeedToggle();
            toggle.InitBase(path, groupKeys, configKey);
            toggle.InitSorting(-configKey.Priority);
            toggle.InitSetupValue(field => field.SetupConfigKeyField(configKey));

            return toggle;
        }

        /// <summary>
        /// Creates a value field item for this config key.
        /// </summary>
        /// <remarks/>
        /// <param name="configKey">The config key to create a value field item for.</param>
        /// <returns>The value field item for this config key.</returns>
        /// <inheritdoc cref="MakeToggle(IDefiningConfigKey{bool}, IReadOnlyList{string}, IReadOnlyList{string})"/>
        public static DataFeedValueField<T> MakeValueField<T>(this IDefiningConfigKey<T> configKey, IReadOnlyList<string> path, IReadOnlyList<string> groupKeys)
        {
            var valueField = new DataFeedValueField<T>();
            valueField.InitBase(path, groupKeys, configKey);
            valueField.InitSorting(-configKey.Priority);
            valueField.InitSetupValue(field => field.SetupConfigKeyField(configKey));

            return valueField;
        }

        private static IAsyncEnumerable<DataFeedItem> EnumerateConfigKeyItemsAsync<T>(IDefiningConfigKey<T> configKey, IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, string? searchPhrase = null, object? viewData = null)
        {
            IEntity<IDefiningConfigKey<T>> configKeyEntity = configKey;

            if (configKeyEntity.Components.TryGet<IConfigKeyCustomDataFeedItems<T>>(out var customItems))
                return customItems.Enumerate(path, groupKeys, searchPhrase, viewData);

            return configKey.EnumerateDefaultItemsAsync(path, groupKeys);
        }

        private static IAsyncEnumerable<DataFeedItem> EnumerateEnumDefaultItemsAsync<T>(this IDefiningConfigKey configKey, IReadOnlyList<string> path, IReadOnlyList<string> groupKeys)
            where T : unmanaged, Enum
        {
            if (typeof(T).GetCustomAttribute<FlagsAttribute>() is null)
                return configKey.MakeEnumCore<T>(path, groupKeys).YieldAsync();

            return EnumerateFlagsEnumItemsAsync<T>(configKey, path, groupKeys);
        }

        private static async IAsyncEnumerable<DataFeedItem> EnumerateFlagsEnumItemsAsync<T>(IDefiningConfigKey configKey, IReadOnlyList<string> path, IReadOnlyList<string> groupKeys)
            where T : unmanaged, Enum
        {
            await Task.CompletedTask;

            var flagsEnumGroup = new DataFeedGroup();
            flagsEnumGroup.InitBase(configKey.FullId + ".Flags", path, groupKeys, Mod.GetLocaleString("EnumFlags.Name", ("KeyId", configKey.Id)));
            flagsEnumGroup.InitSorting(-configKey.Priority);
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

        private static async IAsyncEnumerable<DataFeedItem> EnumerateNullableEnumDefaultItemsAsync<T>(this IDefiningConfigKey<T?> configKey, IReadOnlyList<string> path, IReadOnlyList<string> groupKeys)
            where T : unmanaged, Enum
        {
            await Task.CompletedTask;

            var nullableEnumGroup = new DataFeedGroup();
            nullableEnumGroup.InitSorting(-configKey.Priority);
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
                {
                    // Adding the config key's full id to make it easier to create standalone facets
                    feedItemInterface.Slot.AttachComponent<Comment>().Text.Value = configKey.FullId;
                }

                field.SyncWithNullableConfigKeyHasValue(configKey);
            });
            yield return nullableToggle;

            await foreach (var item in EnumerateEnumDefaultItemsAsync<T>(configKey, path, groupKeys))
                yield return item;
        }

        private static DataFeedEnum<T> MakeEnumCore<T>(this IDefiningConfigKey configKey, IReadOnlyList<string> path, IReadOnlyList<string> groupKeys)
            where T : unmanaged, Enum
        {
            var enumField = new DataFeedEnum<T>();
            enumField.InitBase(path, groupKeys, configKey);
            enumField.InitSorting(-configKey.Priority);
            enumField.InitSetupValue(field => field.SetupConfigKeyField(configKey));

            return enumField;
        }
    }
}

#pragma warning restore CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)