using Elements.Core;
using MonkeyLoader.Configuration;
using System;

namespace MonkeyLoader.Resonite.UI.Tooltips
{
    public sealed class TooltipConfig : SingletonConfigSection<TooltipConfig>
    {
        private static readonly DefiningConfigKey<colorX> _backgroundColorKey = new("BackgroundColor", "Sets the background color of a tooltip.", () => colorX.Black.SetA(.75f));
        private static readonly DefiningConfigKey<colorX> _textColorKey = new("TextColor", "Sets the text color of a tooltip.", () => colorX.White);

        private static readonly DefiningConfigKey<float> _textScaleKey = new("TextSize", "Sets the size of the text on a tooltip.", () => 1)
        {
            new ConfigKeyRange<float>(.5f, 4)
        };

        public colorX BackgroundColor => _backgroundColorKey;

        /// <inheritdoc/>
        public override string Description => "Contains settings for the tooltips displayed for buttons.";

        /// <inheritdoc/>
        public override string Id => "Tooltips";

        public colorX TextColor => _textColorKey;
        public float TextScale => _textScaleKey;

        /// <inheritdoc/>
        public override Version Version { get; } = new Version(1, 0, 0);
    }
}