using EnumerableToolkit;
using EnumerableToolkit.Builder;
using EnumerableToolkit.Builder.AsyncBlocks;
using FrooxEngine;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLoader.Resonite.DataFeeds
{
    /// <summary>
    /// Represents the base class for patchers that run after Resonite's assemblies have been loaded and that hook into the game's lifecycle.<br/>
    /// Specifically, to add this class as a building block to an <see cref="IAsyncParametrizedEnumerableBuilder{T, TParameters}">async
    /// parametrized enumerable builder</see> for a <typeparamref name="TDataFeed"/>'s <see cref="IDataFeed.Enumerate">Enumerate</see> method.
    /// </summary>
    /// <inheritdoc/>
    public abstract class DataFeedBuildingBlockMonkey<TMonkey, TDataFeed> : DataFeedBuilderMonkey<TMonkey, TDataFeed>,
            IAsyncParametrizedBuildingBlock<DataFeedItem, EnumerateDataFeedParameters<TDataFeed>>, IPrioritizable
        where TMonkey : DataFeedBuildingBlockMonkey<TMonkey, TDataFeed>, new()
        where TDataFeed : IDataFeed
    {
        /// <remarks>
        /// This is the priority with which this building block will be added
        /// to the data feed enumerate sequence builder.
        /// </remarks>
        /// <inheritdoc/>
        public abstract int Priority { get; }

        /// <summary>
        /// This method implements the transformation of the <paramref name="current"/>
        /// async sequence that this building block should apply for the
        /// <see cref="IDataFeed.Enumerate">DataFeed.Enumerate</see> call.
        /// </summary>
        /// <remarks>
        /// The default items of the <see cref="IDataFeed"/>.<see cref="IDataFeed.Enumerate">Enumerate</see>() method
        /// are inserted first at priority 0, unless disabled through the parameters.
        /// </remarks>
        public abstract IAsyncEnumerable<DataFeedItem> Apply(IAsyncEnumerable<DataFeedItem> current, EnumerateDataFeedParameters<TDataFeed> parameters);

        /// <inheritdoc/>
        protected override sealed void AddBuildingBlocks(IAsyncParametrizedEnumerableBuilder<DataFeedItem, EnumerateDataFeedParameters<TDataFeed>> builder)
            => builder.AddBuildingBlock(((IAsyncParametrizedBuildingBlock<DataFeedItem, EnumerateDataFeedParameters<TDataFeed>>)this).WithPriority(Priority));
    }
}