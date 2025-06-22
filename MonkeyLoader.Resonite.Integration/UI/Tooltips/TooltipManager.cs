using Elements.Core;
using EnumerableToolkit;
using FrooxEngine;
using MonkeyLoader.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.UI.Tooltips
{
    /// <summary>
    /// Handles resolving the labels as well as opening and closing the tooltips for <see cref="IButton"/>s.
    /// </summary>
    public sealed class TooltipManager
        : ConfiguredResoniteCancelableEventHandlerMonkey<TooltipManager, TooltipConfig, ResolveTooltipLabelEvent>,
          ICancelableEventSource<ResolveTooltipLabelEvent>
    {
        private static readonly Dictionary<IButton, LocaleString> _labelsByButton = [];
        private static readonly Dictionary<IButton, Tooltip> _openTooltips = [];

        private static CancelableEventDispatching<ResolveTooltipLabelEvent>? _resolveTooltipLabel;

        /// <inheritdoc/>
        public override int Priority => HarmonyLib.Priority.First;

        /// <inheritdoc/>
        public override bool SkipCanceled => true;

        /// <summary>
        /// Closes the tooltip for the given button.
        /// </summary>
        /// <param name="button">The button the tooltip is attached to.</param>
        /// <returns><c>true</c> if the tooltip was closed; <c>false</c> if there was no tooltip.</returns>
        public static bool CloseTooltip(IButton button)
        {
            if (!TryGetTooltip(button, out var tooltip))
                return false;

            if (ConfigSection.EnableDebugTooltipStay)
                return true;

            tooltip.Close();
            _openTooltips.Remove(button);

            return true;
        }

        /// <summary>
        /// Checks whether the given button has a <see cref="RegisterLabelForButton">registered</see> <see cref="LocaleString">label</see>.
        /// </summary>
        /// <param name="button">The button to check for a registered label.</param>
        /// <returns><c>true</c> if the button has a registered label; otherwise, <c>false</c>.</returns>
        public static bool HasLabelForButton(IButton button)
            => _labelsByButton.ContainsKey(button);

        /// <summary>
        /// Checks if there is a <see cref="Tooltip"/> associated with this <see cref="IButton"/>.
        /// </summary>
        /// <param name="button">The button the tooltip is attached to.</param>
        /// <returns><c>true</c> if a <see cref="Tooltip"/> for this <see cref="IButton"/> was found; otherwise, <c>false</c>.</returns>
        public static bool HasTooltip(IButton button)
            => _openTooltips.ContainsKey(button);

        /// <inheritdoc cref="MakeTooltip(IButton, Slot, in float3, float, in LocaleString)"/>
        public static Tooltip MakeTooltip(IButton button, Slot parent, in float3 localPosition, in LocaleString label)
            => MakeTooltip(button, parent, localPosition, 1, label);

        /// <summary>
        /// Creates a tooltip according to the given parameters for the given button.<br/>
        /// Any already existing open tooltip will be closed.
        /// </summary>
        /// <param name="button">The button the tooltip is attached to.</param>
        /// <param name="parent">The slot the tooltip will be parented to.</param>
        /// <param name="localPosition">The position of the tooltip inside its <paramref name="parent"/>.</param>
        /// <param name="localScale">The scale of the tooltip inside its <paramref name="parent"/>.</param>
        /// <param name="label">The label that should be shown on the tooltip.</param>
        /// <returns>The data for the newly created tooltip.</returns>
        public static Tooltip MakeTooltip(IButton button, Slot parent, in float3 localPosition, float localScale, in LocaleString label)
        {
            CloseTooltip(button);

            var tooltip = new Tooltip(parent, localPosition, localScale, label);
            _openTooltips.Add(button, tooltip);

            return tooltip;
        }

        /// <summary>
        /// Registers a <see cref="LocaleString">label</see> for the given undestroyed button,
        /// if it doesn't already <see cref="HasLabelForButton">have</see> one.
        /// </summary>
        /// <param name="button">The undestroyed button to register a label for.</param>
        /// <param name="label">The label to register for the button.</param>
        /// <returns><c>true</c> if the label was registered for the button; otherwise, <c>false</c>.</returns>
        public static bool RegisterLabelForButton(IButton button, in LocaleString label)
        {
            if (button.FilterWorldElement() is null || _labelsByButton.ContainsKey(button))
                return false;

            _labelsByButton.Add(button, label);
            button.Destroyed += OnLabeledButtonDestroyed;

            return true;
        }

        /// <summary>
        /// Tries to get the <see cref="RegisterLabelForButton">registered</see>
        /// <see cref="LocaleString">label</see> for the given button.
        /// </summary>
        /// <param name="button">The button to get the registered label for.</param>
        /// <param name="label">The registered label for the button if it has one; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if the button has a registered label; otherwise, <c>false</c>.</returns>
        public static bool TryGetLabelForButton(IButton button, [NotNullWhen(true)] out LocaleString? label)
        {
            label = null;

            if (!_labelsByButton.TryGetValue(button, out var possibleLabel))
                return false;

            label = possibleLabel;
            return true;
        }

        /// <summary>
        /// Tries to get the <see cref="Tooltip"/> associated with this <see cref="IButton"/>.
        /// </summary>
        /// <param name="button">The button the tooltip is attached to.</param>
        /// <param name="tooltip">The associated <see cref="Tooltip"/> if one is open; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if a <see cref="Tooltip"/> for this <see cref="IButton"/> was found; otherwise, <c>false</c>.</returns>
        public static bool TryGetTooltip(IButton button, [NotNullWhen(true)] out Tooltip? tooltip)
            => _openTooltips.TryGetValue(button, out tooltip);

        /// <inheritdoc cref="TryOpenTooltip(IButton, ButtonEventData, Slot, out Tooltip?, in float3, float)"/>
        public static bool TryOpenTooltip(IButton button, ButtonEventData buttonEventData, Slot tooltipParent,
            [NotNullWhen(true)] out Tooltip? tooltip, in float3 globalOffset = default)
            => TryOpenTooltip(button, buttonEventData, tooltipParent, out tooltip, globalOffset);

        /// <summary>
        /// Tries to open a tooltip for the given button details,
        /// optionally returning the already open or newly created tooltip.
        /// </summary>
        /// <remarks>
        /// If the label for the tooltip fails to <see cref="TryResolveTooltipLabel">resolve</see>
        /// or it's a locale key missing a message, no tooltip will be created.
        /// </remarks>
        /// <param name="button">The button the tooltip is attached to.</param>
        /// <param name="buttonEventData">The button event triggering opening the tooltip.</param>
        /// <param name="tooltipParent">The slot the tooltip will be parented to.</param>
        /// <param name="tooltip">The tooltip if one was already open or newly opened; otherwise, <c>null</c>.</param>
        /// <param name="globalOffset">The offset from the button hitpoint at which the tooltip should be opened in global space.</param>
        /// <param name="localScale">The local scale of the tooltip to be opened inside the <paramref name="tooltipParent"/>.</param>
        /// <returns><c>true</c> if a tooltip was already open or newly opened; otherwise, <c>false</c>.</returns>
        public static bool TryOpenTooltip(IButton button, ButtonEventData buttonEventData, Slot tooltipParent,
            [NotNullWhen(true)] out Tooltip? tooltip, in float3 globalOffset = default, float localScale = 1)
        {
            if (_openTooltips.TryGetValue(button, out tooltip))
                return true;

            if (!TryResolveTooltipLabel(button, buttonEventData, out var label))
                return false;

            var position = buttonEventData.globalPoint + globalOffset;

            tooltip = MakeTooltip(button, tooltipParent,
                tooltipParent.GlobalPointToLocal(position),
                localScale, label.Value);

            return true;
        }

        /// <inheritdoc cref="TryOpenTooltip(IButton, ButtonEventData, Slot, in float3, float)"/>
        public static bool TryOpenTooltip(IButton button, ButtonEventData buttonEventData, Slot tooltipParent, in float3 globalOffset = default)
            => TryOpenTooltip(button, buttonEventData, tooltipParent, out _, in globalOffset, 1);

        /// <summary>
        /// Tries to open a tooltip for the given button details.
        /// </summary>
        /// <inheritdoc cref="TryOpenTooltip(IButton, ButtonEventData, Slot, out Tooltip?, in float3, float)"/>
        public static bool TryOpenTooltip(IButton button, ButtonEventData buttonEventData, Slot tooltipParent, in float3 globalOffset = default, float localScale = 1)
            => TryOpenTooltip(button, buttonEventData, tooltipParent, out _, in globalOffset, localScale);

        /// <summary>
        /// Tries to resolve the tooltip label for the given button details.
        /// </summary>
        /// <remarks>
        /// If the label for the tooltip is locale key missing a message, it's treated as an unsuccessfully resolved.
        /// </remarks>
        /// <param name="button">The button the tooltip is attached to.</param>
        /// <param name="buttonEventData">The button event triggering opening the tooltip.</param>
        /// <param name="label">The label if it was successfully resolved; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if the label was successfully resolved; otherwise, <c>false</c>.</returns>
        public static bool TryResolveTooltipLabel(IButton button, ButtonEventData buttonEventData, [NotNullWhen(true)] out LocaleString? label)
        {
            var eventData = new ResolveTooltipLabelEvent(button, buttonEventData);

            _resolveTooltipLabel?.TryInvokeAll(eventData);

            label = eventData.Label!;
            if (!eventData.HasLabel)
                return false;

            if (!label.Value.isLocaleKey || label.Value.HasMessageInCurrent())
            {
                RegisterLabelForButton(button, label.Value);
                return true;
            }

            return ConfigSection.EnableDebugButtonData;
        }

        /// <summary>
        /// Removes the <see cref="RegisterLabelForButton">registered</see>
        /// <see cref="LocaleString">label</see> for the given button.
        /// </summary>
        /// <param name="button">The button to remove the registered label of.</param>
        /// <returns><c>true</c> if there was a label to remove; otherwise, <c>false</c>.</returns>
        public static bool UnregisterLabelForButton(IButton button)
        {
            if (!_labelsByButton.Remove(button))
                return false;

            button.Destroyed -= OnLabeledButtonDestroyed;

            return true;
        }

        /// <inheritdoc/>
        protected override void Handle(ResolveTooltipLabelEvent eventData)
        {
            if (TryGetLabelForButton(eventData.Button, out var label))
                eventData.Label = label;
        }

        /// <inheritdoc/>
        protected override bool OnEngineReady()
        {
            Mod.RegisterEventSource(this);

            return base.OnEngineReady();
        }

        /// <inheritdoc/>
        protected override bool OnShutdown(bool applicationExiting)
        {
            if (!applicationExiting)
                Mod.UnregisterEventSource(this);

            return base.OnShutdown(applicationExiting);
        }

        private static void OnLabeledButtonDestroyed(IDestroyable destroyable)
            => UnregisterLabelForButton((IButton)destroyable);

        event CancelableEventDispatching<ResolveTooltipLabelEvent>? ICancelableEventSource<ResolveTooltipLabelEvent>.Dispatching
        {
            add => _resolveTooltipLabel += value;
            remove => _resolveTooltipLabel -= value;
        }
    }
}