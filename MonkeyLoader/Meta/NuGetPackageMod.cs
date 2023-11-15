using MonkeyLoader.NuGet;
using MonkeyLoader.Patching;
using NuGet.Frameworks;
using NuGet.Packaging;
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
    public sealed class NuGetPackageMod : Mod
    {
        /// <summary>
        /// The search pattern for mod files.
        /// </summary>
        public const string SearchPattern = "*.nupkg";

        internal readonly HashSet<Assembly> PatcherAssemblies = new();
        internal readonly HashSet<Assembly> PrePatcherAssemblies = new();

        private const string AssemblyExtension = ".dll";
        private const string PrePatchersFolderName = "pre-patchers";
        private readonly UPath[] _assemblyPaths;

        /// <summary>
        /// Gets the absolute file path to this mod's file.
        /// </summary>
        public string Location { get; }

        /// <summary>
        /// Gets the paths inside this mod's <see cref="Mod.FileSystem">FileSystem</see> that point to patcher assemblies that should be loaded.
        /// </summary>
        public IEnumerable<UPath> PatcherAssemblyPaths => _assemblyPaths.Where(path => !path.FullName.Contains(PrePatchersFolderName));

        /// <summary>
        /// Gets the paths inside this mod's <see cref="Mod.FileSystem">FileSystem</see> that point to pre-patcher assemblies that should be loaded.
        /// </summary>
        public IEnumerable<UPath> PrePatcherAssemblyPaths => _assemblyPaths.Where(path => path.FullName.Contains(PrePatchersFolderName));

        private NuGetPackageMod(MonkeyLoader loader, PackageArchiveReader packageReader, NuspecReader nuspecReader, FrameworkSpecificGroup? nearestLib,
            string location, IFileSystem fileSystem, bool isGamePack,
            UPath? iconPath, Uri? iconUrl, Uri? projectUrl)
            : base(loader, nuspecReader.GetIdentity(), nearestLib?.TargetFramework ?? NuGetFramework.AnyFramework, isGamePack, fileSystem, nuspecReader.GetTitle(), nuspecReader.GetDescription(), nuspecReader.GetReleaseNotes(), iconPath, iconUrl, projectUrl)
        {
            Location = location;
            tags.AddRange(nuspecReader.GetTags().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            authors.AddRange(nuspecReader.GetAuthors().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(name => name.Trim()));

            if (nearestLib is null)
            {
                _assemblyPaths = Array.Empty<UPath>();
                Logger.Warn(() => $"No compatible lib entry found!");

                return;
            }

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

        /// <summary>
        /// Creates a new <see cref="NuGetPackageMod"/> instance for the given <paramref name="loader"/>, loading a .nupkg from the given <paramref name="location"/>.<br/>
        /// The metadata gets loaded from a <c>.nuspec</c> file, which must be at the root of the file system.
        /// </summary>
        /// <param name="loader">The loader instance that loaded this mod.</param>
        /// <param name="location">The absolute file path to the mod's file.</param>
        /// <param name="isGamePack">Whether this mod is a game pack.</param>
        public static NuGetPackageMod Load(MonkeyLoader loader, string location, bool isGamePack)
        {
            using var fileStream = File.OpenRead(location);
            var memoryStream = new MemoryStream((int)fileStream.Length);
            fileStream.CopyTo(memoryStream);

            var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read, true);
            var packageReader = new PackageArchiveReader(zipArchive, NuGetHelper.NameProvider, NuGetHelper.CompatibilityProvider);
            var fileSystem = new ZipArchiveFileSystem(zipArchive);

            var nuspecReader = packageReader.NuspecReader;

            var iconP = nuspecReader.GetIcon();
            UPath? iconPath = null;
            var iconPathWarn = false;
            if (fileSystem.FileExists(iconP))
                iconPath = new UPath(iconP).ToAbsolute();
            else if (!string.IsNullOrWhiteSpace(iconP))
                iconPathWarn = true;

            var iconU = nuspecReader.GetIconUrl();
            Uri? iconUrl = null;
            var iconUrlWarn = false;
            if (Uri.TryCreate(iconU, UriKind.Absolute, out var iconUri))
                iconUrl = iconUri;
            else if (!string.IsNullOrWhiteSpace(iconU))
                iconUrlWarn = true;

            var projectU = nuspecReader.GetProjectUrl();
            Uri? projectUrl = null;
            var projectUrlWarn = false;
            if (Uri.TryCreate(projectU, UriKind.Absolute, out var projectUri))
                projectUrl = projectUri;
            else if (!string.IsNullOrWhiteSpace(projectU))
                projectUrlWarn = true;

            var nearestLib = packageReader.GetLibItems().GetNearestCompatible();

            var mod = new NuGetPackageMod(loader, packageReader, nuspecReader, nearestLib, location, fileSystem, isGamePack, iconPath, iconUrl, projectUrl);

            if (iconPathWarn)
                mod.Logger.Warn(() => $"Icon Path [{iconP}] is set but the file doesn't exist for mod: {location}");

            if (iconUrlWarn)
                mod.Logger.Warn(() => $"Icon Url [{iconU}] is set but is invalid for mod: {location}");

            if (projectUrlWarn)
                mod.Logger.Warn(() => $"Project Url [{projectUrl}] is set but is invalid for mod: {location}");

            return mod;
        }

        /// <inheritdoc/>
        protected override bool OnLoadEarlyMonkeys()
        {
            var error = false;

            foreach (var prepatcherPath in PrePatcherAssemblyPaths)
            {
                try
                {
                    using var assemblyFile = FileSystem.OpenFile(prepatcherPath, FileMode.Open, FileAccess.Read);
                    using var memStream = new MemoryStream();
                    assemblyFile.CopyTo(memStream);

                    var assembly = Assembly.Load(memStream.ToArray());
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
                    using var memStream = new MemoryStream();
                    assemblyFile.CopyTo(memStream);

                    var assembly = Assembly.Load(memStream.ToArray());
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