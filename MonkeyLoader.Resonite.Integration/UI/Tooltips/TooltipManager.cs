using Elements.Core;
using EnumerableToolkit;
using FrooxEngine;
using MonkeyLoader.Events;
using MonkeyLoader.Patching;
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
    public sealed class TooltipManager : ConfiguredResoniteMonkey<TooltipManager, TooltipConfig>, ICancelableEventSource<ResolveTooltipLabelEvent>
    {
        private static readonly Dictionary<IButton, Tooltip> _openTooltips = [];

        private static CancelableEventDispatching<ResolveTooltipLabelEvent>? _resolveTooltipLabel;

        /// <summary>
        /// Closes the tooltip for the given button.
        /// </summary>
        /// <param name="button">The button the tooltip is attached to.</param>
        /// <returns><c>true</c> if the tooltip was closed; <c>false</c> if there was no tooltip.</returns>
        public static bool CloseTooltip(IButton button)
        {
            if (!TryGetTooltip(button, out var tooltip))
                return false;

            tooltip.Close();
            _openTooltips.Remove(button);

            return true;
        }

        /// <summary>
        /// Checks if there is a <see cref="Tooltip"/> associated with this <see cref="IButton"/>.
        /// </summary>
        /// <param name="button">The button the tooltip is attached to.</param>
        /// <returns><c>true</c> if a <see cref="Tooltip"/> for this <see cref="IButton"/> was found; otherwise, <c>false</c>.</returns>
        public static bool HasTooltip(IButton button)
            => _openTooltips.ContainsKey(button);

        /// <summary>
        /// Creates a tooltip according to the given parameters for the given button.<br/>
        /// Any existing open tooltip will be closed.
        /// </summary>
        /// <param name="button">The button the tooltip is attached to.</param>
        /// <param name="parent">The slot the tooltip will be parented to.</param>
        /// <param name="startingPosition">The position that the button starts at inside its parent slot.</param>
        /// <param name="label">The label that should be shown on the tooltip.</param>
        /// <returns>The data for the newly created tooltip.</returns>
        public static Tooltip MakeTooltip(IButton button, Slot parent, in float3 startingPosition, in LocaleString label)
        {
            CloseTooltip(button);

            var tooltip = new Tooltip(parent, startingPosition, label);
            _openTooltips.Add(button, tooltip);

            return tooltip;
        }

        /// <summary>
        /// Tries to get the <see cref="Tooltip"/> associated with this <see cref="IButton"/>.
        /// </summary>
        /// <param name="button">The button the tooltip is attached to.</param>
        /// <param name="tooltip">The associated <see cref="Tooltip"/> if one is open; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if a <see cref="Tooltip"/> for this <see cref="IButton"/> was found; otherwise, <c>false</c>.</returns>
        public static bool TryGetTooltip(IButton button, [NotNullWhen(true)] out Tooltip? tooltip)
            => _openTooltips.TryGetValue(button, out tooltip);

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
        /// <returns><c>true</c> if a tooltip was already open or newly opened; otherwise, <c>false</c>.</returns>
        public static bool TryOpenTooltip(IButton button, ButtonEventData buttonEventData, Slot tooltipParent, [NotNullWhen(true)] out Tooltip? tooltip, in float3 globalOffset = default)
        {
            if (_openTooltips.TryGetValue(button, out tooltip))
                return true;

            if (!TryResolveTooltipLabel(button, buttonEventData, out var label))
                return false;

            var position = buttonEventData.globalPoint + globalOffset;

            tooltip = MakeTooltip(button, tooltipParent,
                tooltipParent.GlobalPointToLocal(position),// new float3(buttonEventData.localPoint.X, buttonEventData.localPoint.Y, -1 * button.World.LocalUserViewScale.Z * (.001f / tooltipParent.GlobalScale.Z)),
                label.Value);

            return true;
        }

        /// <summary>
        /// Tries to open a tooltip for the given button details.
        /// </summary>
        /// <remarks>
        /// If the label for the tooltip fails to <see cref="TryResolveTooltipLabel">resolve</see>
        /// or it's a locale key missing a message, no tooltip will be created.
        /// </remarks>
        /// <param name="button">The button the tooltip is attached to.</param>
        /// <param name="buttonEventData">The button event triggering opening the tooltip.</param>
        /// <param name="tooltipParent">The slot the tooltip will be parented to.</param>
        /// <param name="globalOffset">The offset of the top-mi</param>
        /// <returns><c>true</c> if a tooltip was already open or newly opened; otherwise, <c>false</c>.</returns>
        public static bool TryOpenTooltip(IButton button, ButtonEventData buttonEventData, Slot tooltipParent, in float3 globalOffset = default)
            => TryOpenTooltip(button, buttonEventData, tooltipParent, out _, in globalOffset);

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
            return eventData.HasLabel && (!label.Value.isLocaleKey || label.Value.HasMessageInCurrent() || TooltipConfig.Instance.EnableDebugButtonData);
        }

        /// <inheritdoc/>
        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => [];

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

        event CancelableEventDispatching<ResolveTooltipLabelEvent>? ICancelableEventSource<ResolveTooltipLabelEvent>.Dispatching
        {
            add => _resolveTooltipLabel += value;
            remove => _resolveTooltipLabel -= value;
        }
    }
}