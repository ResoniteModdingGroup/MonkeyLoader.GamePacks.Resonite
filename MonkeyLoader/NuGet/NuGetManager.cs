using MonkeyLoader.Logging;
using NuGet.Frameworks;
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
        /// <summary>
        /// Gets the NuGet framework this assembly targets.
        /// </summary>
        public static NuGetFramework Framework { get; }

        /// <summary>
        /// Gets the name of the framework this assembly targets.
        /// </summary>
        public static string FrameworkName { get; }

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

        static NuGetManager()
        {
            var frameworkName = Assembly.GetExecutingAssembly().GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;

            Framework = frameworkName == null ? NuGetFramework.AnyFramework
                : NuGetFramework.Parse(frameworkName, new DefaultFrameworkNameProvider());

            FrameworkName = frameworkName ?? "Any";
        }

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
        }
    }
}