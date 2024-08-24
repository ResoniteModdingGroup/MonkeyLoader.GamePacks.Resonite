using FrooxEngine;
using MonkeyLoader.Components;
using MonkeyLoader.Configuration;
using MonkeyLoader.Resonite.DataFeeds;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLoader.Resonite.Configuration
{
    /// <summary>
    /// Represents a basic config key component which generate <see cref="DataFeedItem"/>s
    /// for the config key to represent themselves, rather than requiring default handling.
    /// </summary>
    public abstract class ConfigKeyCustomDataFeedItems<T> : IConfigKeyCustomDataFeedItems<T>
    {
        /// <summary>
        /// Gets the config item that <see cref="DataFeedItem"/>s will be generated for.
        /// </summary>
        public IDefiningConfigKey<T> ConfigKey { get; private set; } = null!;

        /// <inheritdoc/>
        public abstract IAsyncEnumerable<DataFeedItem> Enumerate(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, string searchPhrase, object viewData);

        void IComponent<IDefiningConfigKey<T>>.Initialize(IDefiningConfigKey<T> entity)
        {
            ConfigKey = entity;

            OnInitialize(entity);
        }

        /// <summary>
        /// Initializes this component when it's added to an
        /// entity's <see cref="IEntity{TEntity}.Components">component list</see>.<br/>
        /// This may throw a <see cref="InvalidOperationException"/> when the state of the given entity is invalid for this component.
        /// </summary>
        /// <remarks>
        /// The <see cref="ConfigKey">ConfigKey</see> property is already initialized when this is called.
        /// </remarks>
        /// <param name="entity">The entity this component was added to.</param>
        /// <exception cref="InvalidOperationException">When the state of the given entity is invalid.</exception>
        protected virtual void OnInitialize(IDefiningConfigKey<T> entity)
        { }
    }

    /// <summary>
    /// Defines the interface for config key components which generate <see cref="DataFeedItem"/>s
    /// for the config key to represent themselves, rather than requiring default handling.
    /// </summary>
    public interface IConfigKeyCustomDataFeedItems<T> : IConfigKeyComponent<IDefiningConfigKey<T>>, ICustomDataFeedItems
    { }
}