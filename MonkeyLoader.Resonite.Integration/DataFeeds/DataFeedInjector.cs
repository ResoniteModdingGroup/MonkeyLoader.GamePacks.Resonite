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
        public override int Priority => HarmonyLib.Priority.High;

        internal static AsyncParametrizedEnumerableBuilder<DataFeedItem, EnumerateDataFeedParameters<TDataFeed>> Builder { get; }

        static DataFeedInjector()
        {
            Builder = new();
            Builder.AddBuildingBlock(new OriginalDataFeedResultBuildingBlock<TDataFeed>());
            DataFeedInjectorManager.AddInjector<DataFeedInjector<TDataFeed>>();
        }

        protected override bool AppliesTo(FallbackLocaleGenerationEvent eventData) => true;

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => [];

        protected override Task Handle(FallbackLocaleGenerationEvent eventData)
        {
            eventData.AddMessage(this.GetLocaleKey("Name"), $"{typeof(TDataFeed).CompactDescription()}-Injector");
            eventData.AddMessage(this.GetLocaleKey("Description"), $"Handles injecting elements into the {typeof(TDataFeed).CompactDescription()}.");

            return Task.CompletedTask;
        }

        protected override bool OnEngineReady()
        {
            var enumerateMethod = TargetMethod();

            if (enumerateMethod is null)
                Logger.Error(() => $"Failed to find Enumerate(IReadOnlyList<string>, IReadOnlyList<string>, string) method declared on type [{typeof(TDataFeed).CompactDescription()}]");
            else
                Harmony.Patch(enumerateMethod, postfix: AccessTools.DeclaredMethod(GetType(), nameof(EnumeratePostfix)));

            return base.OnEngineReady();
        }

        private static IAsyncEnumerable<DataFeedItem> EnumeratePostfix(IAsyncEnumerable<DataFeedItem> __result, TDataFeed __instance, IReadOnlyList<string> path, IReadOnlyList<string> groupingKeys, string searchPhrase, object viewData)
        {
            try
            {
                var parameters = new EnumerateDataFeedParameters<TDataFeed>(__instance, __result, path, groupingKeys, searchPhrase, viewData);

                return Builder.GetEnumerable(parameters);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.LogFormat($"Failed to generate replacement for {typeof(TDataFeed).Name} Enumerate - using original result."));

                return __result;
            }
        }

        private static MethodBase TargetMethod()
            => AccessTools.DeclaredMethod(typeof(TDataFeed), nameof(IDataFeed.Enumerate), [typeof(IReadOnlyList<string>), typeof(IReadOnlyList<string>), typeof(string), typeof(object)]);
    }
}