using HarmonyLib;
using MonkeyLoader.Configuration;
using MonkeyLoader.NuGet;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using Zio;

namespace MonkeyLoader.Meta
{
    /// <summary>
    /// Contains all metadata and references for a mod.
    /// </summary>
    public interface IMod : IConfigOwner, ILoadedNuGetPackage, IShutdown, IComparable<IMod>
    {
        /// <summary>
        /// Gets the names of the authors of this mod.
        /// </summary>
        public IEnumerable<string> Authors { get; }

        /// <summary>
        /// Gets the config that this mod's (pre-)patcher(s) can use to load <see cref="ConfigSection"/>s.
        /// </summary>
        public Config Config { get; }

        /// <summary>
        /// Gets the description of this mod.
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Gets the available <see cref="IEarlyMonkey"/>s of this mod.
        /// </summary>
        public IEnumerable<IEarlyMonkey> EarlyMonkeys { get; }

        /// <summary>
        /// Gets the readonly file system of this mod's file.
        /// </summary>
        public abstract IFileSystem FileSystem { get; }

        /// <summary>
        /// Gets the <see cref="HarmonyLib.Harmony"/> instance to be used by this mod's (pre-)patcher(s).
        /// </summary>
        public Harmony Harmony { get; }

        /// <summary>
        /// Gets whether this mod has any <see cref="Monkeys">monkeys</see>.
        /// </summary>
        public bool HasPatchers { get; }

        /// <summary>
        /// Gets whether this mod has any <see cref="EarlyMonkeys">early monkeys</see>.
        /// </summary>
        public bool HasPrePatchers { get; }

        /// <summary>
        /// Gets the path to the mod's icon inside the mod's <see cref="FileSystem">FileSystem</see>.<br/>
        /// <c>null</c> if it wasn't given or doesn't exist.
        /// </summary>
        public abstract UPath? IconPath { get; }

        /// <summary>
        /// Gets the Url to the mod's icon on the web.<br/>
        /// <c>null</c> if it wasn't given or was invalid.
        /// </summary>
        public abstract Uri? IconUrl { get; }

        /// <summary>
        /// Gets the unique identifier of this mod.
        /// </summary>
        //public string Id { get; }
        /// <summary>
        /// Gets whether this mod is a game pack.
        /// </summary>
        public bool IsGamePack { get; }

        /// <summary>
        /// Gets whether this mod's <see cref="IModInternal.LoadEarlyMonkeys">LoadEarlyMonkeys</see>() failed when it was called.
        /// </summary>
        public bool LoadEarlyMonkeysFailed { get; }

        /// <summary>
        /// Gets whether this mod's <see cref="IModInternal.LoadEarlyMonkeys">LoadEarlyMonkeys</see>() method has been called.
        /// </summary>
        public bool LoadedEarlyMonkeys { get; }

        /// <summary>
        /// Gets whether this mod's <see cref="IModInternal.LoadMonkeys">LoadMonkeys</see>() method has been called.
        /// </summary>
        public bool LoadedMonkeys { get; }

        /// <summary>
        /// Gets whether this mod's <see cref="IModInternal.LoadEarlyMonkeys">LoadEarlyMonkeys</see>() or <see cref="IModInternal.LoadMonkeys">LoadMonkeys</see>() failed when they were called.
        /// </summary>
        public bool LoadFailed { get; }

        /// <summary>
        /// Gets whether this mod's <see cref="IModInternal.LoadMonkeys">LoadMonkeys</see>() failed when it was called.
        /// </summary>
        public bool LoadMonkeysFailed { get; }

        /// <summary>
        /// Gets the available <see cref="IMonkey"/>s of this mod.
        /// </summary>
        public IEnumerable<IMonkey> Monkeys { get; }

        /// <summary>
        /// Gets the Url to this mod's project website.<br/>
        /// <c>null</c> if it wasn't given or was invalid.
        /// </summary>
        public abstract Uri? ProjectUrl { get; }

        /// <summary>
        /// Gets the release notes for this mod's version.
        /// </summary>
        public abstract string? ReleaseNotes { get; }

        /// <summary>
        /// Gets the tags of this mod.
        /// </summary>
        public IEnumerable<string> Tags { get; }

        /// <summary>
        /// Gets the nice identifier of this mod.
        /// </summary>
        public abstract string Title { get; }

        /// <summary>
        /// Gets this mod's version.
        /// </summary>
        public NuGetVersion Version { get; }

        /// <summary>
        /// Efficiently checks, whether a given name is listed as an author for this mod.
        /// </summary>
        /// <param name="author">The name to check for.</param>
        /// <returns><c>true</c> if the given name is listed as an author for this mod.</returns>
        public bool HasAuthor(string author);

        /// <summary>
        /// Efficiently checks, whether a given tag is listed for this mod.
        /// </summary>
        /// <param name="tag">The tag to check for.</param>
        /// <returns><c>true</c> if the given tag is listed for this mod.</returns>
        public bool HasTag(string tag);
    }

    internal interface IModInternal : IMod
    {
        public bool LoadEarlyMonkeys();

        public bool LoadMonkeys();
    }
}