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
        public override bool CanBeDisabled { get; } = typeof(TDataFeed) != typeof(SettingsDataFeed);

        /// <inheritdoc/>
        public override string Id { get; } = typeof(TDataFeed).CompactDescription();

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
            eventData.AddMessage(this.GetLocaleKey("Description"), $"Handles injecting elements into the {typeof(TDataFeed).CompactDescription()}.<br/>Note that disabling this will block the effects of all other Monkeys that rely on this injector.");

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

        private static IAsyncEnumerable<DataFeedItem> EnumeratePostfix(IAsyncEnumerable<DataFeedItem> __result, TDataFeed __instance, IReadOnlyList<string> __0, IReadOnlyList<string> __1, string __2, object __3)
        {
            // I hate having to use the __n syntax for Arguments, but Froox can't decide whether to use groupKeys or groupingKeys
            // The method signature is based on an interface anyways, so the argument names can be whatever - better be save than sorry

            if (!Enabled)
                return __result;

            try
            {
                var parameters = new EnumerateDataFeedParameters<TDataFeed>(__instance, __result, __0, __1, __2, __3);

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