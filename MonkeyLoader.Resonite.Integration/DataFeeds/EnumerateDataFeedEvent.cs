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
    /// This event is used by concrete <see cref="DataFeedInjectorMonkey{TMonkey, TDataFeed}"/>
    /// implementations to signal that the <see cref="IDataFeed"/> they're patching is being enumerated.
    /// </remarks>
    /// <typeparam name="TDataFeed">The type of the data feed.</typeparam>
    public sealed class EnumerateDataFeedEvent<TDataFeed> : AsyncEvent
        where TDataFeed : IDataFeed
    {
        /// <summary>
        /// Gets the <see cref="DataFeedItem"/>s which have currently been
        /// <see cref="AppendResult">appended</see> or <see cref="ConcatResult">concatenated</see>
        /// to this enumeration request and will be concatenated to the base result for the <see cref="FinalResult">FinalResult</see>.
        /// </summary>
        public IAsyncEnumerable<DataFeedItem> AddedItems { get; private set; } = AsyncEnumerable.Empty<DataFeedItem>();

        /// <summary>
        /// Gets the <see cref="IDataFeed"/> instance that the enumeration request was made on.
        /// </summary>
        public TDataFeed DataFeed { get; }

        /// <summary>
        /// Gets the <see cref="IAsyncEnumerable{T}"/> that will
        /// currently provide the final result of this enumeration request.
        /// </summary>
        public IAsyncEnumerable<DataFeedItem> FinalResult
            => (ReplacementResult ?? OriginalResult).Concat(AddedItems);

        /// <summary>
        /// Gets the group keys for this enumeration request. Can be empty.
        /// </summary>
        public IReadOnlyList<string> GroupKeys { get; }

        /// <summary>
        /// Gets whether this enumeration request's base result will have
        /// any <see cref="AddedItems">items</see> concatenated to it.
        /// </summary>
        public bool HasAddedItems { get; private set; }

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
        /// Gets whether this enumeration request is for the root path.
        /// </summary>
        /// <value>
        /// <c>true</c> if there's no <see cref="Path">Path</see> elements; otherwise <c>false</c>.
        /// </value>
        public bool IsRootPath => Path.Count == 0;

        /// <summary>
        /// Gets the <see cref="IAsyncEnumerable{T}"/> that would have
        /// originally provided result of this enumeration request.
        /// </summary>
        public IAsyncEnumerable<DataFeedItem> OriginalResult { get; }

        /// <summary>
        /// Gets the path for this enumeration request.
        /// </summary>
        public IReadOnlyList<string> Path { get; }

        /// <summary>
        /// Gets the <see cref="IAsyncEnumerable{T}"/> that will currently replace the
        /// <see cref="OriginalResult">original Result</see> of this enumeration request.
        /// </summary>
        public IAsyncEnumerable<DataFeedItem>? ReplacementResult { get; private set; }

        /// <summary>
        /// Gets whether this enumeration request's <see cref="OriginalResult">original Result</see> will be replaced.
        /// </summary>
        /// <value>
        /// <c>true</c> if <see cref="ReplacementResult">ReplacementResult</see> is not <c>null</c>; otherwise, <c>false</c>.
        /// </value>
        [MemberNotNullWhen(true, nameof(ReplacementResult))]
        public bool ReplacesOriginalResult => ReplacementResult is not null;

        /// <summary>
        /// Gets the search phrase for this enumeration request.
        /// </summary>
        public string? SearchPhrase { get; }

        internal EnumerateDataFeedEvent(TDataFeed dataFeed, IAsyncEnumerable<DataFeedItem> result,
            IReadOnlyList<string>? path, IReadOnlyList<string>? groupKeys, string? searchPhrase)
        {
            DataFeed = dataFeed;
            OriginalResult = result;
            Path = path ?? Array.Empty<string>();
            GroupKeys = groupKeys ?? Array.Empty<string>();
            SearchPhrase = searchPhrase;
        }

        /// <summary>
        /// Appends the given <see cref="DataFeedItem"/> to the current
        /// <see cref="FinalResult">Result</see> of this enumeration request.
        /// </summary>
        /// <param name="item">The item to append.</param>
        public void AppendResult(DataFeedItem item)
        {
            AddedItems = AddedItems.Append(item);
            HasAddedItems = true;
        }

        /// <summary>
        /// Clears any previously <see cref="AddedItems">added items</see>.
        /// </summary>
        public void ClearAddedItems()
        {
            AddedItems = AsyncEnumerable.Empty<DataFeedItem>();
            HasAddedItems = false;
        }

        /// <summary>
        /// Clears a previously set <see cref="ReplacementResult">ReplacementResult</see>.
        /// </summary>
        public void ClearReplacementResult() => ReplacementResult = null;

        /// <summary>
        /// Concatenates the given <see cref="DataFeedItem"/>s to the current
        /// <see cref="FinalResult">Result</see> of this enumeration request.
        /// </summary>
        /// <param name="secondResult">The items to concatenate.</param>
        public void ConcatResult(IAsyncEnumerable<DataFeedItem> secondResult)
        {
            AddedItems = AddedItems.Concat(secondResult);
            HasAddedItems = true;
        }

        /// <summary>
        /// Replaces the base result of this enumeration request with the given <see cref="DataFeedItem"/>s.
        /// </summary>
        /// <param name="newResult">The items replacing the base result.</param>
        public void ReplaceResult(IAsyncEnumerable<DataFeedItem> newResult)
            => ReplacementResult = newResult;
    }
}