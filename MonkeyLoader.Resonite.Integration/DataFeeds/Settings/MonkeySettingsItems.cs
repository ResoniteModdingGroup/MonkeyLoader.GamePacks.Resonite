using EnumerableToolkit;
using FrooxEngine;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.DataFeeds.Settings
{
    internal sealed class MonkeySettingsItems : DataFeedBuildingBlockMonkey<MonkeySettingsItems, SettingsDataFeed>
    {
        public override int Priority => 100;

        public override IAsyncEnumerable<DataFeedItem> Apply(IAsyncEnumerable<DataFeedItem> current, EnumerateDataFeedParameters<SettingsDataFeed> parameters)
        {
            var path = parameters.Path;

            if (path.Count is < 2 or > 4 || path[0] is not SettingsHelpers.MonkeyLoader || path[1] is SettingsHelpers.MonkeyLoader)
                return current;

            if (path.Count >= 3 && path[2] is not SettingsHelpers.MonkeyToggles)
                return current;

            if (path.Count == 4 && path[2] is not SettingsHelpers.Monkeys or SettingsHelpers.EarlyMonkeys)
                return current;

            // Format: MonkeyLoader / modId / [page]
            if (!Mod.Loader.TryGet<Mod>().ById(path[1], out var mod))
            {
                Logger.Error(() => $"Tried to access non-existant mod's settings: {path[1]}");
                return current;
            }

            parameters.IncludeOriginalResult = false;

            if (path.Count == 4)
                return current.Concat(EnumerateMonkeysAsync(parameters, mod, path[3]));

            return current.Concat(EnumerateMonkeysAsync(parameters, mod, SettingsHelpers.Monkeys))
                .Concat(EnumerateMonkeysAsync(parameters, mod, SettingsHelpers.EarlyMonkeys));
        }

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => [];

        private static async IAsyncEnumerable<DataFeedItem> EnumerateMonkeysAsync(EnumerateDataFeedParameters<SettingsDataFeed> parameters, Mod mod, string monkeyType)
        {
            await Task.CompletedTask;

            var path = parameters.Path;

            var monkeys = monkeyType switch
            {
                SettingsHelpers.Monkeys => mod.Monkeys.ToArray(),
                SettingsHelpers.EarlyMonkeys => mod.EarlyMonkeys.ToArray(),
                _ => []
            };

            if (monkeys.Length == 0)
                yield break;

            Array.Sort(monkeys, (left, right) =>
            {
                if (left.CanBeDisabled != right.CanBeDisabled)
                    return left.CanBeDisabled ? -1 : 1;

                return left.GetMessageInCurrent("Name").CompareTo(right.GetMessageInCurrent("Name"));
            });

            var group = new DataFeedGroup();
            group.InitBase(monkeyType, path, parameters.GroupKeys, Mod.GetLocaleString($"{monkeyType}.Name"), Mod.GetLocaleString($"{monkeyType}.Description"));
            yield return group;

            var monkeysGrouping = parameters.GroupKeys.Concat(monkeyType).ToArray();

            var monkeyCount = new DataFeedIndicator<string>();
            monkeyCount.InitBase($"{monkeyType}.Count", path, monkeysGrouping, Mod.GetLocaleString($"{monkeyType}.Count.Name"), Mod.GetLocaleString($"{monkeyType}.Count.Description"));
            monkeyCount.InitSetupValue(field => field.Value = monkeys.Length.ToString());
            yield return monkeyCount;

            foreach (var monkey in monkeys)
            {
                var monkeyGroup = new DataFeedGroup();
                monkeyGroup.InitBase($"{monkey.Id}", path, monkeysGrouping, monkey.GetLocaleString("Name"));
                yield return monkeyGroup;

                var monkeyGrouping = monkeysGrouping.Concat(monkey.Id).ToArray();

                if (monkey is ICustomDataFeedItems customItems)
                {
                    await foreach (var item in customItems.Enumerate(parameters.Path, monkeyGrouping, parameters.SearchPhrase, parameters.ViewData))
                        yield return item;

                    continue;
                }

                if (monkey.CanBeDisabled)
                {
                    var toggle = new DataFeedToggle();
                    toggle.InitBase($"{monkey.Id}.Enabled", path, monkeyGrouping, Mod.GetLocaleString($"{monkeyType}.Enabled.Name"), Mod.GetLocaleString($"{monkeyType}.Enabled.Description"));
                    toggle.InitSetupValue(field => field.SetupConfigKeyField(mod.MonkeyToggles.GetToggle(monkey)));
                    yield return toggle;
                }
                else
                {
                    var enabledIndicator = new DataFeedIndicator<string>();
                    enabledIndicator.InitBase($"{monkey.Id}.Enabled", path, monkeyGrouping, Mod.GetLocaleString($"{monkeyType}.Enabled.Name"), Mod.GetLocaleString($"{monkeyType}.Enabled.Description"));
                    enabledIndicator.InitSetupValue(field => field.AssignLocaleString(Mod.GetLocaleString($"{monkeyType}.AlwaysEnabled")));
                    yield return enabledIndicator;
                }

                var descriptionIndicator = new DataFeedIndicator<string>();
                descriptionIndicator.InitBase($"{monkey.Id}.Description", path, monkeyGrouping, Mod.GetLocaleString("Monkeys.Description.Name"), Mod.GetLocaleString("Monkeys.Description.Description"));
                descriptionIndicator.InitSetupValue(field => field.AssignLocaleString(monkey.GetLocaleString("Description")));
                yield return descriptionIndicator;

                var typeIndicator = new DataFeedIndicator<string>();
                typeIndicator.InitBase($"{monkey.Id}.Type", path, monkeyGrouping, Mod.GetLocaleString("Monkeys.Type.Name"), Mod.GetLocaleString($"{monkeyType}.Type.Description"));
                typeIndicator.InitSetupValue(field => field.Value = monkey.Type.BaseType.CompactDescription());
                yield return typeIndicator;
            }
        }
    }
}