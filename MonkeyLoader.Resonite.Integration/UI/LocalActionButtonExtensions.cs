using FrooxEngine.UIX;
using FrooxEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elements.Core;
using MonkeyLoader.Resonite.UI.Tooltips;

namespace MonkeyLoader.Resonite.UI
{
    /// <summary>
    /// Contains extension methods to setup locally defined actions for <see cref="IButton"/>s which are triggerable by anyone.<br/>
    /// Due to their nature, they will only work while the <see cref="User"/> that creates them hasn't left the session.
    /// As such, they will be destroyed when the local user leaves, and won't be saved either (marking it non-persistent).
    /// </summary>
    public static class LocalActionButtonExtensions
    {
        /// <summary>
        /// Creates a <see cref="Button"/> using the given <paramref name="text"/> and <paramref name="action"/>.<br/>
        /// The <paramref name="action"/> will be triggerable by anyone, as long as the creating <see cref="User"/> hasn't left the session.
        /// As such, it will be destroyed when the local user leaves, and won't be saved either (marking it non-persistent).
        /// </summary>
        /// <param name="builder">The builder to use for creating the button.</param>
        /// <param name="text">The text displayed on the button.</param>
        /// <param name="action">The action to run when pressed.</param>
        /// <returns>The button created by the <see cref="UIBuilder"/>.</returns>
        public static Button LocalActionButton(this UIBuilder builder, in LocaleString text, Action<Button> action)
            => builder.Button(text).WithLocalAction(action);

        /// <summary>
        /// Creates a <see cref="Button"/> using the given <paramref name="icon"/> and <paramref name="action"/>.<br/>
        /// The <paramref name="action"/> will be triggerable by anyone, as long as the creating <see cref="User"/> hasn't left the session.
        /// As such, it will be destroyed when the local user leaves, and won't be saved either (marking it non-persistent).
        /// </summary>
        /// <param name="builder">The builder to use for creating the button.</param>
        /// <param name="icon">The icon displayed on the button.</param>
        /// <param name="action">The action to run when pressed.</param>
        /// <returns>The button created by the <see cref="UIBuilder"/>.</returns>
        public static Button LocalActionButton(this UIBuilder builder, Uri icon, Action<Button> action)
            => builder.Button(icon).WithLocalAction(action);

        /// <summary>
        /// Creates a <see cref="Button"/> using the given <paramref name="text"/>,
        /// <paramref name="icon"/> and <paramref name="action"/>.<br/>
        /// The <paramref name="action"/> will be triggerable by anyone, as long as the creating <see cref="User"/> hasn't left the session.
        /// As such, it will be destroyed when the local user leaves, and won't be saved either (marking it non-persistent).
        /// </summary>
        /// <param name="builder">The builder to use for creating the button.</param>
        /// <param name="icon">The icon displayed on the button.</param>
        /// <param name="text">The text displayed on the button.</param>
        /// <param name="action">The action to run when pressed.</param>
        /// <returns>The button created by the <see cref="UIBuilder"/>.</returns>
        public static Button LocalActionButton(this UIBuilder builder, Uri icon, LocaleString text, Action<Button> action)
            => builder.Button(icon, text).WithLocalAction(action);

        /// <summary>
        /// Creates a <see cref="Button"/> using the given <paramref name="text"/>,
        /// <paramref name="icon"/>, tints and <paramref name="action"/>.<br/>
        /// The <paramref name="action"/> will be triggerable by anyone, as long as the creating <see cref="User"/> hasn't left the session.
        /// As such, it will be destroyed when the local user leaves, and won't be saved either (marking it non-persistent).
        /// </summary>
        /// <param name="builder">The builder to use for creating the button.</param>
        /// <param name="icon">The icon displayed on the button.</param>
        /// <param name="text">The text displayed on the button.</param>
        /// <param name="tint">The background color of the button.</param>
        /// <param name="spriteTint">The tint of the icon.</param>
        /// <param name="action">The action to run when pressed.</param>
        /// <returns>The button created by the <see cref="UIBuilder"/>.</returns>
        public static Button LocalActionButton(this UIBuilder builder, Uri icon, LocaleString text,
            in colorX tint, in colorX spriteTint, Action<Button> action)
            => builder.Button(icon, text, tint, spriteTint).WithLocalAction(action);

