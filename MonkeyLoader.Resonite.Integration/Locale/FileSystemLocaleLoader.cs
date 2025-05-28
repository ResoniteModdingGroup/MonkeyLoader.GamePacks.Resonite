using Elements.Assets;
using MonkeyLoader.Logging;
using MonkeyLoader.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Zio;

namespace MonkeyLoader.Resonite.Locale
{
    internal sealed class FileSystemLocaleLoader : ResoniteAsyncEventHandlerMonkey<FileSystemLocaleLoader, LocaleLoadingEvent>
    {
        /// <inheritdoc/>
        public override int Priority => HarmonyLib.Priority.First;

        protected override bool AppliesTo(LocaleLoadingEvent eventData) => true;

        /// <summary>
        /// Handles the given locale loading event by checking every loaded mod's
        /// <see cref="Mod.FileSystem">FileSystem</see> for matching <c>Locale/[localeCode].json</c> files.
        /// </summary>
        /// <inheritdoc/>
        protected override async Task Handle(LocaleLoadingEvent eventData)
        {
            var searchPath = (new UPath("Locale") / $"{eventData.LocaleCode}.json").ToRelative().ToString();

            foreach (var mod in Mod.Loader.Mods)
            {
                foreach (var localeFilePath in mod.ContentPaths.Where(path => path.ToString().EndsWith(searchPath, StringComparison.OrdinalIgnoreCase)))
                {
                    try
                    {
                        using var localeFileStream = mod.FileSystem.OpenFile(localeFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                        var localeData = await JsonSerializer.DeserializeAsync<LocaleData>(localeFileStream);

                        if (localeData is null)
                            continue;

                        if (!eventData.LocaleCode.Equals(localeData.LocaleCode, StringComparison.OrdinalIgnoreCase))
                            Logger.Warn(() => $"Detected locale data with wrong locale code from locale file! Wanted [{eventData.LocaleCode}] - got [{localeData.LocaleCode}] in file: {mod.Id}:/{localeFilePath}");

                        eventData.LocaleResource.LoadDataAdditively(localeData);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(() => ex.Format($"Failed to deserialize file as LocaleData: {mod.Id}:/{localeFilePath}"));
                    }
                }
            }
        }
    }
}