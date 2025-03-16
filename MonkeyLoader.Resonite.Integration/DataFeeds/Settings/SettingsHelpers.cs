using Elements.Core;
using EnumerableToolkit;
using FrooxEngine;
using MonkeyLoader.Configuration;
using MonkeyLoader.Logging;
using MonkeyLoader.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyLoader.Patching;
using Elements.Assets;

namespace MonkeyLoader.Resonite.DataFeeds.Settings
{
    /// <summary>
    /// Contains helper methods and constants for dynamically creating
    /// the MonkeyLoader category in the <see cref="SettingsDataFeed"/>.
    /// </summary>
    public static class SettingsHelpers
    {
        /// <summary>
        /// The <see cref="ConfigKeyChangedEventArgs{T}.Label">label</see> used when
        /// <see cref="IDefiningConfigKey{T}">config keys</see> are set to a new value from the settings.
        /// </summary>
        public const string ConfigKeyChangeLabel = "Settings";

        /// <summary>
        /// The name of the dedicated category under the usual <c>MonkeyLoader/&lt;Mod&gt;/</c> path
        /// that displays only the <see cref="Config"/> of the <see cref="Mod"/> or loader.
        /// </summary>
        public const string ConfigSections = "ConfigSections";

        /// <summary>
        /// The name of the dedicated category under the usual <c>MonkeyLoader/&lt;Mod&gt;/</c> path
        /// that displays only the <see cref="IEarlyMonkey">early monkeys</see> of the <see cref="Mod"/> or loader.
        /// </summary>
        public const string EarlyMonkeys = "EarlyMonkeys";

        /// <summary>
        /// The name of the dedicated category under the usual <c>MonkeyLoader/&lt;Mod&gt;/</c> path
        /// that displays only the meta data of the <see cref="Mod"/> or loader.
        /// </summary>
        public const string MetaData = "MetaData";

        /// <summary>
        /// The name of the MonkeyLoader category in the <see cref="SettingsDataFeed"/>.<br/>
        /// The category under the <c>MonkeyLoader/MonkeyLoader/</c> path
        /// displays the settings and <see cref="IMonkey">monkeys</see> of the loader.
        /// </summary>
        public const string MonkeyLoader = "MonkeyLoader";

        /// <summary>
        /// The name of the dedicated category under the usual <c>MonkeyLoader/&lt;Mod&gt;/</c> path
        /// that displays only the <see cref="IMonkey">regular monkeys</see> of the <see cref="Mod"/> or loader.
        /// </summary>
        public const string Monkeys = "Monkeys";

        /// <summary>
        /// The name of the dedicated category under the usual <c>MonkeyLoader/&lt;Mod&gt;/</c> path that displays
        /// only the information and toggles for the <see cref="IMonkey">monkeys</see> of the <see cref="Mod"/> or loader.
        /// </summary>
        public const string MonkeyToggles = "MonkeyToggles";

        /// <summary>
        /// The name of the pseudo-category under the usual <c>MonkeyLoader/&lt;Mod&gt;/</c> path that triggers
        /// <see cref="Config.Reset">resetting</see> the <see cref="Config"/> of the <see cref="Mod"/> or loader.
        /// </summary>
        public const string ResetConfig = "ResetConfig";

        /// <summary>
        /// The name of the pseudo-category under the usual <c>MonkeyLoader/&lt;Mod&gt;/</c> path that triggers
        /// <see cref="Config.Save">saving</see> the <see cref="Config"/> of the <see cref="Mod"/> or loader.
        /// </summary>
        public const string SaveConfig = "SaveConfig";

        private static readonly Dictionary<SettingsDataFeed, SettingsViewData> _settingsViewsByFeed = [];

        private static Logger Logger => MonkeyLoaderRootCategorySettingsItems.Logger;