        /// <summary>
        /// Creates a <see cref="Button"/> using the given <paramref name="text"/> and <paramref name="action"/> with an extra <paramref name="argument"/>.<br/>
        /// The <paramref name="action"/> will be triggerable by anyone, as long as the creating <see cref="User"/> hasn't left the session.
        /// As such, it will be destroyed when the local user leaves, and won't be saved either (marking it non-persistent).
        /// </summary>
        /// <typeparam name="T">The type of the extra argument to pass to the action.</typeparam>
        /// <param name="builder">The builder to use for creating the button.</param>
        /// <param name="text">The text displayed on the button.</param>
        /// <param name="action">The action to run when pressed.</param>
        /// <param name="argument">The extra argument to pass to the action when this button is pressed.</param>
        /// <returns>The button created by the <see cref="UIBuilder"/>.</returns>
        public static Button LocalActionButton<T>(this UIBuilder builder, in LocaleString text, Action<Button, T> action, T argument)
            => builder.Button(text).WithLocalAction(argument, action);

        /// <summary>
        /// Creates a <see cref="Button"/> using the given <paramref name="icon"/> and <paramref name="action"/> with an extra <paramref name="argument"/>.<br/>
        /// The <paramref name="action"/> will be triggerable by anyone, as long as the creating <see cref="User"/> hasn't left the session.
        /// As such, it will be destroyed when the local user leaves, and won't be saved either (marking it non-persistent).
        /// </summary>
        /// <typeparam name="T">The type of the extra argument to pass to the action.</typeparam>
        /// <param name="builder">The builder to use for creating the button.</param>
        /// <param name="icon">The icon displayed on the button.</param>
        /// <param name="action">The action to run when pressed.</param>
        /// <param name="argument">The extra argument to pass to the action when this button is pressed.</param>
        /// <returns>The button created by the <see cref="UIBuilder"/>.</returns>
        public static Button LocalActionButton<T>(this UIBuilder builder, Uri icon, Action<Button, T> action, T argument)
            => builder.Button(icon).WithLocalAction(argument, action);

        /// <summary>
        /// Creates a <see cref="Button"/> using the given <paramref name="text"/>,
        /// <paramref name="icon"/> and <paramref name="action"/> with an extra <paramref name="argument"/>.<br/>
        /// The <paramref name="action"/> will be triggerable by anyone, as long as the creating <see cref="User"/> hasn't left the session.
        /// As such, it will be destroyed when the local user leaves, and won't be saved either (marking it non-persistent).
        /// </summary>
        /// <typeparam name="T">The type of the extra argument to pass to the action.</typeparam>
        /// <param name="builder">The builder to use for creating the button.</param>
        /// <param name="icon">The icon displayed on the button.</param>
        /// <param name="text">The text displayed on the button.</param>
        /// <param name="action">The action to run when pressed.</param>
        /// <param name="argument">The extra argument to pass to the action when this button is pressed.</param>
        /// <returns>The button created by the <see cref="UIBuilder"/>.</returns>
        public static Button LocalActionButton<T>(this UIBuilder builder, Uri icon, LocaleString text, Action<Button, T> action, T argument)
            => builder.Button(icon, text).WithLocalAction(argument, action);

        /// <summary>
        /// Creates a <see cref="Button"/> using the given <paramref name="text"/>,
        /// <paramref name="icon"/>, tints and <paramref name="action"/> with an extra <paramref name="argument"/>.<br/>
        /// The <paramref name="action"/> will be triggerable by anyone, as long as the creating <see cref="User"/> hasn't left the session.
        /// As such, it will be destroyed when the local user leaves, and won't be saved either (marking it non-persistent).
        /// </summary>
        /// <typeparam name="T">The type of the extra argument to pass to the action.</typeparam>
        /// <param name="builder">The builder to use for creating the button.</param>
        /// <param name="icon">The icon displayed on the button.</param>
        /// <param name="text">The text displayed on the button.</param>
        /// <param name="tint">The background color of the button.</param>
        /// <param name="spriteTint">The tint of the icon.</param>
        /// <param name="action">The action to run when pressed.</param>
        /// <param name="argument">The extra argument to pass to the action when this button is pressed.</param>
        /// <returns>The button created by the <see cref="UIBuilder"/>.</returns>
        public static Button LocalActionButton<T>(this UIBuilder builder, Uri icon, LocaleString text,
            in colorX tint, in colorX spriteTint, Action<Button, T> action, T argument)
            => builder.Button(icon, text, tint, spriteTint).WithLocalAction(argument, action);

