using Elements.Core;
using Elements.Quantity;
using FrooxEngine;
using MonkeyLoader.Configuration;
using MonkeyLoader.Resonite.Configuration;
using System;

namespace MonkeyLoader.Resonite.UI.Tooltips
{
    /// <summary>
    /// Contains settings for the tooltips displayed for buttons.
    /// </summary>
    public sealed class TooltipConfig : SingletonConfigSection<TooltipConfig>
    {
        private static readonly DefiningConfigKey<colorX> _backgroundColorKey = new("BackgroundColor", "Sets the background color of a tooltip.", () => RadiantUI_Constants.BG_COLOR);
        private static readonly DefiningConfigKey<bool> _enableDebugButtonData = new("EnableDebugButtonData", "Controls whether debug data for missing button tooltips is logged. Useful when wanting to add new labels.", () => false);
        private static readonly DefiningConfigKey<bool> _enableDebugTooltipStay = new("EnableDebugTooltipStay", "Controls whether tooltips stick around after hovering off the button they're attached to.\r\nHover again after disabling to remove them.", () => false);
        private static readonly DefiningConfigKey<bool> _enableNonLocalTooltips = new("EnableNonLocalTooltips", "When enabled, tooltips are created as regular slots instead of local ones. Can be used to show them to others. Experimental.", () => false);

        private static readonly DefiningConfigKey<float> _hoverTime = new("HoverTime", "The time required hovering a button before a tooltip may be opened.", () => 0.5f)
        {
            new ConfigKeyQuantity<float, Time>(new UnitConfiguration("s", "0", " ", ["s", "ms"]), null, 0, 2)
        };

        private static readonly DefiningConfigKey<bool> _showExtendedTooltipForHyperlinks = new("ShowExtendedTooltipForHyperlinks", "When enabled, a Hyperlink component's reason and URL are always shown in tooltips.", () => false);
        private static readonly DefiningConfigKey<colorX> _textColorKey = new("TextColor", "Sets the text color of a tooltip.", () => RadiantUI_Constants.TEXT_COLOR);

        private static readonly DefiningConfigKey<float> _textScaleKey = new("TextSize", "Sets the size of the text on a tooltip.", () => 1f)
        {
            new ConfigKeyRange<float>(.5f, 4)
        };

        private static readonly DefiningConfigKey<ShadowType> _testKey0 = new("testKey0", "Test key0.", () => ShadowType.Hard);
        private static readonly DefiningConfigKey<ShadowType?> _testKey = new("testKey", "Test key.", () => ShadowType.Soft);
        private static readonly DefiningConfigKey<MappingTarget> _testKey3 = new("testKey3", "Test key3.", () => MappingTarget.NONE);
        private static readonly DefiningConfigKey<MappingTarget?> _testKey2 = new("testKey2", "Test key2.", () => null);
        private static readonly DefiningConfigKey<float?> _testKey4 = new("testKey4", "Test key4.", () => null);

        /// <summary>
        /// Gets the background color for tooltips.
        /// </summary>
        public colorX BackgroundColor => _backgroundColorKey;

        /// <inheritdoc/>
        public override string Description => "Contains settings for the tooltips displayed for buttons.";

        /// <summary>
        /// Gets whether debug data for missing button tooltips is logged.
        /// </summary>
        public bool EnableDebugButtonData => _enableDebugButtonData;

        /// <summary>
        /// Gets whether tooltips stick around after hovering off the button they're attached to.
        /// </summary>
        public bool EnableDebugTooltipStay => _enableDebugTooltipStay;

        /// <summary>
        /// Gets whether tooltips should use regular slots rather than local ones.
        /// </summary>
        public bool EnableNonLocalTooltips => _enableNonLocalTooltips;

        /// <summary>
        /// Gets the time required hovering a button before a tooltip may be opened.
        /// </summary>
        /// <value>The time in seconds.</value>
        public float HoverTime => _hoverTime;

        /// <inheritdoc/>
        public override string Id => "Tooltips";

        /// <summary>
        /// Gets whether a Hyperlink component's reason and URL are always shown in tooltips.
        /// </summary>
        public bool ShowExtendedTooltipForHyperlinks => _showExtendedTooltipForHyperlinks;

        /// <summary>
        /// Gets the text color for tooltips.
        /// </summary>
        public colorX TextColor => _textColorKey;

        /// <summary>
        /// Gets the text scale for tooltips.
        /// </summary>
        public float TextScale => _textScaleKey;

        /// <inheritdoc/>
        public override Version Version { get; } = new Version(1, 0, 0);
    }
}