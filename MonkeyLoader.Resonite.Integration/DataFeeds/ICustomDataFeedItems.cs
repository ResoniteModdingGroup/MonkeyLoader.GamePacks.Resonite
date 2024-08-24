using FrooxEngine;
using System;
using System.Collections.Generic;

namespace MonkeyLoader.Resonite.DataFeeds
{
    /// <summary>
    /// Defines the interface for objects that can generate <see cref="DataFeedItem"/>s
    /// to represent themselves, rather than requiring default handling.
    /// </summary>
    public interface ICustomDataFeedItems
    {
        /// <summary>
        /// Enumerates the <see cref="DataFeedItem"/>s that represent this object.
        /// </summary>
        /// <param name="path">The path for this enumeration request.</param>
        /// <param name="groupKeys">The group keys for this enumeration request.</param>
        /// <param name="searchPhrase">The search phrase for this enumeration request.</param>
        /// <param name="viewData">The custom view data for this enumeration request.</param>
        /// <returns>An async sequence of <see cref="DataFeedItem"/>s that represents this object.</returns>
        public IAsyncEnumerable<DataFeedItem> Enumerate(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, string? searchPhrase, object? viewData);
    }
}