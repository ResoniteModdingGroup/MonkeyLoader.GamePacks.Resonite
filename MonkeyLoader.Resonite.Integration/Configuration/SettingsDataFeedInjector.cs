using Elements.Core;
using Elements.Quantity;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using MonkeyLoader.Configuration;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using MonkeyLoader.Resonite.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        private const string SaveConfig = "SaveConfig";

        private static readonly MethodInfo _generateEnumField = AccessTools.Method(typeof(SettingsDataFeedInjector), nameof(GenerateEnumField));

        private static readonly MethodInfo _generateQuantityField = AccessTools.Method(typeof(SettingsDataFeedInjector), nameof(GenerateQuantityField));
        private static readonly MethodInfo _generateSlider = AccessTools.Method(typeof(SettingsDataFeedInjector), nameof(GenerateSlider));
        private static readonly MethodInfo _generateValueField = AccessTools.Method(typeof(SettingsDataFeedInjector), nameof(GenerateValueField));

        public override int Priority => HarmonyLib.Priority.Normal;

        protected override bool AppliesTo(FallbackLocaleGenerationEvent eventData) => true;

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => Enumerable.Empty<IFeaturePatch>();

        protected override Task Handle(FallbackLocaleGenerationEvent eventData)
        {
            foreach (var configSection in Mod.Loader.Config.Sections)
            {
                eventData.AddMessage($"{configSection.FullId}.Name", configSection.Name);

                foreach (var configKey in configSection.Keys)
                {
                    eventData.AddMessage(configKey.GetLocaleKey("Name"), configKey.Id);
                    eventData.AddMessage(configKey.GetLocaleKey("Description"), configKey.Description ?? "No Description");
                }
            }

            foreach (var mod in Mod.Loader.Mods)
            {
                var modNameKey = mod.GetLocaleKey("Name");

                eventData.AddMessage(modNameKey, mod.Title);
                eventData.AddMessage($"Settings.{mod.Id}.Breadcrumb", eventData.GetMessage(modNameKey));

                eventData.AddMessage(mod.GetLocaleKey("Description"), mod.Description);

                foreach (var configSection in mod.Config.Sections)
                {
                    eventData.AddMessage(configSection.GetLocaleKey("Name"), configSection.Name);

                    foreach (var configKey in configSection.Keys)
                    {
                        eventData.AddMessage(configKey.GetLocaleKey("Name"), configKey.Id);
                        eventData.AddMessage(configKey.GetLocaleKey("Description"), configKey.Description ?? "No Description");
                    }
                }

                foreach (var monkey in mod.Monkeys)
                {
                    eventData.AddMessage(monkey.GetLocaleKey("Name"), monkey.Name);
                    eventData.AddMessage(monkey.GetLocaleKey("Description"), "No Description");
                }

                foreach (var earlyMonkey in mod.EarlyMonkeys)
                {
                    eventData.AddMessage(earlyMonkey.GetLocaleKey("Name"), earlyMonkey.Name);
                    eventData.AddMessage(earlyMonkey.GetLocaleKey("Description"), "No Description");
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

        private static async IAsyncEnumerable<DataFeedItem> EnumerateConfigAsync(IReadOnlyList<string> path, Config config)
        {
            foreach (var configSection in config.Sections.Where(section => !section.InternalAccessOnly))
            {
                var sectionGroup = new DataFeedGroup();
                sectionGroup.InitBase(configSection.Id, path, null, $"{configSection.FullId}.Name".AsLocaleKey());
                yield return sectionGroup;

                await foreach (var sectionItem in EnumerateConfigSectionAsync(path, configSection))
                    yield return sectionItem;
            }
            var saveConfigButton = new DataFeedCategory();
            saveConfigButton.InitBase(SaveConfig, path, null, Mod.GetLocaleString("Mod.SaveConfig"));
            yield return saveConfigButton;
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
                    if (configKey is IQuantifiedDefiningConfigKey quantifiedKey)
                    {
                        yield return (DataFeedItem)_generateQuantityField
                            .MakeGenericMethod(quantifiedKey.ValueType, quantifiedKey.QuantityType)
                            .Invoke(null, new object[] { path, configKey });

                        continue;
                    }

                    yield return (DataFeedItem)_generateSlider
                        .MakeGenericMethod(configKey.ValueType)
                        .Invoke(null, new object[] { path, configKey });

                    continue;
                }

                yield return (DataFeedItem)_generateValueField.MakeGenericMethod(configKey.ValueType).Invoke(null, new object[] { path, configKey });
            }
        }

        private static async IAsyncEnumerable<DataFeedItem> EnumerateModMonkeysAsync(IReadOnlyList<string> path, Mod mod)
        {
            await foreach (var feedItem in EnumerateMonkeysAsync(path, mod, Monkeys))
                yield return feedItem;

            await foreach (var feedItem in EnumerateMonkeysAsync(path, mod, EarlyMonkeys))
                yield return feedItem;
        }

        private static async IAsyncEnumerable<DataFeedItem> EnumerateModsAsync(IReadOnlyList<string> path)
        {
            var monkeyLoaderCategory = new DataFeedCategory();
            monkeyLoaderCategory.InitBase("MonkeyLoader", path, null, Mod.GetLocaleString("OpenMonkeyLoader.Name"), Mod.GetLocaleString("OpenMonkeyLoader.Description"));
            yield return monkeyLoaderCategory;

            foreach (var mod in Mod.Loader.Mods)
            {
                var modGroup = new DataFeedGroup();
                modGroup.InitBase(mod.Id, path, null, mod.GetLocaleString("Name"), Mod.GetLocaleString("Mod.GroupDescription"));
                yield return modGroup;

                var grouping = new[] { mod.Id };

                var modCategory = new DataFeedCategory();
                modCategory.InitBase($"{mod.Id}.Settings", path, grouping, Mod.GetLocaleString("Mod.OpenSettings"));
                modCategory.SetOverrideSubpath(mod.Id);
                yield return modCategory;

                var description = new DataFeedIndicator<string>();
                description.InitBase($"{mod.Id}.Description", path, grouping, Mod.GetLocaleString("Mod.Description"));
                description.InitSetupValue(field => field.AssignLocaleString(mod.GetLocaleString("Description")));
                yield return description;

                var version = new DataFeedIndicator<string>();
                version.InitBase($"{mod.Id}.Version", path, grouping, Mod.GetLocaleString("Mod.Version"));
                version.InitSetupValue(field => field.Value = mod.Version.ToString());
                yield return version;

                var authors = new DataFeedIndicator<string>();
                authors.InitBase($"{mod.Id}.Authors", path, grouping, Mod.GetLocaleString("Mod.Authors"));
                authors.InitSetupValue(field => field.Value = string.Join(", ", mod.Authors));
                yield return authors;

                var project = new DataFeedIndicator<string>();
                project.InitBase($"{mod.Id}.Project", path, grouping, Mod.GetLocaleString("Mod.Project"));
                project.InitSetupValue(field =>
                {
                    if (mod.ProjectUrl is null)
                    {
                        field.AssignLocaleString(Mod.GetLocaleString("Mod.Project.None"));
                        return;
                    }

                    field.Value = $"<u>{mod.ProjectUrl}</u>";
                    var text = field.FindNearestParent<Text>();

                    text.Slot.AttachComponent<Hyperlink>().URL.Value = mod.ProjectUrl;

                    var drive = text.Slot.AttachComponent<Button>().ColorDrivers.Add();
                    drive.ColorDrive.Target = text.Color;
                });
                yield return project;

                //var configSectionsCategory = new DataFeedCategory();
                //configSectionsCategory.InitBase($"{mod.Id}.{ConfigSections}", path, grouping, Mod.GetLocaleKey($"Mod.Open{ConfigSections}"));
                //configSectionsCategory.SetOverrideSubpath(mod.Id, ConfigSections);
                //yield return configSectionsCategory;

                //var monkeysCategory = new DataFeedCategory();
                //monkeysCategory.InitBase($"{mod.Id}.{MonkeyToggles}", path, grouping, Mod.GetLocaleKey($"Mod.Open{MonkeyToggles}"));
                //monkeysCategory.SetOverrideSubpath(mod.Id, MonkeyToggles);
                //yield return monkeysCategory;
            }
        }

        private static async IAsyncEnumerable<DataFeedItem> EnumerateModSettingsAsync(IReadOnlyList<string> path)
        {
            // path.Count >= 2 because otherwise other methods are called
            // Format: MonkeyLoader / modId / [page]
            if (!Mod.Loader.TryFindModById(path[1], out var mod))
            {
                Logger.Error(() => $"Tried to access non-existant mod's settings: {path[1]}");
                yield break;
            }

            if (path.Count == 2)
            {
                await foreach (var feedItem in EnumerateConfigAsync(path, mod.Config))
                    yield return feedItem;

                await foreach (var feedItem in EnumerateModMonkeysAsync(path, mod))
                    yield return feedItem;

                yield break;
            }

            switch (path[2])
            {
                case ConfigSections:
                    await foreach (var feedItem in EnumerateConfigAsync(path, mod.Config))
                        yield return feedItem;

                    break;

                case MonkeyToggles:
                    await foreach (var feedItem in EnumerateModMonkeysAsync(path, mod))
                        yield return feedItem;

                    break;

                default:
                    Logger.Error(() => $"Tried to access non-existant mod settings page: {path[2]}");
                    break;
            }
        }

        private static async IAsyncEnumerable<DataFeedItem> EnumerateMonkeyLoaderSettingsAsync(IReadOnlyList<string> path)
        {
            await foreach (var feedItem in EnumerateConfigAsync(path, Mod.Loader.Config))
                yield return feedItem;
        }

        private static async IAsyncEnumerable<DataFeedItem> EnumerateMonkeysAsync(IReadOnlyList<string> path, Mod mod, string monkeyType)
        {
            var monkeys = monkeyType switch
            {
                Monkeys => mod.Monkeys.ToArray(),
                EarlyMonkeys => mod.EarlyMonkeys.ToArray(),
                _ => Array.Empty<IMonkey>()
            };

            var group = new DataFeedGroup();
            group.InitBase(monkeyType, path, null, Mod.GetLocaleString($"{monkeyType}.Name"), Mod.GetLocaleString($"{monkeyType}.Description"));
            yield return group;

            var monkeysGrouping = new[] { monkeyType };

            var monkeyCount = new DataFeedIndicator<string>();
            monkeyCount.InitBase($"{monkeyType}.Count", path, monkeysGrouping, Mod.GetLocaleString($"{monkeyType}.Count.Name"), Mod.GetLocaleString($"{monkeyType}.Count.Description"));
            monkeyCount.InitSetupValue(field => field.Value = monkeys.Length.ToString());
            yield return monkeyCount;

            foreach (var monkey in monkeys)
            {
                var monkeyGroup = new DataFeedGroup();
                monkeyGroup.InitBase($"{monkey.Id}", path, monkeysGrouping, monkey.GetLocaleKey("Name").AsLocaleKey());
                yield return monkeyGroup;

                var monkeyGrouping = new[] { monkeyType, monkey.Id };

                if (monkey.CanBeDisabled)
                {
                    var toggle = new DataFeedToggle();
                    toggle.InitBase($"{monkey.Id}.Enabled", path, monkeyGrouping, Mod.GetLocaleString($"{monkeyType}.Enabled.Name"), Mod.GetLocaleString($"{monkeyType}.Enabled.Description"));
                    toggle.InitSetupValue(field => field.SyncWithConfigKey(mod.MonkeyToggles.GetToggle(monkey)));
                    yield return toggle;
                }
                else
                {
                    var enabledIndicator = new DataFeedIndicator<string>();
                    enabledIndicator.InitBase($"{monkey.Id}.Enabled", path, monkeyGrouping, Mod.GetLocaleString($"{monkeyType}.Enabled.Name"), Mod.GetLocaleString($"{monkeyType}.Enabled.Description"));
                    enabledIndicator.InitSetupValue(field => field.Value = "Always Enabled");
                    yield return enabledIndicator;
                }

                var descriptionIndicator = new DataFeedIndicator<string>();
                descriptionIndicator.InitBase($"{monkey.Id}.Description", path, monkeyGrouping, Mod.GetLocaleString("Monkeys.Description.Name"), Mod.GetLocaleString("Monkeys.Description.Description"));
                descriptionIndicator.InitSetupValue(field => field.AssignLocaleString(monkey.GetLocaleKey("Description").AsLocaleKey()));
                yield return descriptionIndicator;

                var typeIndicator = new DataFeedIndicator<string>();
                typeIndicator.InitBase($"{monkey.Id}.Type", path, monkeyGrouping, Mod.GetLocaleString("Monkeys.Type.Name"), Mod.GetLocaleString($"{monkeyType}.Type.Description"));
                typeIndicator.InitSetupValue(field => field.Value = monkey.Type.BaseType.CompactDescription());
                yield return typeIndicator;
            }
        }

        private static void SaveModConfig(string modId)
        {
            Logger.Info(() => $"Saving config for mod: {modId}");
            if (!Mod.Loader.TryFindModById(modId, out var mod))
            {
                Logger.Error(() => $"Tried to save config for non-existant mod: {modId}");
                return;
            }
            mod.Config.Save();
        }

        private static async IAsyncEnumerable<DataFeedItem> YieldBreakAsync()
        {
            yield break;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SettingsDataFeed.Enumerate))]
        private static bool EnumeratePrefix(SettingsDataFeed __instance, IReadOnlyList<string> path, ref IAsyncEnumerable<DataFeedItem> __result)
        {
            if (path.Count == 0 || path[0] != "MonkeyLoader")
                return true;

            if (path.Last() == SaveConfig)
            {
                SaveModConfig(path[1]);

                var rootCategoryView = __instance.Slot.GetComponent<RootCategoryView>();
                if (rootCategoryView.FilterWorldElement() != null)
                {
                    rootCategoryView.RunSynchronously(() =>
                    {
                        if (rootCategoryView.FilterWorldElement() != null && rootCategoryView.Path.Last() == SaveConfig)
                        {
                            rootCategoryView.Path.RemoveAt(path.Count() - 1);
                        }
                    });
                }

                __result = YieldBreakAsync();
                return false;
            }

            __result = path.Count switch
            {
                1 => EnumerateModsAsync(path),
                2 => path[1] == "MonkeyLoader" ? EnumerateMonkeyLoaderSettingsAsync(path) : EnumerateModSettingsAsync(path),
                _ => EnumerateModSettingsAsync(path),
            };

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

        private static DataFeedQuantityField<TQuantity, T> GenerateQuantityField<T, TQuantity>(IReadOnlyList<string> path, IQuantifiedDefiningConfigKey<T, TQuantity> configKey)
            where TQuantity : unmanaged, IQuantity<TQuantity>
        {
            var quantityField = new DataFeedQuantityField<TQuantity, T>();
            InitBase(quantityField, path, configKey);
            quantityField.InitUnitConfiguration(configKey.DefaultConfiguration, configKey.ImperialConfiguration);
            quantityField.InitSetup(quantityField => quantityField.SyncWithConfigKey(configKey, ConfigKeyChangeLabel), configKey.Min, configKey.Max);

            return quantityField;
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