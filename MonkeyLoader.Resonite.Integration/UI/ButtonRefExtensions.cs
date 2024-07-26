using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.UI
{
    public static class ButtonRefExtensions
    {
        public static Button ButtonRef<T>(this UIBuilder ui, Uri spriteUrl, in colorX? buttonTint,
                        ButtonEventHandler<T> callback, T argument, float doublePressDelay = 0)
            where T : class, IWorldElement
            => ui.Button(spriteUrl, in buttonTint).SetupRefRelay(callback, argument, doublePressDelay);

        public static Button ButtonRef<T>(this UIBuilder ui, Uri spriteUrl, in colorX? buttonTint, in colorX spriteTint,
                ButtonEventHandler<T> callback, T argument, float doublePressDelay = 0)
            where T : class, IWorldElement
            => ui.Button(spriteUrl, in buttonTint, in spriteTint).SetupRefRelay(callback, argument, doublePressDelay);

        public static Button ButtonRef<T>(this UIBuilder ui, Uri spriteUrl,
                ButtonEventHandler<T> callback, T argument, float doublePressDelay = 0)
            where T : class, IWorldElement
            => ui.Button(spriteUrl).SetupRefRelay(callback, argument, doublePressDelay);

        public static Button ButtonRef<T>(this UIBuilder ui, IAssetProvider<Sprite> sprite, LocaleString text,
                ButtonEventHandler<T> callback, T argument, float doublePressDelay = 0)
            where T : class, IWorldElement
            => ui.Button(sprite, text).SetupRefRelay(callback, argument, doublePressDelay);

        public static Button ButtonRef<T>(this UIBuilder ui, IAssetProvider<Sprite> sprite, in colorX spriteTint, LocaleString text,
                ButtonEventHandler<T> callback, T argument, float doublePressDelay = 0, float buttonTextSplit = .33333f, float buttonTextSplitGap = .05f)
            where T : class, IWorldElement
            => ui.Button(sprite, in spriteTint, text, buttonTextSplit, buttonTextSplitGap).SetupRefRelay(callback, argument, doublePressDelay);

        public static Button ButtonRef<T>(this UIBuilder ui, IAssetProvider<Sprite> sprite, in colorX? buttonTint, in colorX spriteTint,
                ButtonEventHandler<T> callback, T argument, float doublePressDelay = 0)
            where T : class, IWorldElement
            => ui.Button(sprite, in buttonTint, in spriteTint).SetupRefRelay(callback, argument, doublePressDelay);

        public static Button ButtonRef<T>(this UIBuilder ui, Uri icon, LocaleString text,
                ButtonEventHandler<T> callback, T argument, float doublePressDelay = 0)
            where T : class, IWorldElement
            => ui.Button(icon, text).SetupRefRelay(callback, argument, doublePressDelay);

        public static Button ButtonRef<T>(this UIBuilder ui, LocaleString text, in colorX? buttonTint,
                ButtonEventHandler<T> callback, T argument, float doublePressDelay = 0)
            where T : class, IWorldElement
            => ui.Button(text, buttonTint).SetupRefRelay(callback, argument, doublePressDelay);

        public static Button ButtonRef<T>(this UIBuilder ui, Uri icon, LocaleString text, in colorX? buttonTint, in colorX spriteTint,
                ButtonEventHandler<T> callback, T argument, float doublePressDelay = 0)
            where T : class, IWorldElement
            => ui.Button(icon, text, buttonTint, spriteTint).SetupRefRelay(callback, argument, doublePressDelay);

        public static TButton SetupRefRelay<TButton, TArgument>(this TButton button,
                ButtonEventHandler<TArgument> callback, TArgument argument, float doublePressDelay = 0)
            where TButton : IButton
            where TArgument : class, IWorldElement
        {
            var relay = button.Slot.AttachComponent<ButtonRefRelay<TArgument>>();
            relay.DoublePressDelay.Value = doublePressDelay;
            relay.ButtonPressed.Target = callback;
            relay.Argument.Target = argument;

            return button;
        }
    }
}