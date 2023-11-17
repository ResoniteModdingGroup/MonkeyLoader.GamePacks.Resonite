using MonkeyLoader.NuGet;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zio;
using Zio.FileSystems;

namespace MonkeyLoader.Meta
{
    public class DynamicMod : Mod, IModInternal
    {
        private readonly string? _title;

        /// <inheritdoc/>
        public override string ConfigPath { get; }

        /// <inheritdoc/>
        public string Description { get; }

        /// <inheritdoc/>
        public IFileSystem FileSystem { get; }

        /// <inheritdoc/>
        public UPath? IconPath { get; set; }

        /// <inheritdoc/>
        public Uri? IconUrl { get; set; }

        /// <inheritdoc/>
        public override PackageIdentity Identity { get; }

        /// <inheritdoc/>
        public Uri? ProjectUrl { get; set; }

        /// <inheritdoc/>
        public string? ReleaseNotes { get; set; }

        /// <inheritdoc/>
        public override NuGetFramework TargetFramework => NuGetHelper.Framework;

        /// <inheritdoc/>
        public override string Title => _title ?? base.Title;

        public NuGetVersion Version { get; }

        public DynamicMod(MonkeyLoader loader, string id, Version version, bool isGamePack, string description = "Dynamic Mod", string title = "Dynamic Mod", IFileSystem? fileSystem = null)
            : base(loader, isGamePack)
        {
            _title = title;
            Description = description;
            Identity = new PackageIdentity(id, new NuGetVersion(version));

            FileSystem = fileSystem ?? new MemoryFileSystem() { Name = $"{Title}'s FileSystem" };
            ConfigPath = Path.Combine(Loader.Locations.Configs, $"{Id}.json");
        }

        bool IModInternal.LoadEarlyMonkeys() => LoadEarlyMonkeys();

        bool IModInternal.LoadMonkeys() => LoadMonkeys();

        /// <inheritdoc/>
        protected override bool OnLoadEarlyMonkeys() => true;

        /// <inheritdoc/>
        protected override bool OnLoadMonkeys() => true;
    }
}