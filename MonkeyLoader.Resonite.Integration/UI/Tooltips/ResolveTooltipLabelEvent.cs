using Elements.Core;
using FrooxEngine;
using MonkeyLoader.Events;
using System.Diagnostics.CodeAnalysis;

namespace MonkeyLoader.Resonite.UI
{
    /// <summary>
    /// Represents the event fired to determine a tooltip label
    /// when a <see cref="IButton">button</see> is hovered.
    /// </summary>
    public sealed class ResolveTooltipLabelEvent : CancelableSyncEvent
    {
        private LocaleString? _label;

        /// <summary>
        /// Gets the button that the label needs to be determined for.
        /// </summary>
        public IButton Button { get; }

        /// <summary>
        /// Gets the event data for the hover event that triggered this tooltip label resolving event.
        /// </summary>
        public ButtonEventData ButtonEventData { get; }

        /// <summary>
        /// Gets whether a tooltip <see cref="Label">label</see>
        /// has already been set for this <see cref="Button">button</see>.
        /// </summary>
        [MemberNotNullWhen(true, nameof(_label), nameof(Label))]
        public bool HasLabel => _label.HasValue;

        /// <summary>
        /// Gets or sets the tooltip label for this <see cref="Button">button</see>.
        /// </summary>
        /// <remarks>
        /// Automatically sets <c><see cref="CancelableSyncEvent.Canceled">Canceled</see> = true</c>
        /// when set to a non-null value for the first time.
        /// </remarks>
        public LocaleString? Label
        {
            get => _label;
            set
            {
                _label = value;
                Canceled |= value.HasValue;
            }
        }

        internal ResolveTooltipLabelEvent(IButton button, ButtonEventData buttonEventData)
        {
            Button = button;
            ButtonEventData = buttonEventData;
        }
    }
}