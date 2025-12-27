using FrooxEngine.UIX;
using FrooxEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elements.Core;
using System.Runtime.CompilerServices;

namespace MonkeyLoader.Resonite.UI
{
    /// <summary>
    /// Contains extension methods to setup locally defined actions for <see cref="IButton"/>s which are triggerable by anyone.
    /// </summary>
    /// <remarks>
    /// Due to their nature, they will only work while the <see cref="User"/> that creates them hasn't left the session.<br/>
    /// As such, they will be destroyed when the local user leaves, and won't be saved either (by marking them non-persistent).
    /// </remarks>
    [TypeForwardedFrom("MonkeyLoader.Resonite.Integration")]
    public static class LocalActionButtonExtensions
    {
        /// <summary>
        /// The prefix that a <see cref="Comment"/>'s text must have to define a tooltip label.
        /// </summary>
        public const string CommentTextPrefix = "TooltipperyLabel:";

        internal static RegisterLabelForButtonFunc? RegisterLabelForButton { get; set; }

        /// <inheritdoc cref="LocalActionButton(UIBuilder, in LocaleString, in colorX?, Action{Button})"/>
        public static Button LocalActionButton(this UIBuilder builder, in LocaleString text, Action<Button> action)
            => builder.Button(text).WithLocalAction(action);

        /// <inheritdoc cref="LocalActionButton{T}(UIBuilder, in LocaleString, in colorX?, Action{Button, T}, T)"/>
        public static Button LocalActionButton<T>(this UIBuilder builder, in LocaleString text, Action<Button, T> action, T argument)
            => builder.Button(text).WithLocalAction(argument, action);

        /// <summary>
        /// Creates a <see cref="Button"/> with text, and
        /// <see cref="WithLocalAction{TButton}(TButton, Action{TButton})">adds</see>
        /// a local action to it using the given parameters.
        /// </summary>
        /// <inheritdoc cref="LocalActionButton(UIBuilder, in LocaleString, in colorX?, Action{Button})"/>
        public static Button LocalActionButton(this UIBuilder builder, in LocaleString text, in colorX? buttonTint, Action<Button> action)
            => builder.Button(text, buttonTint).WithLocalAction(action);

        /// <summary>
        /// Creates a <see cref="Button"/> with text, and
        /// <see cref="WithLocalAction{TButton, TArgument}(TButton, TArgument, Action{TButton, TArgument})">adds</see>
        /// a local action to it using the given parameters and extra <paramref name="argument"/>.
        /// </summary>
        /// <inheritdoc cref="LocalActionButton{T}(UIBuilder, IAssetProvider{Sprite}, in colorX, in LocaleString, Action{Button, T}, T, float, float)"/>
        public static Button LocalActionButton<T>(this UIBuilder builder, in LocaleString text, in colorX? buttonTint, Action<Button, T> action, T argument)
            => builder.Button(text, buttonTint).WithLocalAction(argument, action);

        /// <inheritdoc cref="LocalActionButton(UIBuilder, IAssetProvider{Sprite}, in colorX, in LocaleString, Action{Button}, float, float)"/>
        public static Button LocalActionButton(this UIBuilder builder, Uri spriteUrl, in LocaleString text, Action<Button> action)
            => builder.Button(spriteUrl, text).WithLocalAction(action);

        /// <inheritdoc cref="LocalActionButton{T}(UIBuilder, IAssetProvider{Sprite}, in colorX, in LocaleString, Action{Button, T}, T, float, float)"/>
        public static Button LocalActionButton<T>(this UIBuilder builder, Uri spriteUrl, in LocaleString text, Action<Button, T> action, T argument)
            => builder.Button(spriteUrl, text).WithLocalAction(argument, action);

        /// <inheritdoc cref="LocalActionButton(UIBuilder, IAssetProvider{Sprite}, in colorX, in LocaleString, Action{Button}, float, float)"/>
        public static Button LocalActionButton(this UIBuilder builder, Uri spriteUrl, in LocaleString text,
                in colorX buttonTint, in colorX spriteTint, Action<Button> action)
            => builder.Button(spriteUrl, text, buttonTint, spriteTint).WithLocalAction(action);

        /// <inheritdoc cref="LocalActionButton{T}(UIBuilder, IAssetProvider{Sprite}, in colorX, in LocaleString, Action{Button, T}, T, float, float)"/>
        public static Button LocalActionButton<T>(this UIBuilder builder, Uri spriteUrl, in LocaleString text,
            in colorX buttonTint, in colorX spriteTint, Action<Button, T> action, T argument)
            => builder.Button(spriteUrl, text, buttonTint, spriteTint).WithLocalAction(argument, action);

