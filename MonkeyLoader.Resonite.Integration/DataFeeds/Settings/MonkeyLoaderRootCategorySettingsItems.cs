using Elements.Assets;
using Elements.Core;
using Elements.Quantity;
using EnumerableToolkit;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.DataFeeds.Settings
{
    [HarmonyPatch(typeof(SettingsDataFeed))]
    [HarmonyPatchCategory(nameof(MonkeyLoaderRootCategorySettingsItems))]
    internal sealed class MonkeyLoaderRootCategorySettingsItems : DataFeedBuildingBlockMonkey<MonkeyLoaderRootCategorySettingsItems, SettingsDataFeed>
    {
        public override int Priority => HarmonyLib.Priority.Low;

        public override IAsyncEnumerable<DataFeedItem> Apply(IAsyncEnumerable<DataFeedItem> current, EnumerateDataFeedParameters<SettingsDataFeed> parameters)
        {
            var dataFeed = parameters.DataFeed;
            var path = parameters.Path;

            parameters.DataFeed.GetViewData();

            if (path.Count != 1 || path[0] is not SettingsHelpers.MonkeyLoader)
                return current;

            parameters.IncludeOriginalResult = false;

            if (!dataFeed.World.IsUserspace())
                return current.Concat(WorldNotUserspaceWarningAsync(path));

            return current.Concat(EnumerateModsAsync(path));
        }

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => [];

        protected override bool OnEngineReady()
        {
            var monkeyLoaderCategory = new SettingCategoryInfo(OfficialAssets.Graphics.Icons.Dash.Tools, 255);
            monkeyLoaderCategory.InitKey("MonkeyLoader");

            FrooxEngine.Settings._categoryInfos.Add(monkeyLoaderCategory.Key, monkeyLoaderCategory);

            return base.OnEngineReady();
        }

        private static async IAsyncEnumerable<DataFeedItem> EnumerateModsAsync(IReadOnlyList<string> path)
        {
            await Task.CompletedTask;

            var modsGroup = new DataFeedGroup();
            modsGroup.InitBase("Mods.Group", path, null!, Mod.GetLocaleString("Mods"));
            yield return modsGroup;

            var modsGrid = new DataFeedGrid();
            modsGrid.InitBase("Mods.Grid", path, ["Mods.Group"], Mod.GetLocaleString("Mods"));
            yield return modsGrid;

            var modsGrouping = new[] { "Mods.Group", "Mods.Grid" };

            foreach (var mod in Mod.Loader.RegularMods.OrderBy(mod => mod.GetMessageInCurrent("Name")))
            {
                var modSubCategory = new DataFeedCategory();
                modSubCategory.InitBase(mod.Id, path, modsGrouping, mod.GetLocaleString("Name"));
                yield return modSubCategory;
            }

            var gamePacksGroup = new DataFeedGroup();
            gamePacksGroup.InitBase("GamePacks.Group", path, null!, Mod.GetLocaleString("GamePacks"));
            yield return gamePacksGroup;

            var gamePacksGrid = new DataFeedGrid();
            gamePacksGrid.InitBase("GamePacks.Grid", path, ["GamePacks.Group"], Mod.GetLocaleString("GamePacks"));
            yield return gamePacksGrid;

            var gamePacksGrouping = new[] { "GamePacks.Group", "GamePacks.Grid" };

            foreach (var gamePack in Mod.Loader.GamePacks.OrderBy(mod => mod.GetMessageInCurrent("Name")))
            {
                var gamePackCategory = new DataFeedCategory();
                gamePackCategory.InitBase(gamePack.Id, path, gamePacksGrouping, gamePack.GetLocaleString("Name"));
                yield return gamePackCategory;
            }

            var monkeyLoaderGroup = new DataFeedGroup();
            monkeyLoaderGroup.InitBase("MonkeyLoader", path, null!, Mod.GetLocaleString("MonkeyLoader.Name"));
            yield return monkeyLoaderGroup;

            var monkeyLoaderGrouping = new[] { Mod.Loader.Id };

            var openMonkeyLoaderSettings = new DataFeedCategory();
            openMonkeyLoaderSettings.InitBase("MonkeyLoader.OpenMonkeyLoader", path, monkeyLoaderGrouping, Mod.GetLocaleString("OpenMonkeyLoader.Name"), Mod.GetLocaleString("OpenMonkeyLoader.Description"));
            openMonkeyLoaderSettings.SetOverrideSubpath("MonkeyLoader");
            yield return openMonkeyLoaderSettings;

            var monkeys = Mod.Loader.Mods.SelectMany(mod => mod.Monkeys);
            var monkeyCountIndicator = new DataFeedIndicator<string>();
            monkeyCountIndicator.InitBase("MonkeyLoader.MonkeyCount", path, monkeyLoaderGrouping, Mod.GetLocaleString("MonkeyLoader.MonkeyCount.Name"));
            monkeyCountIndicator.InitSetupValue(field => field.SetLocalized(Mod.GetLocaleString("MonkeyLoader.MonkeyCount.Value", ("available", monkeys.Count()), ("active", monkeys.Count(monkey => monkey.Enabled)))));
            yield return monkeyCountIndicator;

            var earlyMonkeys = Mod.Loader.Mods.SelectMany(mod => mod.EarlyMonkeys);
            var earlyMonkeyCountIndicator = new DataFeedIndicator<string>();
            earlyMonkeyCountIndicator.InitBase("MonkeyLoader.EarlyMonkeyCount", path, monkeyLoaderGrouping, Mod.GetLocaleString("MonkeyLoader.EarlyMonkeyCount.Name"));
            earlyMonkeyCountIndicator.InitSetupValue(field => field.SetLocalized(Mod.GetLocaleString("MonkeyLoader.EarlyMonkeyCount.Value", ("available", earlyMonkeys.Count()), ("active", earlyMonkeys.Count(monkey => monkey.Enabled)))));
            yield return earlyMonkeyCountIndicator;
        }

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

        private static IAsyncEnumerable<DataFeedItem> WorldNotUserspaceWarningAsync(IReadOnlyList<string> path)
        {
            var warning = new DataFeedIndicator<string>();
            warning.InitBase("Information", path, null!, Mod.GetLocaleString("Information"));
            warning.InitSetupValue(field => field.AssignLocaleString(Mod.GetLocaleString("WorldNotUserspace")));
            return ((DataFeedItem)warning).YieldAsync();
        }
    }
}