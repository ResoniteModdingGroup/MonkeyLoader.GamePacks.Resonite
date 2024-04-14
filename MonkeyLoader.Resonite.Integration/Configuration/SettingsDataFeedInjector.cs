using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Configuration;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using MonkeyLoader.Resonite.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.Configuration
{
    [HarmonyPatch(typeof(SettingsDataFeed))]
    [HarmonyPatchCategory(nameof(SettingsDataFeedInjector))]
    internal sealed class SettingsDataFeedInjector : ResoniteAsyncEventHandlerMonkey<SettingsDataFeedInjector, FallbackLocaleGenerationEvent>
    {
        public const string ConfigKeyChangeLabel = "Settings";
        public const string ConfigSections = "ConfigSections";

        public const string MonkeyToggles = "MonkeyToggles";

        private const string EarlyMonkeys = "EarlyMonkeys";
        private const string Monkeys = "Monkeys";
        private static readonly MethodInfo _generateEnumField = AccessTools.Method(typeof(SettingsDataFeedInjector), nameof(GenerateEnumField));

        private static readonly MethodInfo _generateSlider = AccessTools.Method(typeof(SettingsDataFeedInjector), nameof(GenerateSlider));
        private static readonly MethodInfo _generateValueField = AccessTools.Method(typeof(SettingsDataFeedInjector), nameof(GenerateValueField));

        public override int Priority => HarmonyLib.Priority.Normal;

        protected override bool AppliesTo(FallbackLocaleGenerationEvent eventData) => true;

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => Enumerable.Empty<IFeaturePatch>();

        protected override Task Handle(FallbackLocaleGenerationEvent eventData)
        {
            foreach (var mod in Mod.Loader.Mods)
            {
                var modNameKey = $"{mod.Id}.Name";

                eventData.AddMessage(modNameKey, mod.Title);
                eventData.AddMessage($"Settings.{mod.Id}.Breadcrumb", eventData.GetMessage(modNameKey));

                foreach (var configSection in mod.Config.Sections)
                {
                    eventData.AddMessage($"{configSection.FullId}.Name", configSection.Name);

                    foreach (var configKey in configSection.Keys)
                    {
                        eventData.AddMessage($"{configKey.FullId}.Name", configKey.Id);
                        eventData.AddMessage($"{configKey.FullId}.Description", configKey.Description ?? "No Description");
                    }
                }

                foreach (var monkey in mod.Monkeys)
                {
                    eventData.AddMessage($"{mod.Id}.{monkey.Name}.Name", monkey.Name);
                    eventData.AddMessage($"{mod.Id}.{monkey.Name}.Description", "No Description");
                }

                foreach (var earlyMonkey in mod.EarlyMonkeys)
                {
                    eventData.AddMessage($"{mod.Id}.{earlyMonkey.Name}.Name", earlyMonkey.Name);
                    eventData.AddMessage($"{mod.Id}.{earlyMonkey.Name}.Description", "No Description");
                }
            }

            return Task.CompletedTask;
        }

        protected override bool OnEngineReady()
        {
            var monkeyLoaderCategory = new SettingCategoryInfo(OfficialAssets.Graphics.Icons.Dash.Tools, 255);
            monkeyLoaderCategory.InitKey("MonkeyLoader");

            Settings._categoryInfos.Add(monkeyLoaderCategory.Key, monkeyLoaderCategory);

            return base.OnEngineReady();
        }

        private static async IAsyncEnumerable<DataFeedItem> EnumerateConfigSectionAsync(IReadOnlyList<string> path, ConfigSection configSection)
        {
            foreach (var configKey in configSection.Keys.Where(key => !key.InternalAccessOnly))
            {
                //if (setting is SettingIndicatorProperty)
                //{
                //    return (DataFeedItem)_generateIndicator.MakeGenericMethod(type).Invoke(null, new object[4] { identity, setting, path, grouping });
                //}

                if (configKey.ValueType == typeof(bool))
                {
                    yield return GenerateToggle(path, (IDefiningConfigKey<bool>)configKey);
                    continue;
                }

                if (configKey.ValueType.IsEnum)
                {
                    yield return (DataFeedItem)_generateEnumField
                        .MakeGenericMethod(configKey.ValueType)
                        .Invoke(null, new object[] { path, configKey });
                    continue;
                }

                if (configKey is IRangedDefiningKey)
                {
                    yield return (DataFeedItem)_generateSlider
                        .MakeGenericMethod(configKey.ValueType)
                        .Invoke(null, new object[] { path, configKey });
                }

                //QuantityAttribute customAttribute2 = identity.field.GetCustomAttribute<QuantityAttribute>();
                //if (customAttribute2 != null)
                //{
                //    return (DataFeedItem)_generateQuantityField.MakeGenericMethod(customAttribute2.QuantityType, type).Invoke(null, new object[6] { identity, setting, customAttribute2, customAttribute, path, grouping });
                //}

                yield return (DataFeedItem)_generateValueField.MakeGenericMethod(configKey.ValueType).Invoke(null, new object[] { path, configKey });
            }
        }

        private static async IAsyncEnumerable<DataFeedItem> EnumerateModsAsync(IReadOnlyList<string> path)
        {
            foreach (var mod in Mod.Loader.Mods)
            {
                var modGroup = new DataFeedGroup();
                modGroup.InitBase(mod.Id, path, null, $"{mod.Id}.Name".AsLocaleKey(), $"{Mod.Id}.Mod.Description");
                yield return modGroup;

                var grouping = new[] { mod.Id };

                var monkeysCategory = new DataFeedCategory();
                monkeysCategory.InitBase($"{mod.Id}.{MonkeyToggles}", path, grouping, $"{Mod.Id}.Mod.Open{MonkeyToggles}".AsLocaleKey());
                monkeysCategory.SetOverrideSubpath(mod.Id, MonkeyToggles);
                yield return monkeysCategory;

                var configSectionsCategory = new DataFeedCategory();
                configSectionsCategory.InitBase($"{mod.Id}.{ConfigSections}", path, grouping, $"{Mod.Id}.Mod.Open{ConfigSections}".AsLocaleKey());
                configSectionsCategory.SetOverrideSubpath(mod.Id, ConfigSections);
                yield return configSectionsCategory;
            }
        }

        private static async IAsyncEnumerable<DataFeedItem> EnumerateModSettingsAsync(IReadOnlyList<string> path)
        {
            // path.Count >= 3 because otherwise other methods are called
            // Format: MonkeyLoader / modId / {page}
            if (Mod.Loader.Mods.FirstOrDefault(mod => mod.Id == path[1]) is not Mod mod)
            {
                Logger.Error(() => $"Tried to access non-existant mod's settings: {path[1]}");
                yield break;
            }

            switch (path[2])
            {
                case MonkeyToggles:
                    var monkeysGroup = new DataFeedGroup();
                    monkeysGroup.InitBase(Monkeys, path, null, $"{Mod.Id}.{Monkeys}.Name".AsLocaleKey(), $"{Mod.Id}.{Monkeys}.Description".AsLocaleKey());
                    yield return monkeysGroup;

                    var monkeysGrouping = new[] { Monkeys };

                    var monkeyCount = new DataFeedIndicator<string>();
                    monkeyCount.InitBase($"{Monkeys}.Count", path, monkeysGrouping, $"{Mod.Id}.{Monkeys}.Count.Name".AsLocaleKey(), $"{Mod.Id}.{Monkeys}.Count.Description".AsLocaleKey());
                    monkeyCount.InitSetupValue(field => field.Value = mod.Monkeys.Count().ToString());
                    yield return monkeyCount;

                    foreach (var monkey in mod.Monkeys)
                    {
                        var monkeyGroup = new DataFeedGroup();
                        monkeyGroup.InitBase($"{monkey.Name}", path, monkeysGrouping, $"{mod.Id}.{monkey.Name}.Name".AsLocaleKey(), $"{mod.Id}.{monkey.Name}.Description".AsLocaleKey());
                        yield return monkeyGroup;

                        var monkeyGrouping = new[] { Monkeys, monkey.Name };

                        var toggle = new DataFeedToggle();
                        toggle.InitBase($"{monkey.Name}.Enabled", path, monkeyGrouping, $"{Mod.Id}.{Monkeys}.Enabled.Name".AsLocaleKey(), $"{Mod.Id}.{Monkeys}.Enabled.Description".AsLocaleKey());
                        toggle.InitSetupValue(field => field.Value = true);
                        yield return toggle;

                        var typeIndicator = new DataFeedIndicator<string>();
                        typeIndicator.InitBase($"{monkey.Name}.Type", path, monkeyGrouping, $"{Mod.Id}.{Monkeys}.Type.Name".AsLocaleKey(), $"{Mod.Id}.{Monkeys}.Type.Description".AsLocaleKey());
                        typeIndicator.InitSetupValue(field => field.Value = monkey.GetType().BaseType.Name);
                        yield return typeIndicator;
                    }

                    var earlyMonkeysGroup = new DataFeedGroup();
                    earlyMonkeysGroup.InitBase(EarlyMonkeys, path, null, $"{Mod.Id}.{EarlyMonkeys}.Name".AsLocaleKey(), $"{Mod.Id}.{EarlyMonkeys}.Description".AsLocaleKey());
                    yield return earlyMonkeysGroup;

                    var earlyMonkeysGrouping = new[] { EarlyMonkeys };

                    var earlyMonkeyCount = new DataFeedIndicator<string>();
                    earlyMonkeyCount.InitBase($"{EarlyMonkeys}.Count", path, earlyMonkeysGrouping, $"{Mod.Id}.{EarlyMonkeys}.Count.Name".AsLocaleKey(), $"{Mod.Id}.{EarlyMonkeys}.Count.Description".AsLocaleKey());
                    earlyMonkeyCount.InitSetupValue(field => field.Value = mod.EarlyMonkeys.Count().ToString());
                    yield return earlyMonkeyCount;

                    foreach (var earlyMonkey in mod.EarlyMonkeys)
                    {
                        var earlyMonkeyGroup = new DataFeedGroup();
                        earlyMonkeyGroup.InitBase($"{earlyMonkey.Name}", path, monkeysGrouping, $"{mod.Id}.{earlyMonkey.Name}.Name".AsLocaleKey(), $"{mod.Id}.{earlyMonkey.Name}.Description".AsLocaleKey());
                        yield return earlyMonkeyGroup;

                        var earlyMonkeyGrouping = new[] { EarlyMonkeys, earlyMonkey.Name };

                        var toggle = new DataFeedToggle();
                        toggle.InitBase($"{earlyMonkey.Name}.Enabled", path, earlyMonkeyGrouping, $"{Mod.Id}.{EarlyMonkeys}.Enabled.Name".AsLocaleKey(), $"{Mod.Id}.{EarlyMonkeys}.Enabled.Description".AsLocaleKey());
                        toggle.InitSetupValue(field => field.Value = true);
                        yield return toggle;
                    }

                    break;

                case ConfigSections:
                    foreach (var configSection in mod.Config.Sections)
                    {
                        var sectionGroup = new DataFeedGroup();
                        sectionGroup.InitBase(configSection.Id, path, null, $"{configSection.FullId}.Name".AsLocaleKey());
                        yield return sectionGroup;

                        await foreach (var sectionItem in EnumerateConfigSectionAsync(path, configSection))
                            yield return sectionItem;
                    }
                    break;

                default:
                    Logger.Error(() => $"Tried to access non-existant mod settings page: {path[2]}");
                    break;
            }
        }

        private static async IAsyncEnumerable<DataFeedItem> EnumerateMonkeyLoaderSettingsAsync(IReadOnlyList<string> path)
        {
            yield break;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SettingsDataFeed.Enumerate))]
        private static bool EnumeratePrefix(IReadOnlyList<string> path, ref IAsyncEnumerable<DataFeedItem> __result)
        {
            if (path.Count == 0 || path[0] != "MonkeyLoader")
                return true;

            if (path.Count == 1)
                __result = EnumerateModsAsync(path);
            else if (path.Count == 2)
                __result = EnumerateMonkeyLoaderSettingsAsync(path);
            else if (path.Count >= 3)
                __result = EnumerateModSettingsAsync(path);

            return false;
        }

        private static DataFeedEnum<T> GenerateEnumField<T>(IReadOnlyList<string> path, IDefiningConfigKey<T> configKey)
            where T : Enum
        {
            var enumField = new DataFeedEnum<T>();
            InitBase(enumField, path, configKey);
            enumField.InitSetupValue(field => field.SyncWithConfigKey(configKey, ConfigKeyChangeLabel));

            return enumField;
        }

        private static DataFeedIndicator<T> GenerateIndicator<T>(IReadOnlyList<string> path, IDefiningConfigKey<T> configKey)
        {
            var indicator = new DataFeedIndicator<T>();
            InitBase(indicator, path, configKey);
            indicator.InitSetupValue(field => field.SyncWithConfigKey(configKey, ConfigKeyChangeLabel));

            return indicator;
        }

        private static DataFeedSlider<T> GenerateSlider<T>(IReadOnlyList<string> path, IRangedDefiningKey<T> configKey)
        {
            var slider = new DataFeedSlider<T>();
            InitBase(slider, path, configKey);
            slider.InitSetup(field => field.SyncWithConfigKey(configKey, ConfigKeyChangeLabel), configKey.Min, configKey.Max);

            //if (!string.IsNullOrWhiteSpace(configKey.TextFormat))
            //    slider.InitFormatting(configKey.TextFormat);

            return slider;
        }

        private static DataFeedToggle GenerateToggle(IReadOnlyList<string> path, IDefiningConfigKey<bool> configKey)
        {
            var toggle = new DataFeedToggle();
            InitBase(toggle, path, configKey);
            toggle.InitSetupValue(field => field.SyncWithConfigKey(configKey, ConfigKeyChangeLabel));

            return toggle;
        }

        private static DataFeedValueField<T> GenerateValueField<T>(IReadOnlyList<string> path, IDefiningConfigKey<T> configKey)
        {
            var valueField = new DataFeedValueField<T>();
            InitBase(valueField, path, configKey);
            valueField.InitSetupValue(field => field.SyncWithConfigKey(configKey, ConfigKeyChangeLabel));

            return valueField;
        }

        private static void InitBase(DataFeedItem item, IReadOnlyList<string> path, IDefiningConfigKey configKey)
            => item.InitBase(configKey.FullId, path, new[] { configKey.Section.Id },
                $"{configKey.FullId}.Name".AsLocaleKey(), $"{configKey.FullId}.Description".AsLocaleKey());

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SettingsDataFeed.PathSegmentName))]
        private static bool PathSegmentNamePrefix(string pathSegment, int depth, ref LocaleString __result)
        {
            __result = depth switch
            {
                1 => $"Settings.Category.{pathSegment}".AsLocaleKey(),
                _ => $"Settings.{pathSegment}.Breadcrumb".AsLocaleKey()
            };

            return false;
        }
    }
}