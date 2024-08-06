using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Events;
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
    internal sealed class DataFeedInjectorMonkey<TDataFeed> : ResoniteAsyncEventHandlerMonkey<DataFeedInjectorMonkey<TDataFeed>, FallbackLocaleGenerationEvent>,
            IAsyncEventSource<EnumerateDataFeedEvent<TDataFeed>>
        where TDataFeed : IDataFeed
    {
        private static AsyncEventDispatching<EnumerateDataFeedEvent<TDataFeed>>? _dispatching;

        /// <inheritdoc/>
        public override bool CanBeDisabled => true;

        /// <inheritdoc/>
        public override string Id { get; } = typeof(DataFeedInjectorMonkey<TDataFeed>).CompactDescription();

        /// <inheritdoc/>
        public override int Priority => HarmonyLib.Priority.Normal;

        protected override bool AppliesTo(FallbackLocaleGenerationEvent eventData) => !Failed && Enabled;

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => [];

        protected override Task Handle(FallbackLocaleGenerationEvent eventData)
        {
            eventData.AddMessage(this.GetLocaleKey("Name"), $"{typeof(TDataFeed).CompactDescription()}-Injector");
            eventData.AddMessage(this.GetLocaleKey("Description"), $"Sends out the {typeof(EnumerateDataFeedEvent<TDataFeed>).CompactDescription()} event for monkeys to manipulate the items returned.");

            return Task.CompletedTask;
        }

        protected override bool OnEngineReady()
        {
            if (!Prepare())
            {
                Logger.Error(() => $"Failed to find Enumerate(IReadOnlyList<string>, IReadOnlyList<string>, string) method declared on type [{typeof(TDataFeed).CompactDescription()}]");

                return false;
            }

            Mod.RegisterEventSource(this);

            return base.OnEngineReady();
        }

        [HarmonyPrefix]
        private static IAsyncEnumerable<DataFeedItem> EnumeratePostfix(IAsyncEnumerable<DataFeedItem> __result, TDataFeed __instance, IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, string searchPhrase, object viewData)
        {
            var eventData = new EnumerateDataFeedEvent<TDataFeed>(__instance, __result, path, groupKeys, searchPhrase, viewData);

            _dispatching?.Invoke(eventData);

            return eventData.FinalResult;
        }

        private static bool Prepare() => TargetMethod() is not null;

        private static MethodBase TargetMethod()
            => AccessTools.DeclaredMethod(typeof(TDataFeed), nameof(IDataFeed.Enumerate), [typeof(IReadOnlyList<string>), typeof(IReadOnlyList<string>), typeof(string), typeof(object)]);

        event AsyncEventDispatching<EnumerateDataFeedEvent<TDataFeed>>? IAsyncEventSource<EnumerateDataFeedEvent<TDataFeed>>.Dispatching
        {
            add => _dispatching += value;
            remove => _dispatching -= value;
        }
    }
}