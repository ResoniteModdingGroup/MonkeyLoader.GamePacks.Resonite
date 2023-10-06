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
    /// <summary>
    /// The root of all mod loading.
    /// </summary>
    public sealed class MonkeyLoader : IConfigOwner
    {
        private readonly HashSet<Mod> mods = new();
        private ILoggingHandler? loggingHandler;

        /// <summary>
        /// Gets the config that this loader uses to load <see cref="ConfigSection"/>s.
        /// </summary>
        public Config Config { get; }

        public ConfigManager ConfigManager { get; private set; }

        /// <summary>
        /// Gets the path where the loader's config file should be.
        /// </summary>
        public string ConfigPath { get; }

        /// <summary>
        /// Gets the <see cref="EarlyMonkey"/>s of all loaded <see cref="Mods">Mods</see>.
        /// </summary>
        public IEnumerable<EarlyMonkey> EarlyMonkeys => Mods.SelectMany(mod => mod.EarlyMonkeys);

        public bool HasLoadedMods { get; private set; }
        MonkeyLoader IConfigOwner.Loader => this;

        /// <summary>
        /// Gets the configuration for which paths will be searched for certain resources.
        /// </summary>
        public LocationConfigSection Locations { get; private set; }

        /// <summary>
        /// Gets the logger used by the loader and "inherited" by everything loaded by it.
        /// </summary>
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

        public MonkeyLoader(string configPath = "MonkeyLoader.json")
        {
            Logger = new(this);
            ConfigPath = configPath;

            Config = new Config(this);
            Locations = Config.LoadSection<LocationConfigSection>();
        }

        /// <summary>
        /// Loads all mods from the <see cref="LocationConfigSection">configured</see> <see cref="ModLoadingLocation">locations</see>.
        /// </summary>
        /// <returns>All successfully loaded mods.</returns>
        public IEnumerable<Mod> LoadAllMods()
        {
            return Locations.Mods.SelectMany(location =>
            {
                try
                {
                    return location.Search();
                }
                catch (Exception ex)
                {
                    Logger.Error(() => $"Exception while searching files at location {location}:{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                }

                return Enumerable.Empty<string>();
            })
            .TrySelect<string, Mod>(TryLoadMod);
        }

        /// <summary>
        /// Loads the mod from the given path, making no checks.
        /// </summary>
        /// <returns>The loaded mod.</returns>
        public Mod LoadMod(string path)
        {
            var fileSystem = new ZipArchiveFileSystem(path, ZipArchiveMode.Read, isCaseSensitive: true);

            return new Mod(this, path, fileSystem);
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
        /// Attempts to load the given <paramref name="path"/> as a <paramref name="mod"/>.
        /// </summary>
        /// <param name="path">The path to the file to load as a mod.</param>
        /// <param name="mod">The resulting mod when successful, or null when not.</param>
        /// <returns><c>true</c> when the file was successfully loaded.</returns>
        public bool TryLoadMod(string path, [NotNullWhen(true)] out Mod? mod)
        {
            mod = null;

            if (!File.Exists(path))
                return false;

            try
            {
                mod = LoadMod(path);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(() => $"Exception while trying to load mod: {path}{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }

            return false;
        }

        internal void FireConfigChangedEvent(ConfigChangedEvent configChangedEvent)
        {
            try
            {
                OnAnyConfigChanged?.TryInvokeAll(configChangedEvent);
            }
            catch (AggregateException ex)
            {
                Logger.Error(() => ex.Format("Some OnAnyConfigurationChanged event subscribers threw an exception:"));
            }
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

        /// <summary>
        /// Called when the value of any of this loader's configs changes.<br/>
        /// This gets fired <i>after</i> the source config's <see cref="Config.OnChanged">ConfigurationChanged</see> event.
        /// </summary>
        public event Config.ConfigChangedEventHandler? OnAnyConfigChanged;
    }
}