﻿using Elements.Assets;
using Elements.Core;
using Elements.Quantity;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using MonkeyLoader.Components;
using MonkeyLoader.Configuration;
using MonkeyLoader.Logging;
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
    internal sealed class ModSettingStandaloneFacet : ResoniteMonkey<ModSettingStandaloneFacet>
    {
        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => Enumerable.Empty<IFeaturePatch>();

        private static readonly MethodInfo _syncWithConfigKeyWrapperMethod = AccessTools.Method(typeof(ModSettingStandaloneFacet), nameof(SyncWithConfigKeyWrapper));
        private const string ModSettingStandaloneFacetTag = "MonkeyLoaderStandaloneFacet";
        public const string ConfigKeyChangeLabel = "StandaloneFacet";

        private static void SyncWithConfigKeyWrapper<T>(IField field, IDefiningConfigKey key, string? eventLabel)
        {
            ((IField<T>)field).SyncWithConfigKey((IDefiningConfigKey<T>)key, eventLabel);
        }

        private static IDefiningConfigKey? GetConfigKeyByFullId(string fullId)
        {
            if (fullId.StartsWith(Mod.Loader.Id))
            {
                var partialId = fullId.Remove(0, Mod.Loader.Id.Length + 1);
                Logger.Debug(() => "Partial Id: " + partialId);
                var loaderSection = Mod.Loader.Config.Sections.FirstOrDefault(section => partialId.StartsWith(section.Id));
                if (loaderSection != null)
                {
                    var keyId = partialId.Remove(0, loaderSection.Id.Length + 1);
                    if (loaderSection.TryGet<IDefiningConfigKey>().ById(keyId, out var loaderConfigKey))
                    {
                        return loaderConfigKey;
                    }
                }
            }
            else
            {
                foreach (var mod in Mod.Loader.Mods)
                {
                    if (fullId.StartsWith(mod.Id))
                    {
                        var partialId = fullId.Remove(0, mod.Id.Length + 1);
                        Logger.Debug(() => "Partial Id: " + partialId);
                        if (mod.TryGet<IDefiningConfigKey>().ByPartialId(partialId, out var modConfigKey))
                        {
                            return modConfigKey;
                        }
                        break;
                    }
                }
            }

            // Worst case scenario check everything
            //if (Mod.Loader.TryGet<IDefiningConfigKey>().ByFullId(fullId, out var key))
            //{
            //    return key;
            //}

            return null;
        }

        [HarmonyPatch(typeof(UIGrabInstancer), nameof(UIGrabInstancer.TryGrab))]
        [HarmonyPatchCategory(nameof(ModSettingStandaloneFacet))]
        class UIGrabInstancerPatch
        {
            [HarmonyPostfix]
            private static void TryGrabPostfix(UIGrabInstancer __instance, IGrabbable __result)
            {
                if (__result == null || __result is not Grabbable) return;
                if (!__instance.World.IsUserspace()) return;
                if (__result.Slot.GetComponent<Facet>() == null) return;
                if (__instance.Slot.GetComponentInParents<FeedItemInterface>() == null) return;
                if (__instance.Slot.GetComponentInParents<SettingsDataFeed>() == null) return;
                var feedItemInterface = __result.Slot.GetComponentInChildren<FeedItemInterface>();
                var comment = feedItemInterface?.Slot.GetComponent<Comment>();
                if (feedItemInterface != null && comment?.Text != null)
                {
                    // Do these checks to make sure it's not a vanilla settings facet
                    // This might not actually be needed, since vanilla facets probably don't have the comment component
                    if (feedItemInterface.Slot.GetComponentInChildren<FrooxEngine.Component>(component => component.GetType().IsGenericType && component.GetType().GetGenericTypeDefinition() == typeof(SettingValueSync<,>)) != null) return;

                    Logger.Info(() => "Instantiated mod setting standalone facet!");

                    Logger.Debug(() => "ItemName: " + feedItemInterface.ItemName.Target?.Value ?? "NULL");
                    Logger.Debug(() => "Config Key FullId: " + comment.Text);

                    var foundKey = GetConfigKeyByFullId(comment.Text);
                    if (foundKey != null)
                    {
                        Logger.Info(() => $"Got config key! OwnerID: {foundKey.Config.Owner.Id} SectionID: {foundKey.Section.Id} KeyID: {foundKey.Id}");
                        __result.Slot.Tag = ModSettingStandaloneFacetTag;
                        if (foundKey.Section is MonkeyTogglesConfigSection)
                        {
                            var field = feedItemInterface.ItemName.Target;
                            if (field != null)
                            {
                                if (field.IsDriven)
                                {
                                    if (field.ActiveLink.Parent is LocaleStringDriver localeStringDriver)
                                    {
                                        localeStringDriver.Key.Value = foundKey.GetLocaleKey("Name");
                                    }
                                }
                                else
                                {
                                    // Drive the field with the localized value
                                    field.DriveLocalized(foundKey.GetLocaleKey("Name"));
                                }
                            }
                        }
                        if (feedItemInterface.GetSyncMember("Value") is ISyncRef valueField && valueField.Target != null)
                        {
                            var field = (IField)valueField.Target;
                            var genericMethod = _syncWithConfigKeyWrapperMethod.MakeGenericMethod(new Type[] { field.ValueType });
                            genericMethod.Invoke(null, new object[] { field, foundKey, ConfigKeyChangeLabel });
                            feedItemInterface.Slot.PersistentSelf = true;
                            return;
                        }
                    }

                    Logger.Error(() => $"Could not sync a config key with a standalone facet! Comment text: {comment.Text}");
                }
            }
        }

        [HarmonyPatch(typeof(Facet), nameof(Facet.OnLoading))]
        [HarmonyPatchCategory(nameof(ModSettingStandaloneFacet))]
        class FacetPatch
        {
            [HarmonyPostfix]
            private static void OnLoadingPostfix(Facet __instance)
            {
                // Not sure if this is needed
                //if (!Engine.Current.IsReady) return;

                if (!__instance.World.IsUserspace()) return;

                __instance.RunSynchronously(() =>
                {
                    if (__instance.FilterWorldElement() == null) return;

                    if (__instance.Slot.Tag != ModSettingStandaloneFacetTag) return;

                    var feedItemInterface = __instance.Slot.GetComponentInChildren<FeedItemInterface>();
                    var comment = feedItemInterface?.Slot.GetComponent<Comment>();

                    if (comment?.Text != null && feedItemInterface != null)
                    {
                        Logger.Info(() => "Loaded a mod setting standalone facet!");
                        Logger.Debug(() => "Config Key FullId: " + comment.Text);

                        var foundKey = GetConfigKeyByFullId(comment.Text);
                        if (foundKey != null)
                        {
                            Logger.Info(() => $"Got config key! OwnerID: {foundKey.Config.Owner.Id} SectionID: {foundKey.Section.Id} KeyID: {foundKey.Id}");
                            if (feedItemInterface.GetSyncMember("Value") is ISyncRef valueField && valueField.Target != null)
                            {
                                var field = (IField)valueField.Target;
                                var genericMethod = _syncWithConfigKeyWrapperMethod.MakeGenericMethod(new Type[] { field.ValueType });
                                genericMethod.Invoke(null, new object[] { field, foundKey, ConfigKeyChangeLabel });
                                return;
                            }
                        }

                        Logger.Error(() => $"Could not sync a config key with a standalone facet! Comment text: {comment.Text}");
                    }
                });
            }
        }
    }

    [HarmonyPatch(typeof(SettingsDataFeed))]
    [HarmonyPatchCategory(nameof(SettingsDataFeedInjector))]
    internal sealed class SettingsDataFeedInjector : ResoniteAsyncEventHandlerMonkey<SettingsDataFeedInjector, FallbackLocaleGenerationEvent>
    {
        public const string ConfigKeyChangeLabel = "Settings";
        public const string ConfigSections = "ConfigSections";

        public const string MonkeyToggles = "MonkeyToggles";

        private const string EarlyMonkeys = "EarlyMonkeys";
        private const string Monkeys = "Monkeys";

        private const string ResetConfig = "ResetConfig";
        private const string SaveConfig = "SaveConfig";

        private static readonly MethodInfo _generateEnumField = AccessTools.Method(typeof(SettingsDataFeedInjector), nameof(GenerateEnumField));
        private static readonly MethodInfo _generateItemForConfigKeyMethod = AccessTools.Method(typeof(SettingsDataFeedInjector), nameof(GenerateItemForConfigKey));
        private static readonly MethodInfo _generateQuantityField = AccessTools.Method(typeof(SettingsDataFeedInjector), nameof(GenerateQuantityField));

        private static readonly Stack<float> _scrollAmounts = new();
        private static RootCategoryView? _cachedRootCategoryView = null;
        private static Slider<float>? _cachedScrollSlider = null;

        private static bool _legacyColorXTemplateCleanupDone = false;
        private const string LegacyInjectedColorXTemplateName = "Injected DataFeedValueField<colorX>";
        private static DataFeedItemMapper? _cachedDataFeedItemMapper = null;

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

                foreach (var monkey in mod.Monkeys)
                {
                    var monkeyNameKey = monkey.GetLocaleKey("Name");
                    
                    eventData.AddMessage(monkeyNameKey, monkey.Name);
                    eventData.AddMessage(monkey.GetLocaleKey("Description"), "No Description");

                    if (monkey.CanBeDisabled)
                        eventData.AddMessage(mod.MonkeyToggles.GetToggle(monkey).GetLocaleKey("Name"), $"{eventData.GetMessage(monkeyNameKey)} Enabled");
                }

                foreach (var earlyMonkey in mod.EarlyMonkeys)
                {
                    var earlyMonkeyNameKey = earlyMonkey.GetLocaleKey("Name");
                    
                    eventData.AddMessage(earlyMonkeyNameKey, earlyMonkey.Name);
                    eventData.AddMessage(earlyMonkey.GetLocaleKey("Description"), "No Description");

                    if (earlyMonkey.CanBeDisabled)
                        eventData.AddMessage(mod.MonkeyToggles.GetToggle(earlyMonkey).GetLocaleKey("Name"), $"{eventData.GetMessage(earlyMonkeyNameKey)} Enabled");
                }

                foreach (var configSection in mod.Config.Sections)
                {
                    eventData.AddMessage(configSection.GetLocaleKey("Name"), configSection.Name);

                    foreach (var configKey in configSection.Keys)
                    {
                        eventData.AddMessage(configKey.GetLocaleKey("Name"), configKey.Id);
                        eventData.AddMessage(configKey.GetLocaleKey("Description"), configKey.Description ?? "No Description");
                    }
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

        private static void EnsureDataFeedValueFieldTemplate(DataFeedItemMapper mapper, Type typeToInject)
        {
            // Cleanup previously injected colorX templates that were accidentally made persistent and may have been saved with the dash
            if (typeToInject == typeof(colorX) && !_legacyColorXTemplateCleanupDone)
            {
                Logger.Info(() => "Looking for previously injected colorX templates.");
                foreach (var mapping in mapper.Mappings.Where(mapping => mapping.MatchingType == typeof(DataFeedValueField<colorX>) && mapping.Template.Target?.Slot.Name == LegacyInjectedColorXTemplateName).ToArray())
                {
                    mapping.Template.Target.Slot.Destroy();
                    mapper.Mappings.Remove(mapping);
                    Logger.Info(() => "Cleaned up a previously injected colorX template.");
                }
                _legacyColorXTemplateCleanupDone = true;
            }

            Type dataFeedValueFieldType = typeof(DataFeedValueField<>).MakeGenericType(typeToInject);
            if (!mapper.Mappings.Any(mapping => mapping.MatchingType == dataFeedValueFieldType && mapping.Template.Target != null))
            {
                var templatesRoot = mapper.Slot.Parent?.FindChild("Templates");
                if (templatesRoot != null)
                {
                    bool changeIndex = false;
                    DataFeedItemMapper.ItemMapping mapping = mapper.Mappings.FirstOrDefault(mapping => mapping.MatchingType == dataFeedValueFieldType && mapping.Template.Target == null);
                    if (mapping == null)
                    {
                        mapping = mapper.Mappings.Add();
                        mapping.MatchingType.Value = dataFeedValueFieldType;
                        changeIndex = true;
                    }

                    var template = templatesRoot.AddSlot($"Injected DataFeedValueField<{typeToInject.Name}>");
                    template.ActiveSelf = false;
                    template.PersistentSelf = false;
                    template.AttachComponent<LayoutElement>().MinHeight.Value = 96f;
                    var ui = new UIBuilder(template);
                    RadiantUI_Constants.SetupEditorStyle(ui);
                    ui.ForceNext = template.AttachComponent<RectTransform>();
                    ui.HorizontalLayout(11.78908f, 11.78908f);
                    var text = ui.Text("Label");
                    text.Size.Value = 24f;
                    text.HorizontalAlign.Value = TextHorizontalAlignment.Left;
                    ui.Style.MinHeight = 32f;

                    ui.Spacer(128f);

                    FrooxEngine.Component component = null;
                    ISyncMember member = null;
                    FieldInfo fieldInfo = null;
                    if (typeToInject == typeof(Type))
                    {
                        component = template.AttachComponent(typeof(TypeField));
                        member = component.GetSyncMember("Type");
                        if (member == null)
                        {
                            Logger.Error(() => "Could not get Type sync member from attached TypeField component!");
                            return;
                        }
                        fieldInfo = component.GetSyncMemberFieldInfo("Type");
                    }
                    else
                    {
                        component = template.AttachComponent(typeof(ValueField<>).MakeGenericType(typeToInject));
                        member = component.GetSyncMember("Value");
                        if (member == null)
                        {
                            Logger.Error(() => $"Could not get Value sync member from attached ValueField<{typeToInject.Name}> component!");
                            return;
                        }
                        fieldInfo = component.GetSyncMemberFieldInfo("Value");
                    }

                    ui.Style.FlexibleWidth = 1f;
                    SyncMemberEditorBuilder.Build(member, null, fieldInfo, ui, 0f);
                    ui.Style.FlexibleWidth = -1f;

                    var memberActions = ui.Root?.GetComponentInChildren<InspectorMemberActions>()?.Slot;
                    if (memberActions != null)
                    {
                        memberActions.ActiveSelf = false;
                    }

                    var feedValueFieldInterface = template.AttachComponent(typeof(FeedValueFieldInterface<>).MakeGenericType(typeToInject));

                    ((FeedItemInterface)feedValueFieldInterface).ItemName.Target = text.Content;

                    if (feedValueFieldInterface.GetSyncMember("Value") is ISyncRef valueField)
                    {
                        valueField.Target = member;
                    }
                    else
                    {
                        Logger.Error(() => "Could not get Value sync member from attached FeedValueFieldInterface component!");
                    }

                    var innerInterfaceSlot = templatesRoot.FindChild("InnerContainerItem");
                    if (innerInterfaceSlot != null)
                    {
                        var innerInterface = innerInterfaceSlot.GetComponent<FeedItemInterface>();

                        ((FeedItemInterface)feedValueFieldInterface).ParentContainer.Target = innerInterface;
                    }
                    else
                    {
                        Logger.Error(() => "InnerContainerItem slot is null in EnsureDataFeedValueFieldTemplate!");
                    }

                    mapping.Template.Target = (FeedItemInterface)feedValueFieldInterface;

                    if (changeIndex)
                    {
                        // Move the new mapping above the previous last element (default DataFeedItem mapping) in the list
                        mapper.Mappings.MoveToIndex(mapper.Mappings.Count() - 1, mapper.Mappings.Count() - 2);
                    }

                    Logger.Info(() => $"Injected DataFeedValueField<{typeToInject.Name}> template");
                }
                else
                {
                    Logger.Error(() => "Could not find Templates slot in EnsureDataFeedValueFieldTemplate!");
                }
            }
            else
            {
                // This could cause some log spam
                //Logger.Trace(() => $"Existing DataFeedValueField<{typeToInject.Name}> template found.");
            }
        }

        private static async IAsyncEnumerable<DataFeedItem> EnumerateConfigAsync(IReadOnlyList<string> path, Config config)
        {
            var generateConfigButtons = false;

            foreach (var configSection in config.Sections.Where(section => !section.InternalAccessOnly))
            {
                var sectionGroup = new DataFeedGroup();
                sectionGroup.InitBase(configSection.Id, path, null, $"{configSection.FullId}.Name".AsLocaleKey());
                yield return sectionGroup;

                await foreach (var sectionItem in EnumerateConfigSectionAsync(path, configSection))
                {
                    generateConfigButtons = true;
                    yield return sectionItem;
                }
            }

            if (generateConfigButtons)
            {
                var saveConfigButton = new DataFeedCategory();
                saveConfigButton.InitBase(SaveConfig, path, null, Mod.GetLocaleString("SaveConfig"));
                yield return saveConfigButton;

                var resetConfigButton = new DataFeedCategory();
                resetConfigButton.InitBase(ResetConfig, path, null, Mod.GetLocaleString("ResetConfig"));
                yield return resetConfigButton;
            }
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

                yield return (DataFeedItem)_generateItemForConfigKeyMethod
                    .MakeGenericMethod(configKey.ValueType)
                    .Invoke(null, new object[] { path, configKey });
            }
        }

        private static async IAsyncEnumerable<DataFeedItem> EnumerateModMetadataAsync(IReadOnlyList<string> path, Mod mod)
        {
            var modGroup = new DataFeedGroup();
            modGroup.InitBase("Metadata", path, null, Mod.GetLocaleString("Mod.Metadata"));
            yield return modGroup;

            var grouping = new[] { "Metadata" };

            var id = new DataFeedIndicator<string>();
            id.InitBase("Id", path, grouping, Mod.GetLocaleString("Mod.Id"));
            id.InitSetupValue(field => field.Value = mod.Id);
            yield return id;

            var version = new DataFeedIndicator<string>();
            version.InitBase("Version", path, grouping, Mod.GetLocaleString("Mod.Version"));
            version.InitSetupValue(field => field.Value = mod.Version.ToString());
            yield return version;

            var authors = new DataFeedIndicator<string>();
            authors.InitBase("Authors", path, grouping, Mod.GetLocaleString("Mod.Authors", ("count", mod.Authors.Count())));
            authors.InitSetupValue(field => field.Value = mod.Authors.Join());
            yield return authors;

            var project = new DataFeedIndicator<string>();
            project.InitBase("Project", path, grouping, Mod.GetLocaleString("Mod.Project"));
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

            var description = new DataFeedIndicator<string>();
            description.InitBase("Description", path, grouping, Mod.GetLocaleString("Mod.Description"));
            description.InitSetupValue(field => field.AssignLocaleString(mod.GetLocaleString("Description")));
            yield return description;
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
            var modsGroup = new DataFeedGroup();
            modsGroup.InitBase("Mods.Group", path, null, Mod.GetLocaleString("Mods"));
            yield return modsGroup;

            var modsGrid = new DataFeedGrid();
            modsGrid.InitBase("Mods.Grid", path, new[] { "Mods.Group" }, Mod.GetLocaleString("Mods"));
            yield return modsGrid;

            var modsGrouping = new[] { "Mods.Group", "Mods.Grid" };

            foreach (var mod in Mod.Loader.RegularMods.OrderBy(GetLocalizedModName))
            {
                var modSubCategory = new DataFeedCategory();
                modSubCategory.InitBase(mod.Id, path, modsGrouping, mod.GetLocaleString("Name"));
                yield return modSubCategory;
            }

            var gamePacksGroup = new DataFeedGroup();
            gamePacksGroup.InitBase("GamePacks.Group", path, null, Mod.GetLocaleString("GamePacks"));
            yield return gamePacksGroup;

            var gamePacksGrid = new DataFeedGrid();
            gamePacksGrid.InitBase("GamePacks.Grid", path, new[] { "GamePacks.Group" }, Mod.GetLocaleString("GamePacks"));
            yield return gamePacksGrid;

            var gamePacksGrouping = new[] { "GamePacks.Group", "GamePacks.Grid" };

            foreach (var gamePack in Mod.Loader.GamePacks.OrderBy(GetLocalizedModName))
            {
                var gamePackCategory = new DataFeedCategory();
                gamePackCategory.InitBase(gamePack.Id, path, gamePacksGrouping, gamePack.GetLocaleString("Name"));
                yield return gamePackCategory;
            }

            var monkeyLoaderGroup = new DataFeedGroup();
            monkeyLoaderGroup.InitBase("MonkeyLoader", path, null, Mod.GetLocaleString("MonkeyLoader.Name"));
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

                await foreach (var feedItem in EnumerateModMetadataAsync(path, mod))
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
                    toggle.InitSetupValue(field => SetupConfigKeyField(field, mod.MonkeyToggles.GetToggle(monkey)));
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

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SettingsDataFeed.Enumerate))]
        private static bool EnumeratePrefix(SettingsDataFeed __instance, IReadOnlyList<string> path, ref IAsyncEnumerable<DataFeedItem> __result)
        {
            if (__instance.World.IsUserspace() && _cachedRootCategoryView.FilterWorldElement() == null)
            {
                _cachedRootCategoryView = __instance.Slot.GetComponent<RootCategoryView>();
                if (_cachedRootCategoryView != null)
                {
                    _cachedRootCategoryView.Path.ElementsAdded += OnElementsAdded;
                    _cachedRootCategoryView.Path.ElementsRemoved += OnElementsRemoved;
                    Logger.Debug(() => "Cached RootCategoryView and subscribed to events.");
                }
            }

            if (__instance.World.IsUserspace() && _cachedScrollSlider.FilterWorldElement() == null)
            {
                Slot settingsListSlot = __instance.Slot.FindChild(s => s.Name == "Settings List", maxDepth: 2);
                if (settingsListSlot != null)
                {
                    Slot scrollBarSlot = settingsListSlot.FindChild(s => s.Name == "Scroll Bar", maxDepth: 2);
                    if (scrollBarSlot != null)
                    {
                        var slider = scrollBarSlot.GetComponentInChildren<Slider<float>>();
                        if (slider != null)
                        {
                            _cachedScrollSlider = slider;
                            Logger.Debug(() => "Cached settings scroll slider.");
                        }
                    }
                }
            }

            if (path.Count == 0 || path[0] != "MonkeyLoader")
                return true;

            if (!__instance.World.IsUserspace())
            {
                __result = WorldNotUserspaceWarningAsync(path);
                return false;
            }

            switch (path.Last())
            {
                case SaveConfig:
                    SaveModOrLoaderConfig(path[1]);

                    _cachedRootCategoryView?.RunSynchronously(() => MoveUpFromCategory(_cachedRootCategoryView, SaveConfig));

                    __result = YieldBreakAsync();
                    return false;

                case ResetConfig:
                    ResetModOrLoaderConfig(path[1]);

                    _cachedRootCategoryView?.RunSynchronously(() => MoveUpFromCategory(_cachedRootCategoryView, ResetConfig));

                    __result = YieldBreakAsync();
                    return false;

                default:
                    break;
            }

            _cachedDataFeedItemMapper = __instance.Slot.GetComponent((DataFeedItemMapper m) => m.Mappings.Count > 1);

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
            enumField.InitSetupValue(field => SetupConfigKeyField(field, configKey));

            return enumField;
        }

        private static DataFeedIndicator<T> GenerateIndicator<T>(IReadOnlyList<string> path, IDefiningConfigKey<T> configKey)
        {
            var indicator = new DataFeedIndicator<T>();
            InitBase(indicator, path, configKey);
            indicator.InitSetupValue(field => SetupConfigKeyField(field, configKey));

            return indicator;
        }

        private static DataFeedItem GenerateItemForConfigKey<T>(IReadOnlyList<string> path, IEntity<IDefiningConfigKey<T>> configKey)
        {
            if (configKey.Components.TryGet<IConfigKeyRange<T>>(out var range))
            {
                if (configKey.Components.TryGet<IConfigKeyQuantity<T>>(out var quantity))
                {
                    return (DataFeedItem)_generateQuantityField
                        .MakeGenericMethod(configKey.Self.ValueType, quantity.QuantityType)
                        .Invoke(null, new object[] { path, configKey.Self, quantity });
                }

                return GenerateSlider(path, configKey.Self, range);
            }

            return GenerateValueField(path, configKey.Self);
        }

        private static DataFeedQuantityField<TQuantity, T> GenerateQuantityField<T, TQuantity>(IReadOnlyList<string> path, IDefiningConfigKey<T> configKey, IConfigKeyQuantity<T> quantity)
            where TQuantity : unmanaged, IQuantity<TQuantity>
        {
            var quantityField = new DataFeedQuantityField<TQuantity, T>();
            InitBase(quantityField, path, configKey);
            quantityField.InitUnitConfiguration(quantity.DefaultConfiguration, quantity.ImperialConfiguration);
            quantityField.InitSetup(quantityField => SetupConfigKeyField(quantityField, configKey), quantity.Min, quantity.Max);

            return quantityField;
        }

        private static DataFeedSlider<T> GenerateSlider<T>(IReadOnlyList<string> path, IDefiningConfigKey<T> configKey, IConfigKeyRange<T> range)
        {
            var slider = new DataFeedSlider<T>();
            InitBase(slider, path, configKey);
            slider.InitSetup(field => SetupConfigKeyField(field, configKey), range.Min, range.Max);

            //if (!string.IsNullOrWhiteSpace(configKey.TextFormat))
            //    slider.InitFormatting(configKey.TextFormat);

            return slider;
        }

        private static DataFeedToggle GenerateToggle(IReadOnlyList<string> path, IDefiningConfigKey<bool> configKey)
        {
            var toggle = new DataFeedToggle();
            InitBase(toggle, path, configKey);
            toggle.InitSetupValue(field => SetupConfigKeyField(field, configKey));

            return toggle;
        }

        private static DataFeedValueField<T> GenerateValueField<T>(IReadOnlyList<string> path, IDefiningConfigKey<T> configKey)
        {
            var valueField = new DataFeedValueField<T>();
            InitBase(valueField, path, configKey);
            valueField.InitSetupValue(field => SetupConfigKeyField(field, configKey));
            

            var valueType = typeof(T);
            if (valueType != typeof(dummy) && (Coder<T>.IsEnginePrimitive || valueType == typeof(Type)))
            {
                _cachedDataFeedItemMapper?.RunSynchronously(() => EnsureDataFeedValueFieldTemplate(_cachedDataFeedItemMapper, valueType));
            }

            return valueField;
        }

        private static void SetupConfigKeyField<T>(IField<T> field, IDefiningConfigKey<T> configKey)
        {
            var slot = field.FindNearestParent<Slot>();
            if (slot.GetComponentInParents<FeedItemInterface>() is FeedItemInterface feedItemInterface)
            {
                // Adding the config key's full id to make it easier to create standalone facets
                feedItemInterface.Slot.AttachComponent<Comment>().Text.Value = configKey.FullId;
            }
            field.SyncWithConfigKey(configKey, ConfigKeyChangeLabel);
        }

        private static string GetLocalizedModName(Mod mod)
            => mod.GetLocaleString("Name").Format()!;

        private static void InitBase(DataFeedItem item, IReadOnlyList<string> path, IDefiningConfigKey configKey)
            => item.InitBase(configKey.FullId, path, new[] { configKey.Section.Id },
                $"{configKey.FullId}.Name".AsLocaleKey(), $"{configKey.FullId}.Description".AsLocaleKey());

        private static void MoveUpFromCategory(RootCategoryView rootCategoryView, string category)
        {
            if (rootCategoryView.FilterWorldElement() != null && rootCategoryView.Path.Last() == category)
            {
                Logger.Debug(() => $"Moving up from category: {category}");
                rootCategoryView.MoveUpInCategory();
            }
        }

        private static void OnElementsAdded(SyncElementList<Sync<string>> list, int start, int count)
        {
            Logger.Trace(() => $"OnElementsAdded. start: {start} count: {count}");

            if (_cachedScrollSlider.FilterWorldElement() != null)
            {
                _scrollAmounts.Push(_cachedScrollSlider!.Value.Value);
                Logger.Trace(() => $"Pushed value {_cachedScrollSlider!.Value.Value}. _scrollAmounts count: {_scrollAmounts.Count}");
            }
        }

        private static void OnElementsRemoved(SyncElementList<Sync<string>> list, int start, int count)
        {
            Logger.Trace(() => $"OnElementsRemoved. start: {start} count: {count}");

            if (start == 0)
            {
                _scrollAmounts.Clear();
                Logger.Trace(() => $"Cleared _scrollAmounts.");
                return;
            }

            var poppedValue = 0f;

            for (var i = 0; i < count; i++)
            {
                if (_scrollAmounts.Count > 0)
                {
                    poppedValue = _scrollAmounts.Pop();
                    Logger.Trace(() => $"Popped value {poppedValue}. _scrollAmounts count: {_scrollAmounts.Count}");
                }
            }

            if (_cachedScrollSlider.FilterWorldElement() != null)
            {
                _cachedScrollSlider!.RunInUpdates(3, () =>
                {
                    if (_cachedScrollSlider.FilterWorldElement() != null)
                    {
                        _cachedScrollSlider.Value.Value = poppedValue;
                        Logger.Debug(() => $"Set scroll slider to value {poppedValue}");
                    }
                });
            }
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

        private static void ResetModOrLoaderConfig(string modOrLoaderId)
        {
            if (modOrLoaderId == Mod.Loader.Id)
            {
                Logger.Info(() => $"Resetting config to default for loader: {modOrLoaderId}");
                foreach (var key in Mod.Loader.Config.ConfigurationItemDefinitions)
                {
                    key.TryComputeDefault(out var defaultValue);
                    key.SetValue(defaultValue, "Default");
                }
            }
            else
            {
                if (!Mod.Loader.TryFindModById(modOrLoaderId, out var mod))
                {
                    Logger.Error(() => $"Tried to reset config to default for non-existent mod: {modOrLoaderId}");
                    return;
                }
                Logger.Info(() => $"Resetting config to default for mod: {modOrLoaderId}");
                foreach (var key in mod.Config.ConfigurationItemDefinitions)
                {
                    key.TryComputeDefault(out var defaultValue);
                    key.SetValue(defaultValue, "Default");
                }
            }
        }

        private static void SaveModOrLoaderConfig(string modOrLoaderId)
        {
            if (modOrLoaderId == Mod.Loader.Id)
            {
                Logger.Info(() => $"Saving config for loader: {modOrLoaderId}");
                Mod.Loader.Config.Save();
            }
            else
            {
                if (!Mod.Loader.TryFindModById(modOrLoaderId, out var mod))
                {
                    Logger.Error(() => $"Tried to save config for non-existent mod: {modOrLoaderId}");
                    return;
                }
                Logger.Info(() => $"Saving config for mod: {modOrLoaderId}");
                mod.Config.Save();
            }
        }

        private static async IAsyncEnumerable<DataFeedItem> WorldNotUserspaceWarningAsync(IReadOnlyList<string> path)
        {
            var warning = new DataFeedIndicator<string>();
            warning.InitBase("Information", path, null, Mod.GetLocaleString("Information"));
            warning.InitSetupValue(field => field.AssignLocaleString(Mod.GetLocaleKey("WorldNotUserspace").AsLocaleKey()));
            yield return warning;
        }

        private static async IAsyncEnumerable<DataFeedItem> YieldBreakAsync()
        {
            yield break;
        }
    }
}