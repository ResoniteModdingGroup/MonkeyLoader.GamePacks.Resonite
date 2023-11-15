using HarmonyLib;
using MonkeyLoader.Configuration;
using MonkeyLoader.Logging;
using MonkeyLoader.NuGet;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Zio;

namespace MonkeyLoader.Meta
{
    /// <summary>
    /// Contains all metadata and references for a mod.
    /// </summary>
    public abstract class Mod : IConfigOwner, IShutdown, ILoadedNuGetPackage
    {
        /// <summary>
        /// Stores the authors of this mod.
        /// </summary>
        protected readonly HashSet<string> authors = new(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Stores the dependencies of this mod.
        /// </summary>
        protected readonly Dictionary<string, DependencyReference> dependencies = new(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Stores the pre-patchers of this mod.
        /// </summary>
        protected readonly HashSet<IEarlyMonkey> earlyMonkeys = new();

        /// <summary>
        /// Stores the patchers of this mod.
        /// </summary>
        protected readonly HashSet<IMonkey> monkeys = new();

        /// <summary>
        /// Stores the tags of this mod.
        /// </summary>
        protected readonly HashSet<string> tags = new(StringComparer.InvariantCultureIgnoreCase);

        private bool _allDependenciesLoaded = false;

        /// <summary>
        /// Gets an <see cref="IComparer{T}"/> that keeps <see cref="Mod"/>s sorted in topological order.
        /// </summary>
        public static IComparer<Mod> Comparer { get; } = new ModComparer();

        /// <inheritdoc/>
        public bool AllDependenciesLoaded
        {
            get
            {
                if (!_allDependenciesLoaded)
                    _allDependenciesLoaded = dependencies.Values.All(dep => dep.AllDependenciesLoaded);

                return _allDependenciesLoaded;
            }
        }

        /// <summary>
        /// Gets the names of the authors of this mod.
        /// </summary>
        public IEnumerable<string> Authors => authors.AsSafeEnumerable();

        /// <summary>
        /// Gets the config that this mod's (pre-)patcher(s) can use to load <see cref="ConfigSection"/>s.
        /// </summary>
        public Config Config { get; }

        /// <summary>
        /// Gets the path where this mod's config file should be.
        /// </summary>
        public string ConfigPath { get; }

        /// <summary>
        /// Gets the dependencies of this mod.
        /// </summary>
        public IEnumerable<DependencyReference> Dependencies => dependencies.Values.AsSafeEnumerable();

        /// <summary>
        /// Gets the description of this mod.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the available <see cref="IEarlyMonkey"/>s of this mod.
        /// </summary>
        public IEnumerable<IEarlyMonkey> EarlyMonkeys => earlyMonkeys.AsSafeEnumerable();

        /// <summary>
        /// Gets the readonly file system of this mod's file.
        /// </summary>
        public IFileSystem FileSystem { get; }

        /// <summary>
        /// Gets the <see cref="HarmonyLib.Harmony"/> instance to be used by this mod's (pre-)patcher(s).
        /// </summary>
        public Harmony Harmony { get; }

        /// <summary>
        /// Gets whether this mod has any <see cref="Monkeys">monkeys</see>.
        /// </summary>
        public bool HasPatchers => monkeys.Count > 0;

        /// <summary>
        /// Gets whether this mod has any <see cref="EarlyMonkeys">early monkeys</see>.
        /// </summary>
        public bool HasPrePatchers => earlyMonkeys.Count > 0;

        /// <summary>
        /// Gets the path to the mod's icon inside the mod's <see cref="FileSystem">FileSystem</see>.<br/>
        /// <c>null</c> if it wasn't given or doesn't exist.
        /// </summary>
        public UPath? IconPath { get; }

        /// <summary>
        /// Gets the Url to the mod's icon on the web.<br/>
        /// <c>null</c> if it wasn't given or was invalid.
        /// </summary>
        public Uri? IconUrl { get; }

        /// <summary>
        /// Gets the unique identifier of this mod.
        /// </summary>
        public string Id => Identity.Id;

        /// <summary>
        /// Gets the package identity of this mod.
        /// </summary>
        public PackageIdentity Identity { get; }

        /// <summary>
        /// Gets whether this mod is a game pack.
        /// </summary>
        public bool IsGamePack { get; }

        /// <summary>
        /// Gets whether this mod's <see cref="LoadEarlyMonkeys">LoadEarlyMonkeys</see>() failed when it was called.
        /// </summary>
        public bool LoadEarlyMonkeysFailed { get; private set; }

        /// <summary>
        /// Gets whether this mod's <see cref="LoadEarlyMonkeys">LoadEarlyMonkeys</see>() method has been called.
        /// </summary>
        public bool LoadedEarlyMonkeys { get; private set; } = false;

        /// <summary>
        /// Gets whether this mod's <see cref="LoadMonkeys">LoadMonkeys</see>() method has been called.
        /// </summary>
        public bool LoadedMonkeys { get; private set; } = false;

        /// <summary>
        /// Gets the <see cref="MonkeyLoader"/> instance that loaded this mod.
        /// </summary>
        public MonkeyLoader Loader { get; }

        /// <summary>
        /// Gets whether this mod's <see cref="LoadEarlyMonkeys">LoadEarlyMonkeys</see>() or <see cref="LoadMonkeys">LoadMonkeys</see>() failed when they were called.
        /// </summary>
        public bool LoadFailed => LoadEarlyMonkeysFailed || LoadMonkeysFailed;

        /// <summary>
        /// Gets whether this mod's <see cref="LoadMonkeys">LoadMonkeys</see>() failed when it was called.
        /// </summary>
        public bool LoadMonkeysFailed { get; private set; } = false;

        /// <summary>
        /// Gets the logger to be used by this mod.
        /// </summary>
        /// <remarks>
        /// Every mod instance has its own logger and can thus have a different <see cref="LoggingLevel"/>.<br/>
        /// They do all share the <see cref="Loader">Loader's</see> <see cref="MonkeyLoader.LoggingHandler">LoggingHandler</see> though.
        /// </remarks>
        public MonkeyLogger Logger { get; }

        /// <summary>
        /// Gets the available <see cref="IMonkey"/>s of this mod.
        /// </summary>
        public IEnumerable<IMonkey> Monkeys => monkeys.AsSafeEnumerable();

        /// <summary>
        /// Gets the Url to this mod's project website.<br/>
        /// <c>null</c> if it wasn't given or was invalid.
        /// </summary>
        public Uri? ProjectUrl { get; }

        /// <summary>
        /// Gets the release notes for this mod's version.
        /// </summary>
        public string? ReleaseNotes { get; }

        /// <summary>
        /// Gets whether this <see cref="NuGetPackageMod"/>'s <see cref="Shutdown"/> method failed when it was called.
        /// </summary>
        public bool ShutdownFailed { get; private set; } = false;

        /// <summary>
        /// Gets whether this <see cref="NuGetPackageMod"/>'s <see cref="Shutdown"/> method has been called.
        /// </summary>
        public bool ShutdownRan { get; private set; } = false;

        /// <summary>
        /// Gets the tags of this mod.
        /// </summary>
        public IEnumerable<string> Tags => tags.AsSafeEnumerable();

        /// <summary>
        /// Gets the framework targeted by this mod.
        /// </summary>
        public NuGetFramework TargetFramework { get; }

        /// <summary>
        /// Gets the nice identifier of this mod.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets this mod's version.
        /// </summary>
        public NuGetVersion Version => Identity.Version;

        /// <summary>
        /// Creates a new mod instance with the given details.
        /// </summary>
        /// <param name="loader">The loader instance that loaded this mod.</param>
        /// <param name="identity">The package identity of this mod.</param>
        /// <param name="targetFramework">The framework that this mod targets.</param>
        /// <param name="isGamePack">Whether this mod is a game pack.</param>
        /// <param name="fileSystem">The file system this mod should use.</param>
        /// <param name="title">The nice identified of this mod.</param>
        /// <param name="description">The description of this mod.</param>
        /// <param name="releaseNotes">The release notes for this mod's version.</param>
        /// <param name="iconPath">The path to the mod's icon inside its <paramref name="fileSystem"/>.</param>
        /// <param name="iconUrl">The url to the mod's icon on the web.</param>
        /// <param name="projectUrl">The url to the mod's project on the web.</param>
        protected Mod(MonkeyLoader loader, PackageIdentity identity, NuGetFramework targetFramework, bool isGamePack,
            IFileSystem fileSystem, string title, string description, string? releaseNotes = null,
            UPath? iconPath = null, Uri? iconUrl = null, Uri? projectUrl = null)
        {
            Loader = loader;
            Identity = identity;
            TargetFramework = targetFramework;
            IsGamePack = isGamePack;
            FileSystem = fileSystem;
            Title = title;
            Description = description;
            ReleaseNotes = releaseNotes;
            IconPath = iconPath;
            IconUrl = iconUrl;
            ProjectUrl = projectUrl;

            Harmony = new Harmony(Id);
            Logger = new MonkeyLogger(loader.Logger, Title);

            ConfigPath = Path.Combine(Loader.Locations.Configs, $"{Id}.json");
            Config = new Config(this);
        }

        /// <summary>
        /// Efficiently checks, whether a given name is listed as an author for this mod.
        /// </summary>
        /// <param name="author">The name to check for.</param>
        /// <returns><c>true</c> if the given name is listed as an author for this mod.</returns>
        public bool HasAuthor(string author) => authors.Contains(author);

        /// <summary>
        /// Efficiently checks, whether a given tag is listed for this mod.
        /// </summary>
        /// <param name="tag">The tag to check for.</param>
        /// <returns><c>true</c> if the given tag is listed for this mod.</returns>
        public bool HasTag(string tag) => tags.Contains(tag);

        /// <summary>
        /// Lets this mod cleanup and shutdown.<br/>
        /// Must only be called once.
        /// </summary>
        /// <returns>Whether it ran successfully.</returns>
        /// <inheritdoc/>
        public bool Shutdown()
        {
            if (ShutdownRan)
                throw new InvalidOperationException("A mod's Shutdown() method must only be called once!");

            ShutdownRan = true;

            Config.Save();

            try
            {
                foreach (var earlyMonkey in earlyMonkeys)
                    ShutdownFailed |= !earlyMonkey.Shutdown();

                foreach (var monkey in monkeys)
                    ShutdownFailed |= monkey.Shutdown();
            }
            catch (Exception ex)
            {
                ShutdownFailed = true;
                Logger.Error(() => ex.Format("A mod's Shutdown() method threw an Exception:"));
            }

            return !ShutdownFailed;
        }

        public bool TryResolveDependencies()
            => dependencies.Values.Select(dep => dep.TryResolve()).All();

        internal bool LoadEarlyMonkeys()
        {
            if (LoadedEarlyMonkeys)
                throw new InvalidOperationException("A mod's LoadEarlyMonkeys() method must only be called once!");

            LoadedEarlyMonkeys = true;

            try
            {
                if (!OnLoadEarlyMonkeys())
                {
                    LoadEarlyMonkeysFailed = true;
                    Logger.Warn(() => "OnLoadEarlyMonkey failed!");
                }
            }
            catch (Exception ex)
            {
                LoadEarlyMonkeysFailed = true;
                Logger.Error(() => ex.Format("A mod's OnLoadEarlyMonkeys() method threw an Exception:"));
            }

            return !LoadEarlyMonkeysFailed;
        }

        internal bool LoadMonkeys()
        {
            if (LoadedMonkeys)
                throw new InvalidOperationException("A mod's LoadMonkeys() method must only be called once!");

            LoadedMonkeys = true;

            try
            {
                if (!OnLoadMonkeys())
                {
                    LoadMonkeysFailed = true;
                    Logger.Warn(() => "OnLoadEarlyMonkey failed!");
                }
            }
            catch (Exception ex)
            {
                LoadMonkeysFailed = true;
                Logger.Error(() => ex.Format("A mod's OnLoadMonkeys() method threw an Exception:"));
            }

            return !LoadMonkeysFailed;
        }

        /// <summary>
        /// Loads the mod's <see cref="EarlyMonkeys">early monkeys</see>.
        /// </summary>
        /// <returns>Whether it ran successfully.</returns>
        protected abstract bool OnLoadEarlyMonkeys();

        /// <summary>
        /// Loads the mod's <see cref="Monkeys">monkeys</see>.
        /// </summary>
        /// <returns>Whether it ran successfully.</returns>
        protected abstract bool OnLoadMonkeys();

        private sealed class ModComparer : IComparer<Mod>
        {
            public int Compare(Mod x, Mod y)
            {
                // TODO: Make this not a partial order?
                // If x depends on a mod depending on y, it should count the same

                if (x.dependencies.ContainsKey(y.Id))
                    return 1;

                if (y.dependencies.ContainsKey(x.Id))
                    return -1;

                return 0;
            }
        }
    }
}