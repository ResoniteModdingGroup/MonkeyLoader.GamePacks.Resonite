using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Configuration
{
    public sealed class ConfigManager
    {
        public JsonSerializerOptions

        private static JsonSerializer CreateJsonSerializer()
        {
            JsonSerializerSettings settings = new()
            {
                MaxDepth = 32,
                ReferenceLoopHandling = ReferenceLoopHandling.Error,
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
            };
            List<JsonConverter> converters = new();
            IList<JsonConverter> defaultConverters = settings.Converters;
            if (defaultConverters != null && defaultConverters.Count() != 0)
            {
                Logger.DebugFuncInternal(() => $"Using {defaultConverters.Count()} default json converters");
                converters.AddRange(defaultConverters);
            }
            converters.Add(new EnumConverter());
            converters.Add(new ResonitePrimitiveConverter());
            settings.Converters = converters;
            return JsonSerializer.Create(settings);
        }

        private static void ShutdownHook()
        {
            int count = 0;
            ModLoader.Mods()
                .Select(mod => mod.GetConfiguration())
                .Where(config => config != null)
                .Where(config => config!.AutoSave)
                .Where(config => config!.AnyValuesSet())
                .Do(config =>
                {
                    try
                    {
                        // synchronously save the config
                        config!.SaveInternal();
                    }
                    catch (Exception e)
                    {
                        Logger.ErrorInternal($"Error saving configuration for {config!.Owner.Name}:\n{e}");
                    }
                    count += 1;
                });
            Logger.MsgInternal($"Configs saved for {count} mods.");
        }
    }
}