        public static async IAsyncEnumerable<DataFeedItem> EnumerateMonkeysAsync(EnumerateDataFeedParameters<SettingsDataFeed> parameters, Mod mod, string monkeyType, Mod localeMod, bool forceCheck = false, bool canBeDisabled = true)
        {
            await Task.CompletedTask;

            var path = parameters.Path;

            var monkeys = monkeyType switch
            {
                SettingsHelpers.Monkeys => mod.Monkeys.ToArray(),
                SettingsHelpers.EarlyMonkeys => mod.EarlyMonkeys.ToArray(),
                _ => []
            };

            if (forceCheck)
                monkeys = monkeys.Where(monkey => monkey.CanBeDisabled == canBeDisabled).ToArray();

            if (monkeys.Length == 0)
                yield break;

            Array.Sort(monkeys, (left, right) =>
            {
                if (left.CanBeDisabled != right.CanBeDisabled)
                    return left.CanBeDisabled ? -1 : 1;

                return left.GetMessageInCurrent("Name").CompareTo(right.GetMessageInCurrent("Name"));
            });

            string id = monkeyType;
            if (forceCheck)
            {
                if (canBeDisabled)
                {
                    id += "Toggleable";
                }
                else
                {
                    id += "NonToggleable";
                }
            }

            var group = new DataFeedGroup();
            group.InitBase(id, path, parameters.GroupKeys, localeMod.GetLocaleString($"{id}.Name"), localeMod.GetLocaleString($"{id}.Description"));
            yield return group;

            var monkeysGrouping = parameters.GroupKeys.Concat(id).ToArray();

            //var monkeyCount = new DataFeedIndicator<string>();
            //monkeyCount.InitBase($"{id}.Count", path, monkeysGrouping, localeMod.GetLocaleString($"{id}.Count.Name"), localeMod.GetLocaleString($"{id}.Count.Description"));
            //monkeyCount.InitSetupValue(field => field.Value = monkeys.Length.ToString());
            //yield return monkeyCount;

            foreach (var monkey in monkeys)
            {
                if (forceCheck && monkey.CanBeDisabled != canBeDisabled) continue;

                if (monkey is ICustomDataFeedItems customItems)
                {
                    await foreach (var item in customItems.Enumerate(parameters.Path, monkeysGrouping, parameters.SearchPhrase, parameters.ViewData))
                        yield return item;

                    continue;
                }

                if (monkey.CanBeDisabled)
                {
                    var toggle = new DataFeedToggle();
                    toggle.InitBase($"{monkey.Id}.Enabled", path, monkeysGrouping, monkey.GetLocaleString("Name"), monkey.GetLocaleString("Description"));
                    toggle.InitSetupValue(field => field.SetupConfigKeyField(monkey.EnabledToggle));
                    yield return toggle;
                }
                else
                {
                    var enabledIndicator = new DataFeedIndicator<string>();
                    enabledIndicator.InitBase($"{monkey.Id}.Enabled", path, monkeysGrouping, monkey.GetLocaleString("Name"), monkey.GetLocaleString("Description"));
                    enabledIndicator.InitSetupValue(field => field.AssignLocaleString(monkey.GetLocaleString("Description")));
                    yield return enabledIndicator;

                    //var descriptionIndicator = new DataFeedIndicator<string>();
                    //descriptionIndicator.InitBase($"{monkey.Id}.Description", path, monkeysGrouping, localeMod.GetLocaleString("Monkeys.Description.Name"), localeMod.GetLocaleString("Monkeys.Description.Description"));
                    //descriptionIndicator.InitSetupValue(field => field.AssignLocaleString(monkey.GetLocaleString("Description")));
                    //yield return descriptionIndicator;
                }

                //var typeIndicator = new DataFeedIndicator<string>();
                //typeIndicator.InitBase($"{monkey.Id}.Type", path, monkeysGrouping, localeMod.GetLocaleString("Monkeys.Type.Name"), localeMod.GetLocaleString($"{monkeyType}.Type.Description"));
                //typeIndicator.InitSetupValue(field => field.Value = monkey.Type.BaseType.CompactDescription());
                //yield return typeIndicator;
            }
        }

