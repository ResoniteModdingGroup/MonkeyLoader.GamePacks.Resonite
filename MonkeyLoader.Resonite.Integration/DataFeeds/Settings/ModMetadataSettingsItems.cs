using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.DataFeeds.Settings
{
    internal sealed class ModMetadataSettingsItems : DataFeedBuildingBlockMonkey<ModMetadataSettingsItems, SettingsDataFeed>
    {
        public override int Priority => 200;

        public override IAsyncEnumerable<DataFeedItem> Apply(IAsyncEnumerable<DataFeedItem> current, EnumerateDataFeedParameters<SettingsDataFeed> parameters)
        {
            var path = parameters.Path;

            if (path.Count is < 2 or > 3 || path[0] is not SettingsHelpers.MonkeyLoader)
                return current;

            if (path.Count == 3 && path[2] is not SettingsHelpers.MetaData)
                return current;

            // Format: MonkeyLoader / modId / [page]
            if (!Mod.Loader.TryGet<Mod>().ById(path[1], out var mod))
            {
                Logger.Error(() => $"Tried to access non-existant mod's settings: {path[1]}");
                return current;
            }

            parameters.IncludeOriginalResult = false;

            return current.Concat(EnumerateModMetadataAsync(parameters, mod));
        }

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => [];

        private static async IAsyncEnumerable<DataFeedItem> EnumerateModMetadataAsync(EnumerateDataFeedParameters<SettingsDataFeed> parameters, Mod mod)
        {
            await Task.CompletedTask;

            var path = parameters.Path;

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
    }
}