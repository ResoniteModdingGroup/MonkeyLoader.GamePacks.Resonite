using FrooxEngine;
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

        private static void SyncWithConfigKeyWrapper<T>(IField field, IDefiningConfigKey key, string? eventLabel)
        {
            ((IField<T>)field).SyncWithConfigKey((IDefiningConfigKey<T>)key, eventLabel);
        }

        [HarmonyPatch(typeof(Facet), nameof(Facet.OnLoading))]
        [HarmonyPatchCategory(nameof(ModSettingStandaloneFacet))]
        private class FacetPatch
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

        [HarmonyPatch(typeof(UIGrabInstancer), nameof(UIGrabInstancer.TryGrab))]
        [HarmonyPatchCategory(nameof(ModSettingStandaloneFacet))]
        private class UIGrabInstancerPatch
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
    }
}