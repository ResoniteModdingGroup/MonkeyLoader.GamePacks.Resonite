using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using MonkeyLoader.Resonite.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.DataFeeds.Settings
{
    internal sealed class SettingsFallbackLocaleGenerator : ResoniteAsyncEventHandlerMonkey<SettingsFallbackLocaleGenerator, FallbackLocaleGenerationEvent>
    {
        public override int Priority => HarmonyLib.Priority.Normal;

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
                    }
                }
            }
        }
    }
}