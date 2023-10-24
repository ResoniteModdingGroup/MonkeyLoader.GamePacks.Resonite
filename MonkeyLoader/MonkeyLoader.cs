using MonkeyLoader.Configuration;
using MonkeyLoader.Logging;
using MonkeyLoader.Meta;
using MonkeyLoader.NuGet;
using MonkeyLoader.Patching;
using MonkeyLoader.Prepatching;
using Mono.Cecil;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Zio.FileSystems;

namespace MonkeyLoader
{
    /// <summary>
    /// The root of all mod loading.
    /// </summary>
    public sealed class MonkeyLoader : IConfigOwner, IShutdown
    {
        private readonly HashSet<Mod> _allMods = new();
        private ILoggingHandler? _loggingHandler;

        /// <summary>
        /// Gets the config that this loader uses to load <see cref="ConfigSection"/>s.
        /// </summary>
        public Config Config { get; }

        /// <summary>
        /// Gets the path where the loader's config file should be.
        /// </summary>
        public string ConfigPath { get; }

        /// <summary>
        /// Gets the <see cref="IEarlyMonkey"/>s of all loaded <see cref="Mods">Mods</see>.
        /// </summary>
        public IEnumerable<IEarlyMonkey> EarlyMonkeys => Mods.SelectMany(mod => mod.EarlyMonkeys);

        /// <summary>
        /// Gets all loaded game pack <see cref="Mod"/>s.
        /// </summary>
        public IEnumerable<Mod> GamePacks => _allMods.Where(mod => mod.IsGamePack);

        /// <summary>
        /// Gets the json serializer used by this loader and any mods it loads.<br/>
        /// Will be populated with any converters picked up from game integration packs.
        /// </summary>
        public JsonSerializer JsonSerializer { get; }

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
            get => _loggingHandler;
            set
            {
                _loggingHandler = value;

                if (value is not null)
                    Logger.FlushDeferredMessages();
            }
        }

        /// <summary>
        /// Gets all loaded regular <see cref="Mod"/>s.
        /// </summary>
        public IEnumerable<Mod> Mods => _allMods.Where(mod => !mod.IsGamePack);

        /// <summary>
        /// Gets the <see cref="IMonkey"/>s of all loaded <see cref="Mods">Mods</see>.
        /// </summary>
        public IEnumerable<IMonkey> Monkeys => Mods.SelectMany(mod => mod.Monkeys);

        /// <summary>
        /// Gets the NuGet manager used by this loader.
        /// </summary>
        public NuGetManager NuGet { get; private set; }

        /// <summary>
        /// Gets whether this loaders's <see cref="Shutdown">Shutdown</see>() failed when it was called.
        /// </summary>
        public bool ShutdownFailed { get; private set; }

        /// <summary>
        /// Gets whether this loader's <see cref="Shutdown">Shutdown</see>() method has been called.
        /// </summary>
        public bool ShutdownRan { get; private set; }

        internal Queue<MonkeyLogger.DeferredMessage> DeferredMessages { get; } = new();
        internal AssemblyPool GameAssemblyPool { get; } = new();
        internal AssemblyPool PatcherAssemblyPool { get; } = new();

        /// <summary>
        /// Creates a new mod loader with the given configuration file.
        /// </summary>
        /// <param name="configPath">The path to the configuration file to use.</param>
        public MonkeyLoader(string configPath = "MonkeyLoader.json")
        {
            Logger = new(this);
            ConfigPath = configPath;

            JsonSerializer = new();

            Config = new Config(this);
            Locations = Config.LoadSection<LocationConfigSection>();

            NuGet = new NuGetManager(this);
        }

        /// <summary>
        /// Instantiates and adds a <see cref="JsonConverter"/> instance of the given <typeparamref name="TConverter">converter type</typeparamref>
        /// to this loader's <see cref="JsonSerializer">JsonSerializer</see>.
        /// </summary>
        public void AddJsonConverter<TConverter>() where TConverter : JsonConverter, new()
            => AddJsonConverter(new TConverter());

        /// <summary>
        /// Adds the given <see cref="JsonConverter"/> instance to this loader's <see cref="JsonSerializer">JsonSerializer</see>.
        /// </summary>
        public void AddJsonConverter(JsonConverter jsonConverter) => JsonSerializer.Converters.Add(jsonConverter);