        /// <inheritdoc cref="LocalActionButton(UIBuilder, IAssetProvider{Sprite}, in colorX?, in colorX, Action{Button})"/>
        public static Button LocalActionButton(this UIBuilder builder, Uri spriteUrl, Action<Button> action)
            => builder.Button(spriteUrl).WithLocalAction(action);

        /// <inheritdoc cref="LocalActionButton{T}(UIBuilder, IAssetProvider{Sprite}, in colorX?, in colorX, Action{Button, T}, T)"/>
        public static Button LocalActionButton<T>(this UIBuilder builder, Uri spriteUrl, Action<Button, T> action, T argument)
            => builder.Button(spriteUrl).WithLocalAction(argument, action);

        /// <inheritdoc cref="LocalActionButton{T}(UIBuilder, IAssetProvider{Sprite}, in colorX?, in colorX, Action{Button, T}, T)"/>
        public static Button LocalActionButton(this UIBuilder builder, Uri spriteUrl, in colorX buttonTint, Action<Button> action)
            => builder.Button(spriteUrl, buttonTint).WithLocalAction(action);

        /// <inheritdoc cref="LocalActionButton{T}(UIBuilder, IAssetProvider{Sprite}, in colorX?, in colorX, Action{Button, T}, T)"/>
        public static Button LocalActionButton<T>(this UIBuilder builder, Uri spriteUrl, in colorX buttonTint, Action<Button, T> action, T argument)
            => builder.Button(spriteUrl, buttonTint).WithLocalAction(argument, action);

        /// <inheritdoc cref="LocalActionButton{T}(UIBuilder, IAssetProvider{Sprite}, in colorX?, in colorX, Action{Button, T}, T)"/>
        public static Button LocalActionButton(this UIBuilder builder, Uri spriteUrl,
                in colorX buttonTint, in colorX spriteTint, Action<Button> action)
            => builder.Button(spriteUrl, buttonTint, spriteTint).WithLocalAction(action);

        /// <inheritdoc cref="LocalActionButton{T}(UIBuilder, IAssetProvider{Sprite}, in colorX?, in colorX, Action{Button, T}, T)"/>
        public static Button LocalActionButton<T>(this UIBuilder builder, Uri spriteUrl,
            in colorX buttonTint, in colorX spriteTint, Action<Button, T> action, T argument)
            => builder.Button(spriteUrl, buttonTint, spriteTint).WithLocalAction(argument, action);

        /// <inheritdoc cref="LocalActionButton(UIBuilder, IAssetProvider{Sprite}, in colorX, in LocaleString, Action{Button}, float, float)"/>
        public static Button LocalActionButton(this UIBuilder builder, IAssetProvider<Sprite> sprite, in LocaleString text,
                Action<Button> action)
            => builder.Button(sprite, text).WithLocalAction(action);

        /// <inheritdoc cref="LocalActionButton{T}(UIBuilder, IAssetProvider{Sprite}, in colorX, in LocaleString, Action{Button, T}, T, float, float)"/>
        public static Button LocalActionButton<T>(this UIBuilder builder, IAssetProvider<Sprite> sprite, in LocaleString text,
                Action<Button, T> action, T argument)
            => builder.Button(sprite, text).WithLocalAction(argument, action);

        /// <summary>
        /// Creates a <see cref="Button"/> with a sprite and text, and
        /// <see cref="WithLocalAction{TButton}(TButton, Action{TButton})">adds</see>
        /// a local action to it using the given parameters.
        /// </summary>
        /// <inheritdoc cref="LocalActionButton{T}(UIBuilder, IAssetProvider{Sprite}, in colorX, in LocaleString, Action{Button, T}, T, float, float)"/>
        public static Button LocalActionButton(this UIBuilder builder, IAssetProvider<Sprite> sprite, in colorX spriteTint, in LocaleString text,
                Action<Button> action, float buttonTextSplit = .33333f, float buttonTextSplitGap = .05f)
            => builder.Button(sprite, in spriteTint, text, buttonTextSplit, buttonTextSplitGap).WithLocalAction(action);

