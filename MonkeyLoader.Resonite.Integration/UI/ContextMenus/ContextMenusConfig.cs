using FrooxEngine;
using MonkeyLoader.Configuration;
using MonkeyLoader.Resonite.Configuration;
using MonkeyLoader.Resonite.DataFeeds.Settings;

namespace MonkeyLoader.Resonite.UI.ContextMenus
{
    /// <summary>
    /// Contains settings for the generation of Context Menus.
    /// </summary>
    public sealed class ContextMenusConfig : SingletonConfigSection<ContextMenusConfig>
    {
        private static readonly ConfigKeySubgroup _itemLimitSubgroup = new(9, "ItemLimit");
        private static readonly ConfigKeySubgroup _scalingSubgroup = new(7, "Scaling");
        private static readonly ConfigKeySubgroup _toolEquippedSubgroup = new("ToolEquipped");

        private readonly DefiningConfigKey<bool> _alwaysAllowScaleToggle = new("AlwaysAllowScaleToggle", "Show the scaling toggle even when not at the default scale for the avatar.", () => false)
        {
           _scalingSubgroup,
            new ConfigKeyPriority(7)
        };

        private readonly DefiningConfigKey<bool> _alwaysShowLocomotion = new("AlwaysShowLocomotion", "Show the locomotion selection even when a tool is equipped.", () => false)
        {
            _toolEquippedSubgroup
        };

        private readonly DefiningConfigKey<bool> _alwaysShowScaling = new("AlwaysShowScaling", "Show the scaling toggle and/or reset scale action even when a tool is equipped.", () => false)
        {
            _toolEquippedSubgroup
        };

        private readonly DefiningConfigKey<int> _contextMenuItemLimit = new("ContextMenuItemLimit", "Limit the number of items shown in the context menu. If this number is exceeded, the menu will get paginated.", () => 16)
        {
            _itemLimitSubgroup,
            new ConfigKeyPriority(8),
            new ConfigKeyRange<int>(3, 32)
        };

        private readonly DefiningConfigKey<bool> _limitContextMenuItems = new("LimitContextMenuItems", "Limit the number of items shown in the context menu. If the configured number is exceeded, the menu will get paginated.", () => true)
        {
            _itemLimitSubgroup,
            new ConfigKeyPriority(9)
        };

        private readonly DefiningConfigKey<bool> _showResetScaleWithToggle = new("ShowResetScaleWithToggle", "Show the reset scale action alongside the scaling toggle when it is always allowed.", () => true)
        {
            _scalingSubgroup,
            new ConfigKeyPriority(6)
        };

        private readonly DefiningConfigKey<bool> _showSaveLocation = new("ShowSaveLocation", "Show the current inventory path when trying to save something.", () => true)
        {
            new ConfigKeyPriority(10)
        };

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

        /// <summary>
        /// Gets the limit for the number of non-pagination items shown in the context menu.<br/>
        /// If this number is exceeded, the menu will get paginated.
        /// </summary>
        public int ContextMenuItemLimit => _contextMenuItemLimit - 2;

        /// <inheritdoc/>
        public override string Description => "Contains settings for the generation of Context Menus.";

        /// <inheritdoc/>
        public override string Id => "ContextMenus";

        /// <summary>
        /// Limit the number of items shown in the context menu.<br/>
        /// If the <see cref="ContextMenuItemLimit">configured number</see> is exceeded, the menu will get paginated.
        /// </summary>
        public bool LimitContextMenuItems => _limitContextMenuItems;

        /// <summary>
        /// Gets whether to show the reset scale action alongside the scaling toggle when it is always allowed.
        /// </summary>
        public bool ShowResetScaleWithToggle => _showResetScaleWithToggle;

        /// <summary>
        /// Gets whether to show the current inventory path when trying to save something.
        /// </summary>
        public bool ShowSaveLocation => _showSaveLocation;

        /// <inheritdoc/>
        public override Version Version { get; } = new(1, 1, 0);

        /// <inheritdoc/>
        public ContextMenusConfig()
        {
            _contextMenuItemLimit.Components.Add(new ConfigKeyEnabledSource<int>(_limitContextMenuItems));
            _showResetScaleWithToggle.Components.Add(new ConfigKeyEnabledSource<bool>(_alwaysAllowScaleToggle));
        }
    }
}