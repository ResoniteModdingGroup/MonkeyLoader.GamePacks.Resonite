using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.DataFeeds
{
    internal abstract class DataFeedInjectorMonkey<TMonkey, TDataFeed> : ResoniteMonkey<TMonkey>,
            IAsyncEventSource<EnumerateDataFeedEvent<TDataFeed>>
        where TMonkey : DataFeedInjectorMonkey<TMonkey, TDataFeed>, new()
        where TDataFeed : IDataFeed
    {
        private static AsyncEventDispatching<EnumerateDataFeedEvent<TDataFeed>>? _dispatching;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(IDataFeed.Enumerate), [typeof(IReadOnlyList<string>), typeof(IReadOnlyList<string>), typeof(string)])]
        private static IAsyncEnumerable<DataFeedItem> EnumeratePostfix(IAsyncEnumerable<DataFeedItem> __result, TDataFeed __instance, IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, string searchPhrase)
        {
            var eventData = new EnumerateDataFeedEvent<TDataFeed>(__instance, __result, path, groupKeys, searchPhrase);

            _dispatching?.Invoke(eventData);

            return eventData.FinalResult;
        }

        event AsyncEventDispatching<EnumerateDataFeedEvent<TDataFeed>>? IAsyncEventSource<EnumerateDataFeedEvent<TDataFeed>>.Dispatching
        {
            add => _dispatching += value;
            remove => _dispatching -= value;
        }
    }
}