        /// <summary>
        /// Creates a <see cref="Button"/> with a sprite and text, and
        /// <see cref="WithLocalAction{TButton, TArgument}(TButton, TArgument, Action{TButton, TArgument})">adds</see>
        /// a local action to it using the given parameters and extra <paramref name="argument"/>.
        /// </summary>
        /// <typeparam name="T">The type of the extra argument to pass to the action.</typeparam>
        /// <returns>The button created by the <see cref="UIBuilder"/>.</returns>
        /// <inheritdoc cref="WithLocalAction{TButton, TArgument}(TButton, TArgument, Action{TButton, TArgument})"/>
        /// <inheritdoc cref="ButtonRefExtensions.ButtonRef{T}(UIBuilder, IAssetProvider{Sprite}, in colorX, in LocaleString, ButtonEventHandler{T}, T, float, float, float)"/>
        public static Button LocalActionButton<T>(this UIBuilder builder, IAssetProvider<Sprite> sprite, in colorX spriteTint, in LocaleString text,
                Action<Button, T> action, T argument, float buttonTextSplit = .33333f, float buttonTextSplitGap = .05f)
            => builder.Button(sprite, in spriteTint, text, buttonTextSplit, buttonTextSplitGap).WithLocalAction(argument, action);

        /// <summary>
        /// Creates a <see cref="Button"/> with a sprite, and
        /// <see cref="WithLocalAction{TButton}(TButton, Action{TButton})">adds</see>
        /// a local action to it using the given parameters.
        /// </summary>
        /// <inheritdoc cref="LocalActionButton(UIBuilder, IAssetProvider{Sprite}, in colorX, in LocaleString, Action{Button}, float, float)"/>
        public static Button LocalActionButton(this UIBuilder builder, IAssetProvider<Sprite> sprite, in colorX? buttonTint, in colorX spriteTint, Action<Button> action)
            => builder.Button(sprite, in buttonTint, in spriteTint).WithLocalAction(action);

        /// <summary>
        /// Creates a <see cref="Button"/> with a sprite and
        /// <see cref="WithLocalAction{TButton, TArgument}(TButton, TArgument, Action{TButton, TArgument})">adds</see>
        /// a local action to it using the given parameters and extra <paramref name="argument"/>.
        /// </summary>
        /// <inheritdoc cref="LocalActionButton{T}(UIBuilder, IAssetProvider{Sprite}, in colorX, in LocaleString, Action{Button, T}, T, float, float)"/>
        public static Button LocalActionButton<T>(this UIBuilder builder, IAssetProvider<Sprite> sprite, in colorX? buttonTint, in colorX spriteTint,
                Action<Button, T> action, T argument)
            => builder.Button(sprite, in buttonTint, in spriteTint).WithLocalAction(argument, action);

        /// <summary>
        /// Sets up an <see cref="IButton"/> with the given <paramref name="action"/>.
        /// </summary>
        /// <inheritdoc cref="WithLocalAction{TButton, TArgument}(TButton, TArgument, Action{TButton, TArgument})"/>
        public static TButton WithLocalAction<TButton>(this TButton button, Action<TButton> action)
            where TButton : IButton
        {
            var valueField = button.Slot.AttachComponent<ValueField<bool>>().Value;
            valueField.OnValueChange += field => action(button);

            var toggle = button.Slot.AttachComponent<ButtonToggle>();
            toggle.TargetValue.Target = valueField;

            button.Slot.DestroyWhenLocalUserLeaves();

            return button;
        }

        /// <summary>
        /// Sets up an <see cref="IButton"/> with the given <paramref name="action"/> and extra <paramref name="argument"/>.
        /// </summary>
        /// <remarks>
        /// The <paramref name="action"/> will be triggerable by anyone, as long as the creating <see cref="User"/> hasn't left the session.<br/>
        /// As such, it will be destroyed when the local user leaves, and won't be saved either (by marking the button's slot non-persistent).
        /// </remarks>
        /// <typeparam name="TButton">The specific type of the button.</typeparam>
        /// <typeparam name="TArgument">The type of the extra argument to pass to the action.</typeparam>
        /// <param name="button">The button to set up with an action.</param>
        /// <param name="argument">The extra argument to pass to the action when this button is pressed.</param>
        /// <param name="action">The action to run when pressed.</param>
        /// <returns>The unchanged button.</returns>
        public static TButton WithLocalAction<TButton, TArgument>(this TButton button, TArgument argument, Action<TButton, TArgument> action)
            where TButton : IButton
        {
            var valueField = button.Slot.AttachComponent<ValueField<bool>>().Value;
            valueField.OnValueChange += field => action(button, argument);

            var toggle = button.Slot.AttachComponent<ButtonToggle>();
            toggle.TargetValue.Target = valueField;

            button.Slot.DestroyWhenLocalUserLeaves();

            return button;
        }

