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
    internal sealed class NonToggleableMonkeySettingsItems : DataFeedBuildingBlockMonkey<NonToggleableMonkeySettingsItems, SettingsDataFeed>
    {
        public override int Priority => 210;

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
                return current.Concat(SettingsHelpers.EnumerateMonkeysAsync(parameters, mod, path[3], Mod, forceCheck: true, canBeDisabled: false));

            return current.Concat(SettingsHelpers.EnumerateMonkeysAsync(parameters, mod, SettingsHelpers.Monkeys, Mod, forceCheck: true, canBeDisabled: false))
                .Concat(SettingsHelpers.EnumerateMonkeysAsync(parameters, mod, SettingsHelpers.EarlyMonkeys, Mod));
        }

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => [];
    }
}