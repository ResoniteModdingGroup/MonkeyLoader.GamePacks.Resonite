using MonkeyLoader.Configuration;
using MonkeyLoader.Logging;
using MonkeyLoader.Meta;
using MonkeyLoader.Prepatching;
using Mono.Cecil;
using NuGet.Packaging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zio;
using Zio.FileSystems;

namespace MonkeyLoader
{
    public sealed class MonkeyLoader
    {
        public ConfigManager ConfigManager { get; private set; }
        public bool HasLoadedMods { get; private set; }
        public LocationConfigSection Locations { get; private set; }
        public MonkeyLogger Logger { get; private set; }
        //public IEnumerable<Monkey> Mods
        //{
        //    get
        //    {
        //        foreach (var mod in mods)
        //            yield return mod;
        //    }
        //}

        public MonkeyLoader(LocationConfiguration? locations = null)
        {
            Locations = locations ?? new();
        }

        public IEnumerable<Mod> LoadAllMods()
             => Locations.Mods.SelectMany(path => LoadMods(path));

        public IEnumerable<Mod> LoadMods(string directory)
        {
            var files = Enumerable.Empty<string>();

            try
            {
                directory = Path.GetFullPath(directory);

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);

                    yield break;
                }

                files = Directory.GetDirectories(directory);
            }
            catch (Exception ex)
            {
                Logger.Error(() => $"Exception while trying to enumerate files at: {directory}{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }

            foreach (var file in files)
            {
                if (TryLoadMod(file, out var mod))
                    yield return mod;
            }
        }

        /// <summary>
        /// Attempts to load the given <paramref name="file"/> as a <paramref name="mod"/>.
        /// </summary>
        /// <param name="file">The path to the file to load as a mod.</param>
        /// <param name="mod">The resulting mod when successful, or null when not.</param>
        /// <returns><c>true</c> when the file was successfully loaded.</returns>
        public bool TryLoadMod(string file, [NotNullWhen(true)] out Mod? mod)
        {
            mod = null;

            if (!File.Exists(file))
                return false;

            try
            {
                var fileSystem = new ZipArchiveFileSystem(file, ZipArchiveMode.Read, isCaseSensitive: true);

                mod = new Mod(Logger, file, fileSystem);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(() => $"Exception while trying to load mod: {file}{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }

            return false;
        }

        private void prepatch(IEnumerable<EarlyMonkey> prePatchers)
        {
            // should be case insensitive
            var assemblyDefinitions = new Dictionary<string, AssemblyDefinition>();
            var neededDefinitions = new Dictionary<string, AssemblyDefinition>();

            foreach (var prePatcher in prePatchers)
            {
                neededDefinitions.Clear();
            }
        }
    }
}