        /// <summary>
        /// Sets up a local-only <see cref="IButton"/> with the given <see cref="LocaleString"/> as a tooltip
        /// by registering it with the TooltipManager.
        /// </summary>
        /// <remarks>
        /// This is the preferred overload to support localization on local-only buttons.<br/>
        /// If a non-locale-key <see cref="LocaleString"/> is passed in, the bare string is used as the label.
        /// </remarks>
        /// <typeparam name="TButton">The specific type of the button.</typeparam>
        /// <param name="button">The local-only button to set up with a tooltip.</param>
        /// <param name="label">The localized label to display as a tooltip.</param>
        /// <returns>The unchanged button.</returns>
        /// <exception cref="InvalidOperationException">When the button is not local to the user.</exception>
        public static TButton WithLocalTooltip<TButton>(this TButton button, LocaleString label)
            where TButton : IButton
        {
            if (!button.Slot.IsLocalElement && !button.World.IsUserspace())
                throw new InvalidOperationException("The button to label must be on a local slot or in userspace!");

            RegisterLabelForButton?.Invoke(button, label);

            return button;
        }

        /// <summary>
        /// Sets up an <see cref="IButton"/> with the given label as a tooltip.
        /// </summary>
        /// <remarks>
        /// Prefer using the overload taking a <see cref="LocaleString"/> to support localization.
        /// </remarks>
        /// <inheritdoc cref="WithTooltip{TButton}(TButton, LocaleString)"/>
        public static TButton WithTooltip<TButton>(this TButton button, string label)
            where TButton : IButton
        {
            var comment = button.Slot.AttachComponent<Comment>();
            comment.Text.Value = CommentTextPrefix + label;

            RegisterLabelForButton?.Invoke(button, label);

            return button;
        }

        /// <summary>
        /// Sets up an <see cref="IButton"/> with the given <see cref="LocaleString"/> as a tooltip.
        /// </summary>
        /// <remarks>
        /// This is the preferred overload to support localization.<br/>
        /// If a non-locale-key <see cref="LocaleString"/> is passed in, the <c>string</c> overload is used.
        /// </remarks>
        /// <typeparam name="TButton">The specific type of the button.</typeparam>
        /// <param name="button">The button to set up with a tooltip.</param>
        /// <param name="label">The localized label to display as a tooltip.</param>
        /// <returns>The unchanged button.</returns>
        public static TButton WithTooltip<TButton>(this TButton button, LocaleString label)
            where TButton : IButton
        {
            if (!label.isLocaleKey)
                return button.WithTooltip(label.content);

            RegisterLabelForButton?.Invoke(button, label);

            return button.WithTooltip(field => field.AssignLocaleString(label));
        }

        /// <summary>
        /// Sets up an <see cref="IButton"/> with a <see cref="StringConcatenationDriver"/>
        /// which concatenates the tooltip prefix and as many strings as necessary for <paramref name="setupFields"/>.
        /// </summary>
        /// <remarks>
        /// Primarily a helper method for the <see cref="LocaleString"/> overload.<br/>
        /// Try to keep localization in mind (i.e. by using
        /// <see cref="FrooxEngine.LocaleHelper.AssignLocaleString"/> to set up fields).
        /// </remarks>
        /// <typeparam name="TButton">The specific type of the button.</typeparam>
        /// <param name="button">The button to set up with a tooltip.</param>
        /// <param name="setupFields">Actions to set up additional string fields to be concatenated together.</param>
        /// <returns>The unchanged button.</returns>
        public static TButton WithTooltip<TButton>(this TButton button, params Action<IField<string>>[] setupFields)
            where TButton : IButton
        {
            var stringConcat = button.Slot.AttachComponent<StringConcatenationDriver>();
            stringConcat.Strings.Add(CommentTextPrefix);

            var comment = button.Slot.AttachComponent<Comment>();
            stringConcat.TargetString.Target = comment.Text;

            foreach (var setupField in setupFields)
                setupField(stringConcat.Strings.Add());

            return button;
        }

        /// <summary>
        /// Registers a <see cref="LocaleString">label</see> for the given undestroyed button,
        /// if it doesn't already have one.
        /// </summary>
        /// <param name="button">The undestroyed button to register a label for.</param>
        /// <param name="label">The label to register for the button.</param>
        /// <returns><see langword="true"/> if the label was registered for the button; otherwise, <see langword="false"/>.</returns>
        internal delegate bool RegisterLabelForButtonFunc(IButton button, in LocaleString label);
    }
}