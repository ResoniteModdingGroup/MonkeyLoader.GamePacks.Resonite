using MonkeyLoader.NuGet;
using MonkeyLoader.Patching;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Zio;
using Zio.FileSystems;

namespace MonkeyLoader.Meta
{
    /// <summary>
    /// Contains all the metadata and references to loaded patchers from a .nupkg mod file.
    /// </summary>
    public sealed class NuGetPackageMod : Mod, IModInternal
    {
        /// <summary>
        /// The search pattern for mod files.
        /// </summary>
        public const string SearchPattern = "*.nupkg";

        internal readonly HashSet<Assembly> PatcherAssemblies = new();
        internal readonly HashSet<Assembly> PrePatcherAssemblies = new();

        private const string AssemblyExtension = ".dll";
        private const char AuthorsSeparator = ',';
        private const string PrePatchersFolderName = "pre-patchers";
        private const char TagsSeparator = ' ';
        private readonly UPath[] _assemblyPaths;

        private readonly string? _title;

        /// <inheritdoc/>
        public override string ConfigPath { get; }

        /// <inheritdoc/>
        public string Description { get; }

        /// <inheritdoc/>
        public IFileSystem FileSystem { get; }

        /// <inheritdoc/>
        public UPath? IconPath { get; }

        /// <inheritdoc/>
        public Uri? IconUrl { get; }

        /// <inheritdoc/>
        public override PackageIdentity Identity { get; }

        /// <summary>
        /// Gets the absolute file path to this mod's file.
        /// </summary>
        public string Location { get; }

        /// <summary>
        /// Gets the paths inside this mod's <see cref="FileSystem">FileSystem</see> that point to patcher assemblies that should be loaded.
        /// </summary>
        public IEnumerable<UPath> PatcherAssemblyPaths => _assemblyPaths.Where(path => !path.FullName.Contains(PrePatchersFolderName));

        /// <summary>
        /// Gets the paths inside this mod's <see cref="FileSystem">FileSystem</see> that point to pre-patcher assemblies that should be loaded.
        /// </summary>
        public IEnumerable<UPath> PrePatcherAssemblyPaths => _assemblyPaths.Where(path => path.FullName.Contains(PrePatchersFolderName));

        /// <inheritdoc/>
        public Uri? ProjectUrl { get; }

        /// <inheritdoc/>
        public string? ReleaseNotes { get; }

        /// <inheritdoc/>
        public override NuGetFramework TargetFramework { get; }

        /// <inheritdoc/>
        public override string Title => _title ?? base.Title;

        /// <inheritdoc/>
        public NuGetVersion Version => Identity.Version;

        /// <summary>
        /// Creates a new <see cref="NuGetPackageMod"/> instance for the given <paramref name="loader"/>, loading a .nupkg from the given <paramref name="location"/>.<br/>
        /// The metadata gets loaded from a <c>.nuspec</c> file, which must be at the root of the file system.
        /// </summary>
        /// <param name="loader">The loader instance that loaded this mod.</param>
        /// <param name="location">The absolute file path to the mod's file.</param>
        /// <param name="isGamePack">Whether this mod is a game pack.</param>
        public NuGetPackageMod(MonkeyLoader loader, string location, bool isGamePack) : base(loader, isGamePack)
        {
            Location = location;

            using var fileStream = File.OpenRead(location);
            var memoryStream = new MemoryStream((int)fileStream.Length);
            fileStream.CopyTo(memoryStream);

            var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read, true);
            var packageReader = new PackageArchiveReader(zipArchive, NuGetHelper.NameProvider, NuGetHelper.CompatibilityProvider);
            FileSystem = new ZipArchiveFileSystem(zipArchive);

            var nuspecReader = packageReader.NuspecReader;

            var title = nuspecReader.GetTitle();
            _title = string.IsNullOrWhiteSpace(title) ? null : title;

            Identity = nuspecReader.GetIdentity();
            ConfigPath = Path.Combine(Loader.Locations.Configs, $"{Id}.json");

            Description = nuspecReader.GetDescription();
            ReleaseNotes = nuspecReader.GetReleaseNotes();

            tags.AddRange(nuspecReader.GetTags().Split(new[] { TagsSeparator }, StringSplitOptions.RemoveEmptyEntries));
            authors.AddRange(nuspecReader.GetAuthors().Split(new[] { AuthorsSeparator }, StringSplitOptions.RemoveEmptyEntries).Select(name => name.Trim()));

            var iconPath = nuspecReader.GetIcon();
            if (FileSystem.FileExists(iconPath))
                IconPath = new UPath(iconPath).ToAbsolute();
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

            var nearestLib = packageReader.GetLibItems().GetNearestCompatible();
            if (nearestLib is null)
            {
                _assemblyPaths = Array.Empty<UPath>();
                TargetFramework = NuGetFramework.AnyFramework;
                Logger.Warn(() => $"No compatible lib entry found!");

                return;
            }

            TargetFramework = nearestLib.TargetFramework;
            Logger.Debug(() => $"Nearest compatible lib entry: {nearestLib.TargetFramework}");

            _assemblyPaths = nearestLib.Items
                .Where(path => AssemblyExtension.Equals(Path.GetExtension(path), StringComparison.OrdinalIgnoreCase))
                .Select(path => new UPath(path).ToAbsolute()).ToArray() ?? Array.Empty<UPath>();

