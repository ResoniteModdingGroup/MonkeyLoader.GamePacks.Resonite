using EnumerableToolkit;
using FrooxEngine;
using FrooxEngine.UIX;
using MonkeyLoader.Resonite.DataFeeds.Settings;

namespace MonkeyLoader.Resonite.Configuration
{
    /// <summary>
    /// Implements a <see cref="IConfigKeyCustomDataFeedItems{T}">custom data feed items component</see>
    /// that will apply a <see cref="MaskPattern">MaskPattern</see> to the string input for the
    /// <see cref="ConfigKeyCustomDataFeedItems{T}.ConfigKey">ConfigKey</see>.
    /// </summary>
    /// <inheritdoc/>
    public sealed class ConfigKeyProtectedString : ConfigKeyCustomDataFeedItems<string>
    {
        /// <summary>
        /// Gets the mask pattern that will be applied to the string input for the
        /// <see cref="ConfigKeyCustomDataFeedItems{T}.ConfigKey">ConfigKey</see>.
        /// </summary>
        /// <remarks>
        /// Note that while <see cref="Text.MaskPattern"/> is a <see langword="string"/>,
        /// the content is always masked by repeating the first character only.
        /// </remarks>
        public string MaskPattern { get; }

        /// <summary>
        /// Creates a new instance of this <see cref="IConfigKeyCustomDataFeedItems{T}">custom data feed items component</see>
        /// that will apply the given <paramref name="maskPattern"/> to the string input for the
        /// <see cref="ConfigKeyCustomDataFeedItems{T}.ConfigKey">ConfigKey</see>.
        /// </summary>
        /// <param name="maskPattern"><para>
        /// The mask pattern that will be applied to the string input for the
        /// <see cref="ConfigKeyCustomDataFeedItems{T}.ConfigKey">ConfigKey</see>.
        /// </para><para>
        /// Note that while <see cref="Text.MaskPattern"/> is a <see langword="string"/>,
        /// the content is always masked by repeating the first character only.
        /// </para></param>
        public ConfigKeyProtectedString(string maskPattern = "*")
        {
            MaskPattern = maskPattern;
        }

        /// <inheritdoc/>
        public override IAsyncEnumerable<DataFeedItem> Enumerate(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, string? searchPhrase, object? viewData)
        {
            var item = ConfigKey.MakeValueField(path, groupKeys);
            var existingSetup = item.SetupValue;

            item.InitSetupValue(field =>
            {
                existingSetup(field);

                var slot = field.FindNearestParent<Slot>();

                if (slot.GetComponentInParents<FeedValueFieldInterface<string>>() is not { } valueFieldInterface)
                {
                    ConfigKey.Config.Logger.Warn(() => $"Tried to set up protected string on data feed item template without FeedValueFieldInterface<string> for config key {ConfigKey.FullId}!");
                    return;
                }

                if (valueFieldInterface.Value.Target?.Parent is not Text text)
                {
                    ConfigKey.Config.Logger.Warn(() => $"Tried to set up protected string on data feed item template without FeedValueFieldInterface<string>.Value.Target owned by a Text for config key {ConfigKey.FullId}!");
                    return;
                }

                text.ParseRichText.Value = false;

                var booleanDriver = text.Slot.AttachComponent<BooleanValueDriver<string>>();
                booleanDriver.State.DriveByEquality(text.Content!, null);
                booleanDriver.TargetField.Target = text.MaskPattern;
                booleanDriver.FalseValue.Value = MaskPattern;
            });

            return item.YieldAsync();
        }
    }
}