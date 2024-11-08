using EnumerableToolkit;
using EnumerableToolkit.Builder;
using EnumerableToolkit.Builder.AsyncBlocks;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.DataFeeds
{
    /// <summary>
    /// Represents the base class for patchers that run after Resonite's assemblies have been loaded and that hook into the game's lifecycle.<br/>
    /// Specifically, to add one or more <see cref="IAsyncParametrizedBuildingBlock{T, TParameters}">async
    /// parametrized building blocks</see> to an <see cref="IAsyncParametrizedEnumerableBuilder{T, TParameters}">async
    /// parametrized enumerable builder</see> for a <typeparamref name="TDataFeed"/>'s <see cref="IDataFeed.Enumerate">Enumerate</see> method.
    /// </summary>
    /// <typeparam name="TMonkey">The type of the actual patcher.</typeparam>
    /// <typeparam name="TDataFeed">The <see cref="IDataFeed"/> to modify.</typeparam>
    /// <inheritdoc/>
    public abstract class DataFeedBuilderMonkey<TMonkey, TDataFeed> : ResoniteMonkey<TMonkey>
        where TMonkey : DataFeedBuilderMonkey<TMonkey, TDataFeed>, new()
        where TDataFeed : IDataFeed
    {
        private Prioritizable<IAsyncParametrizedBuildingBlock<DataFeedItem, EnumerateDataFeedParameters<TDataFeed>>>[] _addedBlocks = [];

        /// <summary>
        /// Called to add one or more <see cref="IAsyncParametrizedBuildingBlock{T, TParameters}">async
        /// parametrized building blocks</see> to the <see cref="IAsyncParametrizedEnumerableBuilder{T, TParameters}">async
        /// parametrized enumerable builder</see> for this <typeparamref name="TDataFeed"/>.
        /// </summary>
        /// <remarks>
        /// The default items of the <see cref="IDataFeed"/>.<see cref="IDataFeed.Enumerate">Enumerate</see>() method
        /// are inserted at priority 0, unless disabled through the parameters.
        /// </remarks>
        /// <param name="builder">The enumerable builder to add one or more building blocks too.</param>
        protected abstract void AddBuildingBlocks(IAsyncParametrizedEnumerableBuilder<DataFeedItem, EnumerateDataFeedParameters<TDataFeed>> builder);

        /// <remarks>
        /// Override this method if you need to patch something involved in the initialization of the game.<br/>
        /// For ResoniteMonkeys, the default behavior of<see cref="Monkey{TMonkey}.OnLoaded">OnLoaded</see>()
        /// is moved to <see cref="ResoniteMonkey{TMonkey}.OnEngineReady">OnEngineReady</see>().
        /// <para/>
        /// Strongly consider also overriding <see cref="ResoniteMonkey{TMonkey}.OnEngineReady">OnEngineReady</see>() if you override this method.<br/>
        /// Otherwise your patches will be applied twice, if you're using <c>[<see cref="HarmonyPatchCategory"/>(nameof(MyPatcher))]</c> attributes.
        /// <para/>
        /// <i>By default:</i> Calls <see cref="AddBuildingBlocks">AddBuildingBlock</see> and returns <c>true</c>.
        /// </remarks>
        /// <inheritdoc/>
        protected override bool OnLoaded()
        {
            try
            {
                var before = Traverse.Create(DataFeedInjector<TDataFeed>.Builder)
                    .Field<PrioritySortedCollection<Prioritizable<IAsyncParametrizedBuildingBlock<DataFeedItem,
                        EnumerateDataFeedParameters<TDataFeed>>>>>("_buildingBlocks")
                    .Value.ToImmutableHashSet();

                AddBuildingBlocks(DataFeedInjector<TDataFeed>.Builder);

                var after = Traverse.Create(DataFeedInjector<TDataFeed>.Builder)
                    .Field<PrioritySortedCollection<Prioritizable<IAsyncParametrizedBuildingBlock<DataFeedItem,
                        EnumerateDataFeedParameters<TDataFeed>>>>>("_buildingBlocks")
                    .Value.ToImmutableHashSet();

                _addedBlocks = [.. after.Except(before)];
            }
            catch (Exception ex)
            {
                Logger.Error(ex.LogFormat("Failed to add building block for data feed!"));

                return false;
            }

            return base.OnLoaded();
        }

        /// <remarks>
        /// <i>By default:</i> Removes all building blocks
        /// <see cref="AddBuildingBlocks">added</see> by this Monkey and
        /// all <see cref="HarmonyLib.Harmony"/> patches done
        /// using this Monkey's <see cref="Harmony">Harmony</see> instance,
        /// if not exiting, and returns <c>true</c>.
        /// </remarks>
        /// <inheritdoc/>
        protected override bool OnShutdown(bool applicationExiting)
        {
            if (!applicationExiting)
            {
                foreach (var buildingBlock in _addedBlocks)
                    DataFeedInjector<TDataFeed>.Builder.RemoveBuildingBlock(buildingBlock);
            }

            return base.OnShutdown(applicationExiting);
        }
    }
}