            if (_assemblyPaths.Any())
                Logger.Trace(() => $"Found the following assemblies:{Environment.NewLine}    - {string.Join($"{Environment.NewLine}    - ", _assemblyPaths)}");
            else
                Logger.Warn(() => "Found no assemblies!");

            var deps = packageReader.GetPackageDependencies()
                .SingleOrDefault(group => TargetFramework.Equals(group.TargetFramework))
                ?? packageReader.GetPackageDependencies().SingleOrDefault(group => NuGetFramework.AnyFramework.Equals(group.TargetFramework));

            if (deps is null)
                return;

            foreach (var package in deps.Packages)
                dependencies.Add(package.Id, new DependencyReference(loader.NuGet, package));
        }

        bool IModInternal.LoadEarlyMonkeys() => LoadEarlyMonkeys();

        bool IModInternal.LoadMonkeys() => LoadMonkeys();

        /// <inheritdoc/>
        protected override bool OnLoadEarlyMonkeys()
        {
            var error = false;

            foreach (var prepatcherPath in PrePatcherAssemblyPaths)
            {
                try
                {
                    using var assemblyFile = FileSystem.OpenFile(prepatcherPath, FileMode.Open, FileAccess.Read);
                    using var assemblyStream = new MemoryStream();
                    assemblyFile.CopyTo(assemblyStream);

                    var mdbPath = prepatcherPath + ".mdb";
                    var pdbPath = prepatcherPath.GetDirectory() / prepatcherPath.GetNameWithoutExtension()! / ".pdb";
                    using var symbolStream = new MemoryStream();

                    if (FileSystem.FileExists(mdbPath))
                    {
                        using var mdbFile = FileSystem.OpenFile(mdbPath, FileMode.Open, FileAccess.Read);
                        mdbFile.CopyTo(symbolStream);
                    }
                    else if (FileSystem.FileExists(pdbPath))
                    {
                        using var pdbFile = FileSystem.OpenFile(pdbPath, FileMode.Open, FileAccess.Read);
                        pdbFile.CopyTo(symbolStream);
                    }

                    var assembly = Assembly.Load(assemblyStream.ToArray(), symbolStream.ToArray());
                    Loader.AddJsonConverters(assembly);
                    PrePatcherAssemblies.Add(assembly);

                    foreach (var type in assembly.GetTypes().Instantiable<IEarlyMonkey>())
                    {
                        Logger.Debug(() => $"Found instantiable EarlyMonkey Type: {type.FullName}");
                        var monkey = MonkeyBase.GetInstance(type);
                        monkey.Mod = this;
                        earlyMonkeys.Add((IEarlyMonkey)monkey);
                    }

                    Logger.Info(() => $"Found {earlyMonkeys.Count} Early Monkeys!");
                }
                catch (Exception ex)
                {
                    error = true;
                    Logger.Error(() => ex.Format($"Error while loading Early Monkeys from assembly: {prepatcherPath}!"));
                }
            }

            return !error;
        }

        /// <inheritdoc/>
        protected override bool OnLoadMonkeys()
        {
            // assemblies should be Mono.Cecil loaded before the Early ones, to allow pre-patchers access

            var error = false;

            foreach (var patcherPath in PatcherAssemblyPaths)
            {
                try
                {
                    Logger.Debug(() => $"Loading patcher assembly from: {patcherPath}");

                    using var assemblyFile = FileSystem.OpenFile(patcherPath, FileMode.Open, FileAccess.Read);
                    using var assemblyStream = new MemoryStream();
                    assemblyFile.CopyTo(assemblyStream);

                    var mdbPath = patcherPath + ".mdb";
                    var pdbPath = patcherPath.GetDirectory() / patcherPath.GetNameWithoutExtension()! / ".pdb";
                    using var symbolStream = new MemoryStream();

                    if (FileSystem.FileExists(mdbPath))
                    {
                        using var mdbFile = FileSystem.OpenFile(mdbPath, FileMode.Open, FileAccess.Read);
                        mdbFile.CopyTo(symbolStream);
                    }
                    else if (FileSystem.FileExists(pdbPath))
                    {
                        using var pdbFile = FileSystem.OpenFile(pdbPath, FileMode.Open, FileAccess.Read);
                        pdbFile.CopyTo(symbolStream);
                    }

                    var assembly = Assembly.Load(assemblyStream.ToArray(), symbolStream.ToArray());
                    Loader.AddJsonConverters(assembly);
                    PatcherAssemblies.Add(assembly);

                    Logger.Info(() => $"Loaded patcher assembly: {assembly.FullName}");

                    foreach (var type in assembly.GetTypes().Instantiable<MonkeyBase>())
                    {
                        Logger.Debug(() => $"Found instantiable Monkey Type: {type.FullName}");
                        var monkey = MonkeyBase.GetInstance(type);
                        monkey.Mod = this;
                        monkeys.Add(monkey);
                    }

                    Logger.Info(() => $"Found {monkeys.Count} Monkeys!");
                }
                catch (Exception ex)
                {
                    error = true;
                    Logger.Error(() => ex.Format($"Error while loading Monkeys from assembly: {patcherPath}!"));
                }
            }

            return !error;
        }
    }
}