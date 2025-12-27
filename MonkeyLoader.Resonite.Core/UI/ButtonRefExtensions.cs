using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS1572 // XML comment has a param tag, but there is no parameter by that name
#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

namespace MonkeyLoader.Resonite.UI
{
    /// <summary>
    /// Contains <see cref="UIBuilder"/> extension methods that simplify
    /// the creation of <see cref="Button"/>s with <see cref="ButtonRefRelay{T}"/>s.
    /// </summary>
    [TypeForwardedFrom("MonkeyLoader.Resonite.Integration")]
    public static class ButtonRefExtensions
    {
        /// <inheritdoc cref="ButtonRef{T}(UIBuilder, in LocaleString, in colorX?, ButtonEventHandler{T}, T, float)"/>
        public static Button ButtonRef<T>(this UIBuilder builder, in LocaleString text,
                ButtonEventHandler<T> callback, T argument, float doublePressDelay = 0)
            where T : class, IWorldElement
            => builder.Button(text).WithRefRelay(callback, argument, doublePressDelay);

        /// <summary>
        /// Creates a <see cref="Button"/> with text and <see cref="WithRefRelay">adds</see>
        /// a <see cref="ButtonRefRelay{T}"/> to it using the given parameters.
        /// </summary>
        /// <inheritdoc cref="ButtonRef{T}(UIBuilder, IAssetProvider{Sprite}, in colorX, in LocaleString, ButtonEventHandler{T}, T, float, float, float)"/>
        public static Button ButtonRef<T>(this UIBuilder builder, in LocaleString text, in colorX? buttonTint,
                ButtonEventHandler<T> callback, T argument, float doublePressDelay = 0)
            where T : class, IWorldElement
            => builder.Button(text, buttonTint).WithRefRelay(callback, argument, doublePressDelay);

        /// <inheritdoc cref="ButtonRef{T}(UIBuilder, IAssetProvider{Sprite}, in colorX, in LocaleString, ButtonEventHandler{T}, T, float, float, float)"/>
        public static Button ButtonRef<T>(this UIBuilder builder, Uri spriteUrl, in LocaleString text,
                ButtonEventHandler<T> callback, T argument, float doublePressDelay = 0)
            where T : class, IWorldElement
            => builder.Button(spriteUrl, text).WithRefRelay(callback, argument, doublePressDelay);

        /// <inheritdoc cref="ButtonRef{T}(UIBuilder, IAssetProvider{Sprite}, in colorX, in LocaleString, ButtonEventHandler{T}, T, float, float, float)"/>
        public static Button ButtonRef<T>(this UIBuilder builder, Uri spriteUrl, in LocaleString text, in colorX? buttonTint, in colorX spriteTint,
                ButtonEventHandler<T> callback, T argument, float doublePressDelay = 0)
            where T : class, IWorldElement
            => builder.Button(spriteUrl, text, buttonTint, spriteTint).WithRefRelay(callback, argument, doublePressDelay);

        /// <inheritdoc cref="ButtonRef{T}(UIBuilder, Uri, in colorX?, in colorX, ButtonEventHandler{T}, T, float)"/>
        public static Button ButtonRef<T>(this UIBuilder builder, Uri spriteUrl,
                ButtonEventHandler<T> callback, T argument, float doublePressDelay = 0)
            where T : class, IWorldElement
            => builder.Button(spriteUrl).WithRefRelay(callback, argument, doublePressDelay);

        /// <inheritdoc cref="ButtonRef{T}(UIBuilder, Uri, in colorX?, in colorX, ButtonEventHandler{T}, T, float)"/>
        public static Button ButtonRef<T>(this UIBuilder builder, Uri spriteUrl, in colorX? buttonTint,
                ButtonEventHandler<T> callback, T argument, float doublePressDelay = 0)
            where T : class, IWorldElement
            => builder.Button(spriteUrl, in buttonTint).WithRefRelay(callback, argument, doublePressDelay);

        /// <summary>
        /// Creates a <see cref="Button"/> with a sprite and <see cref="WithRefRelay">adds</see>
        /// a <see cref="ButtonRefRelay{T}"/> to it using the given parameters.
        /// </summary>
        /// <inheritdoc cref="ButtonRef{T}(UIBuilder, IAssetProvider{Sprite}, in colorX, in LocaleString, ButtonEventHandler{T}, T, float, float, float)"/>
        public static Button ButtonRef<T>(this UIBuilder builder, Uri spriteUrl, in colorX? buttonTint, in colorX spriteTint,
                ButtonEventHandler<T> callback, T argument, float doublePressDelay = 0)
            where T : class, IWorldElement
            => builder.Button(spriteUrl, in buttonTint, in spriteTint).WithRefRelay(callback, argument, doublePressDelay);