        /// <summary>
        /// Gets the <see cref="SettingsViewData"/> associated with this <see cref="SettingsDataFeed"/>.
        /// </summary>
        /// <param name="dataFeed">The <see cref="SettingsDataFeed"/> to get the view data for.</param>
        /// <returns>The view data associated with this data feed.</returns>
        public static SettingsViewData GetViewData(this SettingsDataFeed dataFeed)
            => _settingsViewsByFeed.GetOrCreateValue(dataFeed, () => CreateViewData(dataFeed));

        /// <summary>
        /// Shorthand for <see cref="DataFeedItem">DataFeedItem</see>.InitBase() with the
        /// <see cref="IIdentifiable.FullId">FullId</see> as well as the locale-name and locale-description
        /// of the given <see cref="IDefiningConfigKey"/>.
        /// </summary>
        /// <param name="item">The <see cref="DataFeedItem"/> to initialize.</param>
        /// <param name="path">The path to initialize the item with.</param>
        /// <param name="groupKeys">The group keys to initialize the item with.</param>
        /// <param name="configKey">The config key to initialize the id, and localize the name and description of the item with.</param>
        public static void InitBase(this DataFeedItem item, IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, IDefiningConfigKey configKey)
            => item.InitBase(configKey.FullId, path, groupKeys, configKey.GetLocaleString("Name"), configKey.GetLocaleString("Description"));

        /// <summary>
        /// Checks whether an editor template can be injected for the given <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to check.</param>
        /// <returns><c>true</c> if the <paramref name="type"/> is suitable for a template; otherwise, <c>false</c>.</returns>
        public static bool IsInjectableEditorType(this Type type)
            // Check with nameof for dummy, because there's also dummy<>
            => type.Name != nameof(dummy) && (Coder.IsEnginePrimitive(type) || type == typeof(Type));

        /// <summary>
        /// Checks whether an editor template can be injected for the given <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T">The type to check.</typeparam>
        /// <returns><c>true</c> if <typeparamref name="T"/> is suitable for a template; otherwise, <c>false</c>.</returns>
        public static bool IsInjectableEditorType<T>() => IsInjectableEditorType(typeof(T));

        /// <summary>
        /// Handles the standard case of setting up the field of a <see cref="DataFeedItem"/>
        /// to be synchronized with a <see cref="IDefiningConfigKey{T}">config key</see>.
        /// </summary>
        /// <remarks>
        /// Adds a <see cref="Comment"/> with the <paramref name="configKey"/>.<see cref="IIdentifiable.FullId">FullId</see>
        /// to allow easily mapping it back to the config key in the standalone facet process.
        /// </remarks>
        /// <typeparam name="T">The type of the field and config item's value.</typeparam>
        /// <param name="field">The field to synchronize with the <paramref name="configKey"/>.</param>
        /// <param name="configKey">The config key to synchronize with the <paramref name="field"/>.</param>
        public static void SetupConfigKeyField<T>(this IField<T> field, IDefiningConfigKey configKey)
        {
            var slot = field.FindNearestParent<Slot>();

            if (slot.GetComponentInParents<FeedItemInterface>() is FeedItemInterface feedItemInterface)
            {
                // Adding the config key's full id to make it easier to create standalone facets
                feedItemInterface.Slot.AttachComponent<Comment>().Text.Value = configKey.FullId;
            }

            field.SyncWithConfigKeyUntyped(configKey, ConfigKeyChangeLabel);
        }

        private static SettingsViewData CreateViewData(SettingsDataFeed dataFeed)
        {
            static void OnDestroyed(IDestroyable destroyable)
            {
                destroyable.Destroyed -= OnDestroyed;
                _settingsViewsByFeed.Remove((SettingsDataFeed)destroyable);

                Logger.Debug(() => $"Removed ViewData for SettingsDataFeed ({destroyable.ReferenceID})");
            }

            var viewData = new SettingsViewData(dataFeed);
            dataFeed.Destroyed += OnDestroyed;

            return viewData;
        }
    }
}