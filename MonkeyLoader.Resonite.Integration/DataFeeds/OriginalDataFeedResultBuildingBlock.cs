using EnumerableToolkit.Builder.AsyncBlocks;
using FrooxEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkeyLoader.Resonite.DataFeeds
{
    internal sealed class OriginalDataFeedResultBuildingBlock<TDataFeed> : IAsyncParametrizedBuildingBlock<DataFeedItem, EnumerateDataFeedParameters<TDataFeed>>
        where TDataFeed : IDataFeed
    {
        public IAsyncEnumerable<DataFeedItem> Apply(IAsyncEnumerable<DataFeedItem> current, EnumerateDataFeedParameters<TDataFeed> parameters)
        {
            if (!parameters.IncludeOriginalResult)
                return current;

            return current.Concat(parameters.OriginalResult);
        }
    }
}