        /// <summary>
        /// Instantiates and adds a <see cref="JsonConverter"/> instance of the given <paramref name="converterType">converter type</paramref>
        /// to this loader's <see cref="JsonSerializer">JsonSerializer</see>.
        /// </summary>
        /// <param name="converterType">The <see cref="JsonConverter"/> derived type to instantiate.</param>
        public void AddJsonConverter(Type converterType)
            => AddJsonConverter((JsonConverter)Activator.CreateInstance(converterType));

        /// <summary>
        /// Searches the given <paramref name="assembly"/> for all instantiable types derived from <see cref="JsonConverter"/>,
        /// which are not decorated with the <see cref="IgnoreJsonConverterAttribute"/>.<br/>
        /// Instantiates adds an instance of them to this loader's <see cref="JsonSerializer">JsonSerializer</see>.
        /// </summary>
        /// <param name="assembly"></param>
        public void AddJsonConverters(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes().Instantiable<JsonConverter>())
            {
                if (type.GetCustomAttribute<IgnoreJsonConverterAttribute>() is null)
                    AddJsonConverter(type);
            }
        }

        /// <summary>
        /// Tries to create all <see cref="Locations">Locations</see> used by this loader.
        /// </summary>
        public void EnsureAllLocationsExist()
        {
            var locations = new[] { Locations.Configs, Locations.GamePacks, Locations.Libs };
            var modLocations = Locations.Mods.Select(modLocation => modLocation.Path).ToArray();

            Logger.Info(() => $"Ensuring that all configured locations exist as directories:{Environment.NewLine}" +
                $"    {nameof(Locations.Configs)}: {Locations.Configs}{Environment.NewLine}" +
                $"    {nameof(Locations.GamePacks)}: {Locations.GamePacks}{Environment.NewLine}" +
                $"    {nameof(Locations.Libs)}: {Locations.Libs}{Environment.NewLine}" +
                $"    {nameof(Locations.Mods)}:{Environment.NewLine}" +
                $"      - {string.Join(Environment.NewLine + "      - ", modLocations)}");

            foreach (var location in locations.Concat(modLocations))
            {
                try
                {
                    Directory.CreateDirectory(location);
                }
                catch (Exception ex)
                {
                    Logger.Error(() => ex.Format($"Exception while trying to create directory: {location}"));
                }
            }
        }

        /// <summary>
        /// Performs the full loading routine without customizations or interventions.
        /// </summary>
        public void FullLoad()
        {
            EnsureAllLocationsExist();
            LoadAllGamePacks();
            LoadAllMods();

            LoadGamePackEarlyMonkeys();
            RunGamePackEarlyMonkeys();

            LoadModEarlyMonkeys();
            RunModEarlyMonkeys();

            LoadGameAssemblies();

            LoadGamePackMonkeys();
            RunGamePackMonkeys();

            LoadModMonkeys();
            RunModMonkeys();
        }

        /// <summary>
        /// Loads all game pack mods from the <see cref="LocationConfigSection">configured</see> <see cref="LocationConfigSection.GamePacks"> location</see>.
        /// </summary>
        /// <returns>All successfully loaded game pack mods.</returns>
        public IEnumerable<Mod> LoadAllGamePacks()
        {
            try
            {
                return Directory.EnumerateFiles(Locations.GamePacks, Mod.SearchPattern, SearchOption.TopDirectoryOnly)
                    .TrySelect<string, Mod>(TryLoadGamePack)
                    .ToArray();
            }
            catch (Exception ex)
            {
                Logger.Error(() => ex.Format($"Exception while searching files at location {Locations.GamePacks}:"));
                return Enumerable.Empty<Mod>();
            }
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
                    Logger.Error(() => ex.Format($"Exception while searching files at location {location}:"));
                }

                return Enumerable.Empty<string>();
            })
            .TrySelect<string, Mod>(TryLoadMod)
            .ToArray();
        }

        /// <summary>
        /// Loads every given <see cref="Mod"/>'s patcher assemblies and <see cref="IEarlyMonkey"/>s.
        /// </summary>
        public void LoadEarlyMonkeys(IEnumerable<Mod> mods)
        {
            foreach (var mod in mods)
                mod.LoadEarlyMonkeys();
        }

        /// <summary>
        /// Loads all of the game's assemblies from their potentially modified in-memory versions.
        /// </summary>
        public void LoadGameAssemblies()
        {
            GameAssemblyPool.LoadAll();
        }

        /// <summary>
        /// Loads every loaded game pack <see cref="Mods">mod's</see> pre-patcher assemblies and <see cref="IEarlyMonkey"/>s.
        /// </summary>
        public void LoadGamePackEarlyMonkeys() => LoadEarlyMonkeys(_allMods.Where(mod => mod.IsGamePack));

        /// <summary>
        /// Loads every loaded game pack <see cref="Mods">mod's</see> patcher assemblies and <see cref="IMonkey"/>s.
        /// </summary>
        public void LoadGamePackMonkeys() => LoadMonkeys(_allMods.Where(mod => mod.IsGamePack));

        /// <summary>
        /// Loads the mod from the given path, making no checks.
        /// </summary>
        /// <param name="path">The path to the mod file.</param>
        /// <param name="isGamePack">Whether the mod is a game pack.</param>
        /// <returns>The loaded mod.</returns>
        public Mod LoadMod(string path, bool isGamePack = false)
        {
            var fileSystem = new ZipArchiveFileSystem(path, ZipArchiveMode.Read);

            var mod = new Mod(this, path, fileSystem, isGamePack);
            _allMods.Add(mod);

            return mod;
        }

        /// <summary>
        /// Loads every loaded regular <see cref="Mods">mod's</see> pre-patcher assemblies and <see cref="IEarlyMonkey"/>s.
        /// </summary>
        public void LoadModEarlyMonkeys() => LoadEarlyMonkeys(Mods);

        /// <summary>
        /// Loads every loaded regular <see cref="Mods">mod's</see> patcher assemblies and <see cref="IMonkey"/>s.
        /// </summary>
        public void LoadModMonkeys() => LoadMonkeys(Mods);

        /// <summary>
        /// Loads every given <see cref="Mod"/>'s patcher assemblies and <see cref="IMonkey"/>s.
        /// </summary>
        public void LoadMonkeys(IEnumerable<Mod> mods)
        {
            foreach (var mod in mods)
                mod.LoadMonkeys();
        }

        /// <summary>
        /// Runs every given <see cref="Mod"/>'s loaded
        /// <see cref="IEarlyMonkey"/>s <see cref="MonkeyBase.Run">Run</see>() method.
        /// </summary>
        public void RunEarlyMonkeys(IEnumerable<Mod> mods)
        {
            foreach (var mod in mods)
            {
                // Add check for mod.EarlyMonkeyLoadError

                foreach (var earlyMonkey in mod.EarlyMonkeys)
                    earlyMonkey.Run();
            }
        }

        /// <summary>
        /// Runs every loaded game pack <see cref="Mod"/>'s loaded <see cref="IEarlyMonkey"/>s.
        /// </summary>
        public void RunGamePackEarlyMonkeys() => RunEarlyMonkeys(GamePacks);

        /// <summary>
        /// Runs every loaded game pack <see cref="Mod"/>'s loaded
        /// <see cref="Mod.Monkeys">monkeys'</see> <see cref="MonkeyBase.Run">Run</see>() method.
        /// </summary>
        public void RunGamePackMonkeys() => RunMonkeys(GamePacks);

        /// <summary>
        /// Runs every loaded regular <see cref="Mod"/>'s loaded <see cref="IEarlyMonkey"/>s.
        /// </summary>
        public void RunModEarlyMonkeys() => RunEarlyMonkeys(Mods);

        /// <summary>
        /// Runs every loaded regular <see cref="Mod"/>'s loaded
        /// <see cref="Mod.Monkeys">monkeys'</see> <see cref="MonkeyBase.Run">Run</see>() method.
        /// </summary>
        public void RunModMonkeys() => RunMonkeys(Mods);

        /// <summary>
        /// Runs every given <see cref="Mod"/>'s loaded
        /// <see cref="Mod.Monkeys">monkeys'</see> <see cref="MonkeyBase.Run">Run</see>() method.
        /// </summary>
        public void RunMonkeys(IEnumerable<Mod> mods)
        {
            foreach (var mod in mods)
            {
                foreach (var monkey in mod.Monkeys)
                    monkey.Run();
            }
        }

        /// <summary>
        /// Should be called by the game integration or application using this as a library when things are shutting down.<br/>
        /// Saves its config and triggers <see cref="Mod.Shutdown">Shutdown</see>() on all <see cref="Mods">Mods</see>.
        /// </summary>
        /// <inheritdoc/>
        public bool Shutdown()
        {
            if (ShutdownRan)
                throw new InvalidOperationException("A loader's Shutdown() method must only be called once!");

            ShutdownRan = true;

            var sw = Stopwatch.StartNew();
            Logger.Info(() => $"Triggering shutdown routine! Saving the loader's config.");

            try
            {
                Logger.Debug(() => $"Triggering save for the mod loader's config to shut down!");
                Config.Save();
            }
            catch (Exception ex)
            {
                ShutdownFailed = true;
                Logger.Error(() => ex.Format("The mod loader's config threw an exception while saving during shutdown!"));
            }

            Logger.Info(() => $"Triggering shutdown for all {_allMods.Count} mods!");

            foreach (var mod in _allMods)
                ShutdownFailed |= !mod.Shutdown();

            Logger.Info(() => $"Processed shutdown in {sw.ElapsedMilliseconds}ms!");

            return !ShutdownFailed;
        }

        /// <summary>
        /// Tries to get the <see cref="AssemblyDefinition"/> for the given <see cref="AssemblyName"/> from
        /// the <see cref="GameAssemblyPool">GameAssemblyPool</see> or the <see cref="PatcherAssemblyPool">PatcherAssemblyPool</see>.
        /// </summary>
        /// <param name="assemblyName">The assembly to look for.</param>
        /// <param name="assemblyPool">The pool it came from if found, or <c>null</c> otherwise.</param>
        /// <param name="assemblyDefinition">The <see cref="AssemblyDefinition"/> if found, or <c>null</c> otherwise.</param>
        /// <returns>Whether the <see cref="AssemblyDefinition"/> could be returned.</returns>
        public bool TryGetAssemblyDefinition(AssemblyName assemblyName,
            [NotNullWhen(true)] out AssemblyPool? assemblyPool, [NotNullWhen(true)] out AssemblyDefinition? assemblyDefinition)
        {
            lock (this)
            {
                if (GameAssemblyPool.TryWaitForDefinition(assemblyName, out assemblyDefinition))
                {
                    assemblyPool = GameAssemblyPool;
                    return true;
                }

                if (PatcherAssemblyPool.TryWaitForDefinition(assemblyName, out assemblyDefinition))
                {
                    assemblyPool = PatcherAssemblyPool;
                    return true;
                }

                assemblyPool = null;
                return false;
            }
        }

        /// <summary>
        /// Attempts to load the given <paramref name="path"/> as a <paramref name="mod"/>.
        /// </summary>
        /// <param name="path">The path to the file to load as a mod.</param>
        /// <param name="mod">The resulting mod when successful, or null when not.</param>
        /// <param name="isGamePack">Whether the mod is a game pack.</param>
        /// <returns><c>true</c> when the file was successfully loaded.</returns>
        public bool TryLoadMod(string path, [NotNullWhen(true)] out Mod? mod, bool isGamePack = false)
        {
            mod = null;

            if (!File.Exists(path))
                return false;

            try
            {
                mod = LoadMod(path, isGamePack);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(() => ex.Format($"Exception while trying to load mod from {path}:"));
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
                Logger.Error(() => ex.Format($"Some {nameof(OnAnyConfigChanged)} event subscribers threw an exception:"));
            }
        }

        private bool TryLoadGamePack(string path, [NotNullWhen(true)] out Mod? gamePack)
            => TryLoadMod(path, out gamePack, true);

        private bool TryLoadMod(string path, [NotNullWhen(true)] out Mod? mod)
            => TryLoadMod(path, out mod, false);

        /// <summary>
        /// Called when the value of any of this loader's configs changes.<br/>
        /// This gets fired <i>after</i> the source config's <see cref="Config.OnChanged">ConfigurationChanged</see> event.
        /// </summary>
        public event ConfigChangedEventHandler? OnAnyConfigChanged;
    }
}