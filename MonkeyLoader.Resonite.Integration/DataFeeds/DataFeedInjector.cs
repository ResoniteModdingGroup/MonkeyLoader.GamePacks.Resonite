using EnumerableToolkit.Builder;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Patching;
using MonkeyLoader.Resonite.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.DataFeeds
{
    internal sealed class DataFeedInjector<TDataFeed> : ResoniteAsyncEventHandlerMonkey<DataFeedInjector<TDataFeed>, FallbackLocaleGenerationEvent>
        where TDataFeed : IDataFeed
    {
        /// <inheritdoc/>
        public override bool CanBeDisabled => true;

        /// <inheritdoc/>
        public override string Id { get; } = typeof(DataFeedInjector<TDataFeed>).CompactDescription();

        /// <inheritdoc/>
        public override int Priority => HarmonyLib.Priority.Normal;

        internal static AsyncParametrizedEnumerableBuilder<DataFeedItem, EnumerateDataFeedParameters<TDataFeed>> Builder { get; } = new();

        static DataFeedInjector()
        {
            Builder.AddBuildingBlock(new OriginalDataFeedResultBuildingBlock<TDataFeed>());
            DataFeedInjectorManager.AddInjector<DataFeedInjector<TDataFeed>>();
        }

        protected override bool AppliesTo(FallbackLocaleGenerationEvent eventData) => true;

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => [];

        protected override Task Handle(FallbackLocaleGenerationEvent eventData)
        {
            eventData.AddMessage(this.GetLocaleKey("Name"), $"{typeof(TDataFeed).CompactDescription()}-Injector");
            eventData.AddMessage(this.GetLocaleKey("Description"), $"Sends out the {typeof(EnumerateDataFeedParameters<TDataFeed>).CompactDescription()} event for monkeys to manipulate the items returned.");

            return Task.CompletedTask;
        }

        protected override bool OnEngineReady()
        {
            if (!Prepare())
            {
                Logger.Error(() => $"Failed to find Enumerate(IReadOnlyList<string>, IReadOnlyList<string>, string) method declared on type [{typeof(TDataFeed).CompactDescription()}]");

                return false;
            }

            return base.OnEngineReady();
        }

        [HarmonyPostfix]
        private static IAsyncEnumerable<DataFeedItem> EnumeratePostfix(IAsyncEnumerable<DataFeedItem> __result, TDataFeed __instance, IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, string searchPhrase, object viewData)
        {
            var parameters = new EnumerateDataFeedParameters<TDataFeed>(__instance, __result, path, groupKeys, searchPhrase, viewData);

            return Builder.GetEnumerable(parameters);
        }

        private static bool Prepare() => TargetMethod() is not null;

        private static MethodBase TargetMethod()
            => AccessTools.DeclaredMethod(typeof(TDataFeed), nameof(IDataFeed.Enumerate), [typeof(IReadOnlyList<string>), typeof(IReadOnlyList<string>), typeof(string), typeof(object)]);
    }
}