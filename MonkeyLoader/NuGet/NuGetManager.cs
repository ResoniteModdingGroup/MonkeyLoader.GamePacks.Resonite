using MonkeyLoader.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.NuGet
{
    /// <summary>
    /// Handles accessing NuGet feeds and loading dependencies.
    /// </summary>
    public sealed class NuGetManager
    {
        private readonly Dictionary<string, ILoadedNuGetPackage> _loadedPackages = new(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Gets the config used by the manager.
        /// </summary>
        public NuGetConfigSection Config { get; }

        /// <summary>
        /// Gets the loader this NuGet manager works for.
        /// </summary>
        public MonkeyLoader Loader { get; }

        /// <summary>
        /// Gets the logger used by the manager.
        /// </summary>
        public MonkeyLogger Logger { get; }

        /// <summary>
        /// Creates a new NuGet manager instance that works for the given loader.<br/>
        /// Requires <see cref="MonkeyLoader.Logger"/> and <see cref="MonkeyLoader.Config"/> to be set.
        /// </summary>
        /// <param name="loader">The loader this NuGet manager works for.</param>
        internal NuGetManager(MonkeyLoader loader)
        {
            Loader = loader;
            Logger = new MonkeyLogger(loader.Logger, "NuGet");
            Config = loader.Config.LoadSection<NuGetConfigSection>();

            Logger.Info(() => $"Detected Runtime Target NuGet Framework: {NuGetHelper.Framework} ({NuGetHelper.Framework.GetShortFolderName()})");
            Logger.Debug(() => $"Compatible NuGet Frameworks:{Environment.NewLine}" +
                $"    - {string.Join($"{Environment.NewLine}    - ", NuGetHelper.CompatibleFrameworks.Select(fw => $"{fw} ({fw.GetShortFolderName()})"))}");
        }

        public void Add(ILoadedNuGetPackage package)
        {
            _loadedPackages.Add(package.Identity.Id, package);

            Logger.Trace(() => $"Added loaded package [{package.Identity}]");
        }

        public void AddAll(IEnumerable<ILoadedNuGetPackage> packages)
        {
            foreach (var package in packages)
                Add(package);
        }
    }
}