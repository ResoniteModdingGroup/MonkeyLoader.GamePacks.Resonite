using Elements.Core;
using MonkeyLoader.Configuration;
using System;

namespace MonkeyLoader.Resonite.UI
{
    internal sealed class ButtonTooltipConfig : ConfigSection
    {
        private static readonly DefiningConfigKey<colorX> _bgColor = new("BackgroundColor", "Sets the background color of a tooltip.", () => colorX.Black.SetA(.75f));
        private static readonly DefiningConfigKey<colorX> _textColor = new("TextColor", "Sets the text color of a tooltip.", () => colorX.White);

        private static readonly DefiningConfigKey<float> _textScale = new("TextSize", "Sets the size of the text on a tooltip.", () => 1)
        {
            new ConfigKeyRange<float>(.5f, 4)
        };

        public override string Description => "Contains settings for the tooltips displayed for buttons.";
        public override string Id => "Tooltips";
        public override Version Version { get; } = new Version(1, 0, 0);
    }
}