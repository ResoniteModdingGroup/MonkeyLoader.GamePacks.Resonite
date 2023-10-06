using MonkeyLoader.Configuration;
using MonkeyLoader.Logging;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using MonkeyLoader.Prepatching;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zio.FileSystems;

namespace MonkeyLoader
{
    public sealed class MonkeyLoader
    {
        private readonly HashSet<Mod> mods = new();
        private ILoggingHandler? loggingHandler;

        public ConfigManager ConfigManager { get; private set; }

        /// <summary>
        /// Gets the <see cref="EarlyMonkey"/>s of all loaded <see cref="Mods">Mods</see>.
        /// </summary>
        public IEnumerable<EarlyMonkey> EarlyMonkeys => Mods.SelectMany(mod => mod.EarlyMonkeys);

        public bool HasLoadedMods { get; private set; }
        public LocationConfigSection Locations { get; private set; }
        public MonkeyLogger Logger { get; }

        /// <summary>
        /// Gets or sets the logging handler used by the loader and all <see cref="Mods">Mods</see>.
        /// </summary>
        public ILoggingHandler? LoggingHandler
        {
            get => loggingHandler;
            set
            {
                loggingHandler = value;

                if (value is not null)
                    Logger.FlushDeferredMessages();
            }
        }

        /// <summary>
        /// Gets all loaded <see cref="Mod"/>s.
        /// </summary>
        public IEnumerable<Mod> Mods
        {
            get
            {
                foreach (var mod in mods)
                    yield return mod;
            }
        }

        /// <summary>
        /// Gets the <see cref="Monkey"/>s of all loaded <see cref="Mods">Mods</see>.
        /// </summary>
        public IEnumerable<Monkey> Monkeys => Mods.SelectMany(mod => mod.Monkeys);

        internal Queue<MonkeyLogger.DeferredMessage> DeferredMessages { get; } = new();

        public MonkeyLoader()
        {
            Logger = new(this);
        }

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
        /// Should be called by the game integration or application using this as a library when things are shutting down.<br/>
        /// Saves the configs of all mods etc.
        /// </summary>
        public void Shutdown()
        {
            foreach (var mod in mods)
                mod.Config.Save();
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

                mod = new Mod(this, file, fileSystem);
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