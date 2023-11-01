using MonkeyLoader.Logging;
using NuGet.Frameworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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
        private static readonly NuGetFramework[] _compatibleFrameworks;

        /// <summary>
        /// Gets the framework compatibility provider to use.
        /// </summary>
        public static IFrameworkCompatibilityProvider CompatibilityProvider { get; } = DefaultCompatibilityProvider.Instance;

        /// <summary>
        /// Gets the short folder names of the <see cref="NuGetFramework"/>s
        /// compatible with this <see cref="Framework">AppDomain's framework</see>.
        /// </summary>
        public static IEnumerable<string> CompatibleFrameworkFolders
            => _compatibleFrameworks.Select(framework => framework.GetShortFolderName());

        /// <summary>
        /// Gets the <see cref="NuGetFramework"/>s compatible with
        /// this <see cref="Framework">AppDomain's framework</see>.
        /// </summary>
        public static IEnumerable<NuGetFramework> CompatibleFrameworks
            => _compatibleFrameworks.AsSafeEnumerable();

        /// <summary>
        /// Gets the NuGet framework of the current AppDomain's framework target.
        /// </summary>
        public static NuGetFramework Framework { get; }

        /// <summary>
        /// Gets the framework target of the current AppDomain.
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
            FrameworkName = GetTargetFramework();
            Framework = NuGetFramework.Parse(FrameworkName);

            _compatibleFrameworks = GetCompatibleFrameworks(Framework).ToArray();
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

            Logger.Trace(() => $"Detected NuGet Framework: {Framework} ({Framework.GetShortFolderName()})");
            Logger.Trace(() => $"Compatible NuGet Frameworks:{Environment.NewLine}" +
                $"    - {string.Join($"{Environment.NewLine}    - ", CompatibleFrameworks.Select(fw => $"{fw} ({fw.GetShortFolderName()})"))}");
        }

        private static IEnumerable<NuGetFramework> GetCompatibleFrameworks(NuGetFramework target)
        {
            var nameProvider = DefaultFrameworkNameProvider.Instance;
            var _reducer = new FrameworkReducer(nameProvider, CompatibilityProvider);

            var remaining = nameProvider
                .GetCompatibleCandidates()
                .Where(candidate => CompatibilityProvider.IsCompatible(target, candidate));

            //remaining = _reducer.ReduceEquivalent(remaining);

            return remaining.OrderBy(f => f, new NuGetFrameworkSorter());
        }

        private static string GetTargetFramework()
        {
            // https://github.com/mono/mono/issues/14141
            // This works in regular .NET, but not on Mono - only been an open issue since 2019
            if (!string.IsNullOrWhiteSpace(AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName))
                return AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName;

            // For example: ".NET Framework 4.6.57.0"
            var descSplit = RuntimeInformation.FrameworkDescription?.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (descSplit is not null && descSplit.Length >= 3)
                return $"{descSplit[0]}{descSplit[1]},Version=v{descSplit[2]}";

            throw new InvalidOperationException("No framework version found.");
        }
    }
}