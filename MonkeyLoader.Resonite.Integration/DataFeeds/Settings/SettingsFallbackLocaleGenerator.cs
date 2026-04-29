using EnumerableToolkit;
using MonkeyLoader.Meta;
using MonkeyLoader.Resonite.Configuration;
using MonkeyLoader.Resonite.Locale;

namespace MonkeyLoader.Resonite.DataFeeds.Settings
{
    internal sealed class SettingsFallbackLocaleGenerator : ResoniteAsyncEventHandlerMonkey<SettingsFallbackLocaleGenerator, FallbackLocaleGenerationEvent>
    {
        public override int Priority => HarmonyLib.Priority.Normal;

        public override Sequence<string> SubgroupPath => SubgroupDefinitions.LocaleFallback;

        protected override bool AppliesTo(FallbackLocaleGenerationEvent eventData) => true;

        protected override Task Handle(FallbackLocaleGenerationEvent eventData)
            => Task.Run(() => GenerateLocaleMessages(eventData));

        private static void GenerateLocaleMessages(FallbackLocaleGenerationEvent eventData)
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

                foreach (var monkey in mod.Monkeys.Concat(mod.EarlyMonkeys))
                {
                    var monkeyNameKey = monkey.GetLocaleKey("Name");

                    eventData.AddMessage(monkeyNameKey, monkey.Name);
                    eventData.AddMessage(monkey.GetLocaleKey("Description"), "No Description");
                }

                foreach (var configSection in mod.Config.Sections)
                {
                    eventData.AddMessage(configSection.GetLocaleKey("Name"), configSection.Name);

                    foreach (var configKey in configSection.Keys)
                    {
                        eventData.AddMessage(configKey.GetLocaleKey("Name"), configKey.Id);
                        eventData.AddMessage(configKey.GetLocaleKey("Description"), configKey.Description ?? "No Description");

                        if (configKey.Components.TryGet<IConfigKeySubgroup>(out var subgroup))
                        {
                            var subgroupKey = configSection.FullId;

                            foreach (var pathElement in subgroup.SubgroupPath)
                            {
                                subgroupKey = $"{subgroupKey}.{pathElement}";

                                eventData.AddMessage($"{subgroupKey}.Name", pathElement);
                                eventData.AddMessage($"{subgroupKey}.Description", "No Description");
                            }
                        }
                    }
                }
            }
        }
    }
}