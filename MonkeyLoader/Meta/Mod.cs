using HarmonyLib;
using MonkeyLoader.Configuration;
using MonkeyLoader.Logging;
using MonkeyLoader.Patching;
using MonkeyLoader.Prepatching;
using NuGet.Packaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zio;

namespace MonkeyLoader.Meta
{
    /// <summary>
    /// Contains all the metadata and references to loaded patchers from a mod file.
    /// </summary>
    public sealed class Mod : IConfigOwner
    {
        private readonly HashSet<string> authors;
        private readonly List<EarlyMonkey> earlyMonkeys = new();
        private readonly List<Monkey> monkeys = new();
        private readonly HashSet<string> tags;

        /// <summary>
        /// Gets the names of the authors of this mod.
        /// </summary>
        public IEnumerable<string> Authors
        {
            get
            {
                foreach (var author in Authors)
                    yield return author;
            }
        }

        /// <summary>
        /// Gets the config that this mod's (pre-)patcher(s) can use to load <see cref="ConfigSection"/>s.
        /// </summary>
        public Config Config { get; }

        /// <summary>
        /// Gets the path where this mod's config file should be.
        /// </summary>
        public string ConfigPath { get; }

        /// <summary>
        /// Gets the description of this mod.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the available <see cref="EarlyMonkey"/>s of this mod.
        /// </summary>
        public IEnumerable<EarlyMonkey> EarlyMonkeys
        {
            get
            {
                foreach (var earlyMonkey in earlyMonkeys)
                    yield return earlyMonkey;
            }
        }

        /// <summary>
        /// Gets the readonly file system of this mod's file.
        /// </summary>
        public IFileSystem FileSystem { get; }

        /// <summary>
        /// Gets the <see cref="HarmonyLib.Harmony"/> instance to be used by this mod's (pre-)patcher(s).
        /// </summary>
        public Harmony Harmony { get; }

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
        public string Id { get; }

        /// <summary>
        /// Gets the <see cref="MonkeyLoader"/> instance that loaded this mod.
        /// </summary>
        public MonkeyLoader Loader { get; }

        /// <summary>
        /// Gets the absolute file path to this mod's file.
        /// </summary>
        public string Location { get; }

        /// <summary>
        /// Gets the logger to be used by this mod.
        /// </summary>
        /// <remarks>
        /// Every mod instance has its own logger and can thus have a different <see cref="LoggingLevel"/>.<br/>
        /// They do all share the <see cref="Loader">Loader's</see> <see cref="MonkeyLoader.LoggingHandler">LoggingHandler</see> though.
        /// </remarks>
        public MonkeyLogger Logger { get; }

        /// <summary>
        /// Gets the available <see cref="Monkey"/>s of this mod.
        /// </summary>
        public IEnumerable<Monkey> Monkeys
        {
            get
            {
                foreach (var monkey in monkeys)
                    yield return monkey;
            }
        }

        /// <summary>
        /// Gets the Url to this mod's project website.<br/>
        /// <c>null</c> if it wasn't given or was invalid.
        /// </summary>
        public Uri? ProjectUrl { get; }

        /// <summary>
        /// Gets the release notes for this mod's version.
        /// </summary>
        public string ReleaseNotes { get; }

        /// <summary>
        /// Gets the tags of this mod.
        /// </summary>
        public IEnumerable<string> Tags
        {
            get
            {
                foreach (var tag in tags)
                    yield return tag;
            }
        }

        /// <summary>
        /// Gets the nice identifier of this mod.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets this mod's version.
        /// </summary>
        public Version Version { get; }

        /// <summary>
        /// Creates a new <see cref="Mod"/> instance with the given <paramref name="monkeyLoader"/> and <paramref name="fileSystem"/> for the <paramref name="location"/>.<br/>
        /// The metadata gets loaded from a <c>.nuspec</c> file, which must be at the root of the file system.
        /// </summary>
        /// <param name="monkeyLoader">The loader instance that loaded this mod.</param>
        /// <param name="location">The absolute file path to the mod's file.</param>
        /// <param name="fileSystem">The file system of the mod's file.</param>
        /// <exception cref="FileNotFoundException">When there's no <c>.nuspec</c> file at the root of the file system.</exception>
        public Mod(MonkeyLoader monkeyLoader, string location, IFileSystem fileSystem)
        {
            if (!(fileSystem.EnumerateFiles("", "*.nuspec").SingleOrDefault() is UPath nuspecPath))
                throw new FileNotFoundException("Couldn't find required .nuspec file at the root of the mod's file system.", location);

            using var nuspecStream = fileSystem.OpenFile(nuspecPath, FileMode.Open, FileAccess.Read);
            var nuspecReader = new NuspecReader(nuspecStream);

            Loader = monkeyLoader;
            Id = nuspecReader.GetId();
            Harmony = new Harmony(Id);
            Logger = new MonkeyLogger(monkeyLoader.Logger, Id);

            Location = location;
            FileSystem = fileSystem;

            Title = nuspecReader.GetTitle();
            Version = nuspecReader.GetVersion().Version;
            Description = nuspecReader.GetDescription();
            ReleaseNotes = nuspecReader.GetReleaseNotes();

            var iconPath = nuspecReader.GetIcon();
            if (fileSystem.FileExists(iconPath))
                IconPath = iconPath;
            else if (!string.IsNullOrWhiteSpace(iconPath))
                Logger.Warn(() => $"Icon Path [{iconPath}] is set but the file doesn't exist for mod: {location}");

            var iconUrl = nuspecReader.GetIconUrl();
            if (Uri.TryCreate(iconUrl, UriKind.Absolute, out var iconUri))
                IconUrl = iconUri;
            else if (!string.IsNullOrWhiteSpace(iconUrl))
                Logger.Warn(() => $"Icon Url [{iconUrl}] is set but is invalid for mod: {location}");

            var projectUrl = nuspecReader.GetProjectUrl();
            if (Uri.TryCreate(projectUrl, UriKind.Absolute, out var projectUri))
                ProjectUrl = projectUri;
            else if (!string.IsNullOrWhiteSpace(projectUrl))
                Logger.Warn(() => $"Project Url [{projectUrl}] is set but is invalid for mod: {location}");

            tags = new(nuspecReader.GetTags().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            authors = new(nuspecReader.GetAuthors().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(name => name.Trim()));

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
    }
}