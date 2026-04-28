using FrooxEngine;
using MonkeyLoader.Configuration;
using MonkeyLoader.Resonite.DataFeeds.Settings;

namespace MonkeyLoader.Resonite.Configuration
{
    /// <summary>
    /// Implements a <see cref="IConfigKeyCustomDataFeedItems{T}">custom data feed items component</see>
    /// that will use another config key as the <see cref="ConfigToSettingsExtensions.WithEnabledSource">enabled source</see>
    /// for the default items generated for the <see cref="ConfigKeyCustomDataFeedItems{T}.ConfigKey">ConfigKey</see>.
    /// </summary>
    /// <typeparam name="T">The type of the config key </typeparam>
    /// <inheritdoc/>
    public sealed class ConfigKeyEnabledSource<T> : ConfigKeyCustomDataFeedItems<T>
    {
        /// <summary>
        /// Gets the config item that will be used as the
        /// <see cref="ConfigToSettingsExtensions.WithEnabledSource{TDataFeedItem}(TDataFeedItem, IDefiningConfigKey{bool})">enabled source</see>
        /// for the default items generated for the <see cref="ConfigKeyCustomDataFeedItems{T}.ConfigKey">ConfigKey</see>.
        /// </summary>
        public IDefiningConfigKey<bool> EnabledSource { get; }

        /// <summary>
        /// Creates a new instance of this <see cref="IConfigKeyCustomDataFeedItems{T}">custom data feed items component</see>
        /// that will use the given <see cref="IDefiningConfigKey{T}">config key</see> as the
        /// <see cref="ConfigToSettingsExtensions.WithEnabledSource{TDataFeedItem}(TDataFeedItem, IDefiningConfigKey{bool})">enabled source</see>
        /// for the default items generated for the <see cref="ConfigKeyCustomDataFeedItems{T}.ConfigKey">ConfigKey</see>.
        /// </summary>
        /// <param name="enabledSource"></param>
        /// <inheritdoc cref="ConfigKeyCustomDataFeedItems{T}"/>
        public ConfigKeyEnabledSource(IDefiningConfigKey<bool> enabledSource)
        {
            EnabledSource = enabledSource;
        }

        /// <inheritdoc/>
        public override async IAsyncEnumerable<DataFeedItem> Enumerate(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, string? searchPhrase, object? viewData)
        {
            await foreach (var feedItem in ConfigKey.EnumerateDefaultItemsAsync(path, groupKeys))
                yield return feedItem.WithEnabledSource(EnabledSource);
        }
    }
}