        /// <inheritdoc cref="ButtonRef{T}(UIBuilder, IAssetProvider{Sprite}, in colorX, in LocaleString, ButtonEventHandler{T}, T, float, float, float)"/>
        public static Button ButtonRef<T>(this UIBuilder builder, IAssetProvider<Sprite> sprite, in LocaleString text,
                ButtonEventHandler<T> callback, T argument, float doublePressDelay = 0)
            where T : class, IWorldElement
            => builder.Button(sprite, text).WithRefRelay(callback, argument, doublePressDelay);

        /// <inheritdoc cref="ButtonRef{T}(UIBuilder, Uri, in colorX?, in colorX, ButtonEventHandler{T}, T, float)"/>
        public static Button ButtonRef<T>(this UIBuilder builder, IAssetProvider<Sprite> sprite, in colorX? buttonTint, in colorX spriteTint,
                ButtonEventHandler<T> callback, T argument, float doublePressDelay = 0)
            where T : class, IWorldElement
            => builder.Button(sprite, in buttonTint, in spriteTint).WithRefRelay(callback, argument, doublePressDelay);

        /// <summary>
        /// Creates a <see cref="Button"/> with a sprite and text, and <see cref="WithRefRelay">adds</see>
        /// a <see cref="ButtonRefRelay{T}"/> to it using the given parameters.
        /// </summary>
        /// <typeparam name="T">The type of the reference argument.</typeparam>
        /// <param name="builder">The builder to use for creating the button.</param>
        /// <param name="spriteUrl">The url for the sprite displayed on the button.</param>
        /// <param name="sprite">The sprite displayed on the button.</param>
        /// <param name="spriteTint">The tint of the sprite.</param>
        /// <param name="buttonTint">The background color of the button.</param>
        /// <param name="text">The text displayed on the button.</param>
        /// <param name="buttonTextSplit">The ratio of the button to use for the sprite.</param>
        /// <param name="buttonTextSplitGap">The ratio of the button to keep as space between sprite and text. The sprite and text ratios are decreased by half of it.</param>
        /// <inheritdoc cref="WithRefRelay"/>
        public static Button ButtonRef<T>(this UIBuilder builder, IAssetProvider<Sprite> sprite, in colorX spriteTint, in LocaleString text,
                ButtonEventHandler<T> callback, T argument, float doublePressDelay = 0, float buttonTextSplit = .33333f, float buttonTextSplitGap = .05f)
            where T : class, IWorldElement
            => builder.Button(sprite, in spriteTint, text, buttonTextSplit, buttonTextSplitGap).WithRefRelay(callback, argument, doublePressDelay);

        /// <inheritdoc cref="WithRefRelay"/>
        [Obsolete("Use WithRefRelay instead.")]
        public static TButton SetupRefRelay<TButton, TArgument>(this TButton button,
                ButtonEventHandler<TArgument> callback, TArgument argument, float doublePressDelay = 0)
            where TButton : IButton
            where TArgument : class, IWorldElement
            => button.WithRefRelay(callback, argument, doublePressDelay);

        /// <summary>
        /// Creates a <see cref="ButtonRefRelay{T}"/> with the given parameters for this button.
        /// </summary>
        /// <typeparam name="TButton">The specific type of the button.</typeparam>
        /// <typeparam name="TArgument">The type of the reference argument.</typeparam>
        /// <param name="button">The button to set up with a <see cref="ButtonRefRelay{T}"/>.</param>
        /// <param name="callback">The <see cref="ButtonRefRelay{T}.ButtonPressed">ButtonPressed</see> handler for the created relay.</param>
        /// <param name="argument">The <see cref="ButtonRefRelay{T}.Argument">Argument</see> for the created relay.</param>
        /// <param name="doublePressDelay">The <see cref="ButtonRelayBase.DoublePressDelay">DoublePressDelay</see> for the created relay.</param>
        /// <returns>The unchanged button.</returns>
        public static TButton WithRefRelay<TButton, TArgument>(this TButton button,
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

#pragma warning restore CS1572 // XML comment has a param tag, but there is no parameter by that name
#pragma warning restore CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)