        /// <summary>
        /// Sets up an <see cref="IButton"/> with the given <paramref name="action"/> and extra <paramref name="argument"/>.<br/>
        /// The <paramref name="action"/> will be triggerable by anyone, as long as the creating <see cref="User"/> hasn't left the session.
        /// As such, it will be destroyed when the local user leaves, and won't be saved either (marking it non-persistent).
        /// </summary>
        /// <typeparam name="TButton">The specific type of the button.</typeparam>
        /// <typeparam name="TArgument">The type of the extra argument to pass to the action.</typeparam>
        /// <param name="button">The button to set up with an action.</param>
        /// <param name="argument">The extra argument to pass to the action when this button is pressed.</param>
        /// <param name="action">The action to run when pressed.</param>
        /// <returns>The unchanged button.</returns>
        [Obsolete("Use WithLocalAction instead.")]
        public static TButton SetupLocalAction<TButton, TArgument>(this TButton button, TArgument argument, Action<TButton, TArgument> action)
            where TButton : IButton
            => button.WithLocalAction(argument, action);

        /// <summary>
        /// Sets up an <see cref="IButton"/> with the given <paramref name="action"/>.<br/>
        /// The <paramref name="action"/> will be triggerable by anyone, as long as the creating <see cref="User"/> hasn't left the session.
        /// As such, it will be destroyed when the local user leaves, and won't be saved either (marking it non-persistent).
        /// </summary>
        /// <typeparam name="TButton">The specific type of the button.</typeparam>
        /// <param name="button">The button to set up with an action.</param>
        /// <param name="action">The action to run when pressed.</param>
        /// <returns>The unchanged button.</returns>
        [Obsolete("Use WithLocalAction instead.")]
        public static TButton SetupLocalAction<TButton>(this TButton button, Action<TButton> action)
            where TButton : IButton
            => button.WithLocalAction(action);

        /// <summary>
        /// Sets up an <see cref="IButton"/> with the given <paramref name="action"/>.<br/>
        /// The <paramref name="action"/> will be triggerable by anyone, as long as the creating <see cref="User"/> hasn't left the session.
        /// As such, it will be destroyed when the local user leaves, and won't be saved either (marking it non-persistent).
        /// </summary>
        /// <typeparam name="TButton">The specific type of the button.</typeparam>
        /// <param name="button">The button to set up with an action.</param>
        /// <param name="action">The action to run when pressed.</param>
        /// <returns>The unchanged button.</returns>
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
        /// Sets up an <see cref="IButton"/> with the given <paramref name="action"/> and extra <paramref name="argument"/>.<br/>
        /// The <paramref name="action"/> will be triggerable by anyone, as long as the creating <see cref="User"/> hasn't left the session.
        /// As such, it will be destroyed when the local user leaves, and won't be saved either (marking it non-persistent).
        /// </summary>
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
        /// by <see cref="TooltipManager.RegisterLabelForButton">registering</see> it with the <see cref="TooltipManager"/>.
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

            TooltipManager.RegisterLabelForButton(button, label);
            return button;
        }

        /// <summary>
        /// Sets up an <see cref="IButton"/> with the given label as a tooltip.
        /// </summary>
        /// <remarks>
        /// Prefer using the overload taking a <see cref="LocaleString"/> to support localization.
        /// </remarks>
        /// <typeparam name="TButton">The specific type of the button.</typeparam>
        /// <param name="button">The button to set up with a tooltip.</param>
        /// <param name="label">The label to display as a tooltip.</param>
        /// <returns>The unchanged button.</returns>
        public static TButton WithTooltip<TButton>(this TButton button, string label)
            where TButton : IButton
        {
            var comment = button.Slot.AttachComponent<Comment>();
            comment.Text.Value = CommentTooltipResolver.CommentTextPrefix + label;

            TooltipManager.RegisterLabelForButton(button, label);

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

            TooltipManager.RegisterLabelForButton(button, label);

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
            stringConcat.Strings.Add(CommentTooltipResolver.CommentTextPrefix);

            var comment = button.Slot.AttachComponent<Comment>();
            stringConcat.TargetString.Target = comment.Text;

            foreach (var setupField in setupFields)
                setupField(stringConcat.Strings.Add());

            return button;
        }
    }
}