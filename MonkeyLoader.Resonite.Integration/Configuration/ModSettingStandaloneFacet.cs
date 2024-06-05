﻿using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using MonkeyLoader.Configuration;
using MonkeyLoader.Logging;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MonkeyLoader.Resonite.Configuration
{
    internal sealed class ModSettingStandaloneFacet : ResoniteMonkey<ModSettingStandaloneFacet>
    {
        public const string ConfigKeyChangeLabel = "StandaloneFacet";

        private const string ModSettingStandaloneFacetTag = "MonkeyLoaderStandaloneFacet";

        private static readonly MethodInfo _syncWithConfigKeyWrapperMethod = AccessTools.Method(typeof(ModSettingStandaloneFacet), nameof(SyncWithConfigKeyWrapper));

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => Enumerable.Empty<IFeaturePatch>();

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
                            return modConfigKey;

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

        private static void SyncWithConfigKeyWrapper<T>(IField field, IDefiningConfigKey key, string? eventLabel)
            => ((IField<T>)field).SyncWithConfigKey((IDefiningConfigKey<T>)key, eventLabel);

        [HarmonyPatch(typeof(Facet), nameof(Facet.OnLoading))]
        [HarmonyPatchCategory(nameof(ModSettingStandaloneFacet))]
        private class FacetPatch
        {
            [HarmonyPostfix]
            private static void OnLoadingPostfix(Facet __instance)
            {
                if (!__instance.World.IsUserspace())
                    return;

                __instance.RunSynchronously(() =>
                {
                    if (__instance.FilterWorldElement() is null
                        || __instance.Slot.Tag != ModSettingStandaloneFacetTag)
                        return;

                    if (__instance.Slot.GetComponentInChildren<FeedItemInterface>() is not FeedItemInterface feedItemInterface
                        || feedItemInterface?.Slot.GetComponent<Comment>()?.Text.Value is not string commentText)
                    {
                        Logger.Warn(() => "Attempted to load a Facet with the standalone ModSetting tag that was missing its FeedItemInterface and/or config key id.");
                        Logger.Warn(() => __instance.Slot);
                        return;
                    }

                    Logger.Info(() => "Loaded a mod setting standalone facet!");
                    Logger.Debug(() => "Config Key FullId: " + commentText);

                    if (GetConfigKeyByFullId(commentText) is not IDefiningConfigKey foundKey)
                    {
                        Logger.Error(() => $"Could not sync a config key with a standalone facet! Comment text: {commentText}");
                        return;
                    }

                    Logger.Info(() => $"Got config key! OwnerID: {foundKey.Config.Owner.Id} SectionID: {foundKey.Section.Id} KeyID: {foundKey.Id}");

                    if (feedItemInterface.GetSyncMember("Value") is ISyncRef valueFieldRef && valueFieldRef.Target is IField valueField)
                    {
                        var genericMethod = _syncWithConfigKeyWrapperMethod.MakeGenericMethod(new Type[] { valueField.ValueType });
                        genericMethod.Invoke(null, new object[] { valueField, foundKey, ConfigKeyChangeLabel });
                        return;
                    }
                });
            }
        }

        [HarmonyPatch(typeof(UIGrabInstancer), nameof(UIGrabInstancer.TryGrab))]
        [HarmonyPatchCategory(nameof(ModSettingStandaloneFacet))]
        private class UIGrabInstancerPatch
        {
            [HarmonyPostfix]
            private static void TryGrabPostfix(UIGrabInstancer __instance, IGrabbable? __result)
            {
                if (!__instance.World.IsUserspace() || __result is not Grabbable 
                    || __result?.Slot.GetComponent<Facet>() is null
                    || __instance.Slot.GetComponentInParents<FeedItemInterface>() is null
                    || __instance.Slot.GetComponentInParents<SettingsDataFeed>() is null
                    || __result.Slot.GetComponentInChildren<FeedItemInterface>() is not FeedItemInterface feedItemInterface
                    || feedItemInterface?.Slot.GetComponent<Comment>()?.Text.Value is not string commentText)
                    return;

                // Do these checks to make sure it's not a vanilla settings facet
                // This might not actually be needed, since vanilla facets probably don't have the comment component
                if (feedItemInterface.Slot.GetComponentInChildren<Component>(component => component.GetType().IsGenericType && component.GetType().GetGenericTypeDefinition() == typeof(SettingValueSync<,>)) != null)
                    return;

                Logger.Info(() => "Instantiated mod setting standalone facet!");

                Logger.Debug(() => "ItemName: " + feedItemInterface.ItemName.Target?.Value ?? "NULL");
                Logger.Debug(() => "Config Key FullId: " + commentText);

                var foundKey = GetConfigKeyByFullId(commentText);
                if (foundKey is null)
                {
                    Logger.Error(() => $"Could not sync a config key with a standalone facet! Comment text: {commentText}");
                    return;
                }

                Logger.Info(() => $"Got config key! OwnerID: {foundKey.Config.Owner.Id} SectionID: {foundKey.Section.Id} KeyID: {foundKey.Id}");
                __result.Slot.Tag = ModSettingStandaloneFacetTag;

                if (foundKey.Section is MonkeyTogglesConfigSection
                    && feedItemInterface.ItemName.Target is IField<string> field)
                {
                    if (field.IsDriven && field.GetLocalizedDriver() is LocaleStringDriver localeStringDriver)
                    {
                        localeStringDriver.Key.Value = foundKey.GetLocaleKey("Name");
                    }
                    else
                    {
                        // Drive the field with the localized value
                        field.ActiveLink.ReleaseLink();
                        field.DriveLocalized(foundKey.GetLocaleKey("Name"));
                    }
                }

                if (feedItemInterface.GetSyncMember("Value") is ISyncRef valueFieldRef && valueFieldRef.Target is IField valueField)
                {
                    var genericMethod = _syncWithConfigKeyWrapperMethod.MakeGenericMethod(new Type[] { valueField.ValueType });
                    genericMethod.Invoke(null, new object[] { valueField, foundKey, ConfigKeyChangeLabel });

                    feedItemInterface.Slot.PersistentSelf = true;
                    return;
                }
            }
        }
    }
}