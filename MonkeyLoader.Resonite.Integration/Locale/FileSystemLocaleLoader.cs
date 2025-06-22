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
        private static readonly string _localeDirectory = $"{UPath.DirectorySeparator}Locale{UPath.DirectorySeparator}";

        private static readonly Dictionary<string, Func<UPath, bool>> _predicatesByLocaleCode = new(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc/>
        public override int Priority => HarmonyLib.Priority.First;

        /// <summary>
        /// Handles the given locale loading event by checking every loaded mod's
        /// <see cref="Mod.FileSystem">FileSystem</see> for matching <c>Locale/[localeCode].json</c> files.
        /// </summary>
        /// <inheritdoc/>
        protected override async Task Handle(LocaleLoadingEvent eventData)
        {
            var localePredicate = GetPredicateForLocaleCode(eventData.LocaleCode);

            foreach (var mod in Mod.Loader.Mods)
            {
                foreach (var localeFilePath in mod.ContentPaths.Where(localePredicate))
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

        private static Func<UPath, bool> GetPredicateForLocaleCode(string localeCode)
        {
            if (!_predicatesByLocaleCode.TryGetValue(localeCode, out var predicate))
            {
                var fileName = $"{UPath.DirectorySeparator}{localeCode}.json";

                predicate = path => path.FullName.EndsWith(fileName, StringComparison.OrdinalIgnoreCase)
                    && path.FullName.Contains(_localeDirectory, StringComparison.OrdinalIgnoreCase);

                _predicatesByLocaleCode.Add(localeCode, predicate);
            }

            return predicate;
        }
    }
}