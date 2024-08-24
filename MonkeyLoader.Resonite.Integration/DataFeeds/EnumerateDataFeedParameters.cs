using FrooxEngine;
using MonkeyLoader.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.DataFeeds
{
    /// <summary>
    /// Represents the event data for the Enumerate Data Feed Event.
    /// </summary>
    /// <remarks>
    /// This event is used by concrete <see cref="DataFeedInjector{TDataFeed}"/>
    /// implementations to signal that the <see cref="IDataFeed"/> they're patching is being enumerated.
    /// </remarks>
    /// <typeparam name="TDataFeed">The type of the data feed.</typeparam>
    public sealed class EnumerateDataFeedParameters<TDataFeed> : AsyncEvent
        where TDataFeed : IDataFeed
    {
        /// <summary>
        /// Gets the <see cref="IDataFeed"/> instance that the enumeration request was made on.
        /// </summary>
        public TDataFeed DataFeed { get; }

        /// <summary>
        /// Gets the group keys for this enumeration request. Can be empty.
        /// </summary>
        public IReadOnlyList<string> GroupKeys { get; }

        /// <summary>
        /// Gets whether this enumeration request has any <see cref="GroupKeys">GroupKeys</see>.
        /// </summary>
        /// <value>
        /// <c>true</c> when there's at least one group key; otherwise, <c>false</c>.
        /// </value>
        public bool HasGroupKeys => GroupKeys.Count > 0;

        /// <summary>
        /// Gets whether this enumeration request has a search phrase.
        /// </summary>
        /// <value>
        /// <c>true</c> if <see cref="SearchPhrase">SearchPhrase</see> is not <c>null</c> or empty; otherwise, <c>false</c>.
        /// </value>
        [MemberNotNullWhen(true, nameof(SearchPhrase))]
        public bool HasSearchPhrase => !string.IsNullOrEmpty(SearchPhrase);

        /// <summary>
        /// Gets or sets whether the original result should be inserted into the generated sequence.
        /// </summary>
        public bool IncludeOriginalResult { get; set; } = true;

        /// <summary>
        /// Gets whether this enumeration request is for the root path.
        /// </summary>
        /// <value>
        /// <c>true</c> if there's no <see cref="Path">Path</see> elements; otherwise <c>false</c>.
        /// </value>
        public bool IsRootPath => Path.Count == 0;

        /// <summary>
        /// Gets the path for this enumeration request.
        /// </summary>
        public IReadOnlyList<string> Path { get; }

        /// <summary>
        /// Gets the search phrase for this enumeration request.
        /// </summary>
        public string? SearchPhrase { get; }

        /// <summary>
        /// Gets the custom view data for this enumeration request.
        /// </summary>
        public object ViewData { get; }

        /// <summary>
        /// Gets the <see cref="IAsyncEnumerable{T}"/> that would have
        /// originally provided result of this enumeration request.
        /// </summary>
        internal IAsyncEnumerable<DataFeedItem> OriginalResult { get; }

        internal EnumerateDataFeedParameters(TDataFeed dataFeed, IAsyncEnumerable<DataFeedItem> originalResult,
            IReadOnlyList<string>? path, IReadOnlyList<string>? groupKeys, string? searchPhrase, object viewData)
        {
            DataFeed = dataFeed;
            OriginalResult = originalResult;
            Path = path ?? [];
            GroupKeys = groupKeys ?? [];
            SearchPhrase = searchPhrase;
            ViewData = viewData;
        }
    }
}