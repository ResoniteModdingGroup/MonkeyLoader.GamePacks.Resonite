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
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.Configuration
{
    [HarmonyPatch(typeof(SettingsDataFeed))]
    [HarmonyPatchCategory(nameof(SettingsDataFeedInjector))]
    internal sealed class SettingsDataFeedInjector : ResoniteMonkey<SettingsDataFeedInjector>
    {
        public const string ConfigSections = "ConfigSections";

        public const string MonkeyToggles = "MonkeyToggles";

        private static MethodInfo _generateValueField = AccessTools.Method(typeof(SettingsDataFeedInjector), nameof(GenerateValueField));

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => Enumerable.Empty<IFeaturePatch>();

        protected override bool OnEngineReady()
        {
            AddLocaleGeneratorMappers();

            var monkeyLoaderCategory = new SettingCategoryInfo(OfficialAssets.Graphics.Settings.Misc, 255);
            monkeyLoaderCategory.InitKey("MonkeyLoader");

            Settings._categoryInfos.Add(monkeyLoaderCategory.Key, monkeyLoaderCategory);

            return base.OnEngineReady();
        }

        private static void AddLocaleGeneratorMappers()
        {
            FallbackLocaleGenerator.AddMapper(mod => $"{mod.Id}.Name", mod => mod.Title);
            FallbackLocaleGenerator.AddMapper(mod => $"Settings.{mod.Id}.MonkeyToggles.Breadcrumb", mod => $"{mod.Title} Monkeys");
            FallbackLocaleGenerator.AddMapper(mod => $"Settings.{mod.Id}.ConfigSections.Breadcrumb", mod => $"{mod.Title} Settings");

            //foreach (var mod in Mod.Loader.Mods) {
            //FallbackLocaleGenerator.AddMapper(mod)
        }

        private static async IAsyncEnumerable<DataFeedItem> EnumerateConfigSectionAsync(IReadOnlyList<string> path, ConfigSection configSection)
        {
            foreach (var configKey in configSection.Keys)
            {
                if (Coder.IsEnginePrimitive(configKey.ValueType))
                {
                    yield return (DataFeedItem)_generateValueField.MakeGenericMethod(configKey.ValueType).Invoke(null, new object[] { path, configKey });
                }
            }
        }

        private static async IAsyncEnumerable<DataFeedItem> EnumerateModsAsync(IReadOnlyList<string> path)
        {
            foreach (var mod in Mod.Loader.Mods)
            {
                var modGroup = new DataFeedGroup();
                modGroup.InitBase(mod.Id, path, null, $"{mod.Id}.Name".AsLocaleKey());
                yield return modGroup;

                var grouping = new[] { mod.Id };

                var monkeysCategory = new DataFeedCategory();
                monkeysCategory.InitBase($"{mod.Id}.{MonkeyToggles}", path, grouping, $"MonkeyLoader.GamePacks.Resonite.Open{MonkeyToggles}".AsLocaleKey());
                monkeysCategory.SetOverrideSubpath(mod.Id, MonkeyToggles);
                yield return monkeysCategory;

                var configSectionsCategory = new DataFeedCategory();
                configSectionsCategory.InitBase($"{mod.Id}.{ConfigSections}", path, grouping, $"MonkeyLoader.GamePacks.Resonite.Open{ConfigSections}".AsLocaleKey());
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

                    break;

                case ConfigSections:
                    foreach (var configSection in mod.Config.Sections)
                    {
                        var sectionGroup = new DataFeedGroup();
                        sectionGroup.InitBase(configSection.Id, path, null, $"{mod.Id}.{configSection.Id}.Name".AsLocaleKey());
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

        private static DataFeedValueField<T> GenerateValueField<T>(IReadOnlyList<string> path, IDefiningConfigKey<T> configKey)
        {
            var valueField = new DataFeedValueField<T>();
            valueField.InitBase(configKey.FullId, path, new[] { configKey.Section.Id }, $"{configKey.FullId}.Name".AsLocaleKey(), $"{configKey.FullId}.Description");
            valueField.InitSetupValue(field => ((Sync<T>)field).OnValueChange += syncField => configKey.SetValue(syncField.Value));

            return valueField;
        }
    }
}