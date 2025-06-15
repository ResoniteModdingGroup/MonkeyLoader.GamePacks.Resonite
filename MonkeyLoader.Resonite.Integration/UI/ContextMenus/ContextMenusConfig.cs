using FrooxEngine;
using MonkeyLoader.Configuration;
using MonkeyLoader.Resonite.Configuration;
using MonkeyLoader.Resonite.DataFeeds.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkeyLoader.Resonite.UI.ContextMenus
{
    /// <summary>
    /// Contains settings for the generation of Context Menus.
    /// </summary>
    public sealed class ContextMenusConfig : SingletonConfigSection<ContextMenusConfig>
    {
        private static readonly DefiningConfigKey<bool> _alwaysAllowScaleToggle = new("AlwaysAllowScaleToggle", "Show the scaling toggle even when not at the default scale for the avatar.", () => false);
        private static readonly DefiningConfigKey<bool> _alwaysShowLocomotion = new("AlwaysShowLocomotion", "Show the locomotion selection even when a tool is equipped.", () => false);
        private static readonly DefiningConfigKey<bool> _alwaysShowScaling = new("AlwaysShowScalingToggle", "Show the scaling toggle and/or reset scale action even when a tool is equipped.", () => false);

        private static readonly DefiningConfigKey<bool> _showResetScaleWithToggle = new("ShowResetScaleWithToggle", "Show the reset scale action alongside the scaling toggle when it is always allowed.", () => true)
        {
            new AlwaysAllowScaleToggleLink()
        };

        private static readonly DefiningConfigKey<bool> _showSaveLocation = new("ShowSaveLocation", "Show the current inventory path when trying to save something.", () => true);

        /// <summary>
        /// Gets whether to show the scaling toggle even when not at the default scale for the avatar.
        /// </summary>
        public bool AlwaysAllowScaleToggle => _alwaysAllowScaleToggle;

        /// <summary>
        /// Gets whether to show the locomotion selection even when a tool is equipped.
        /// </summary>
        public bool AlwaysShowLocomotion => _alwaysShowLocomotion;

        /// <summary>
        /// Gets whether to show the locomotion selection,
        /// or the scaling toggle and/or reset scale action even when a tool is equipped.
        /// </summary>
        public bool AlwaysShowLocomotionOrScaling => _alwaysShowLocomotion || _alwaysShowScaling;

        /// <summary>
        /// Gets whether to show the scaling toggle and/or reset scale action even when a tool is equipped.
        /// </summary>
        public bool AlwaysShowScaling => _alwaysShowScaling;

        /// <inheritdoc/>
        public override string Description => "Contains settings for the generation of Context Menus.";

        /// <inheritdoc/>
        public override string Id => "ContextMenus";

        /// <summary>
        /// Gets whether to show the reset scale action alongside the scaling toggle when it is always allowed.
        /// </summary>
        public bool ShowResetScaleWithToggle => _showResetScaleWithToggle;

        /// <summary>
        /// Gets whether to show the current inventory path when trying to save something.
        /// </summary>
        public bool ShowSaveLocation => _showSaveLocation;

        /// <inheritdoc/>
        public override Version Version { get; } = new(1, 0, 0);

        private sealed class AlwaysAllowScaleToggleLink : ConfigKeyCustomDataFeedItems<bool>
        {
            public override IAsyncEnumerable<DataFeedItem> Enumerate(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, string? searchPhrase, object? viewData)
            {
                var toggle = new DataFeedToggle();
                toggle.InitBase(path, groupKeys, ConfigKey);
                toggle.InitSetupValue(field => field.SetupConfigKeyField(ConfigKey));
                toggle.InitEnabled(field => field.SetupConfigKeyField(_alwaysAllowScaleToggle));

                return new[] { toggle }.ToAsyncEnumerable();
            }
        }
    }
}