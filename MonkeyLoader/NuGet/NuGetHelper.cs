using NuGet.Frameworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MonkeyLoader.NuGet
{
    public static class NuGetHelper
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
        /// Gets the framework name provider to use.
        /// </summary>
        public static IFrameworkNameProvider NameProvider { get; } = DefaultFrameworkNameProvider.Instance;

        static NuGetHelper()
        {
            FrameworkName = GetTargetFramework();
            Framework = NuGetFramework.Parse(FrameworkName);

            _compatibleFrameworks = GetCompatibleFrameworks(Framework).ToArray();
        }

        public static T? GetNearestCompatible<T>(this IEnumerable<T> items, Func<T, NuGetFramework> selector) where T : class
            => NuGetFrameworkUtility.GetNearest(items, Framework, NameProvider, CompatibilityProvider, selector);

        public static T? GetNearestCompatible<T>(this IEnumerable<T> items) where T : class, IFrameworkSpecific
            => items.GetNearestCompatible(items => items.TargetFramework);

        private static IEnumerable<NuGetFramework> GetCompatibleFrameworks(NuGetFramework target)
        {
            var _reducer = new FrameworkReducer(NameProvider, CompatibilityProvider);

            var remaining = NameProvider
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