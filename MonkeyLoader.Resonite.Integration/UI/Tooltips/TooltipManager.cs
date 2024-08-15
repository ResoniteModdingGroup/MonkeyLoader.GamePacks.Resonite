using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
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
    public sealed class TooltipManager : ConfiguredResoniteMonkey<TooltipManager, TooltipConfig>, ICancelableEventSource<ResolveTooltipLabelEvent>
    {
        private static readonly Dictionary<IButton, Tooltip> _openTooltips = [];

        private static CancelableEventDispatching<ResolveTooltipLabelEvent>? _resolveTooltipLabel;

        public static bool CloseTooltip(IButton button)
        {
            if (!_openTooltips.TryGetValue(button, out var tooltip))
                return false;

            tooltip.Close();
            _openTooltips.Remove(button);

            return true;
        }

        public static Tooltip MakeTooltip(IButton button, Slot parent, in float3 startingPosition, in LocaleString label)
        {
            var tooltip = new Tooltip(parent, startingPosition, label);
            _openTooltips.Add(button, tooltip);

            return tooltip;
        }

        public static bool TryOpenTooltip(IButton button, ButtonEventData buttonEventData, Slot tooltipParent, [NotNullWhen(true)] out Tooltip? tooltip)
        {
            if (_openTooltips.TryGetValue(button, out tooltip))
                return true;

            if (!TryResolveTooltipLabel(button, buttonEventData, out var label))
                return false;

            tooltip = MakeTooltip(button, tooltipParent,
                new float3(buttonEventData.localPoint.X, buttonEventData.localPoint.Y, -1 * button.World.LocalUserViewScale.Z * (.001f / tooltipParent.GlobalScale.Z)),
                label.Value);

            return true;
        }

        public static void TryOpenTooltip(IButton button, ButtonEventData buttonEventData, Slot tooltipParent)
            => TryOpenTooltip(button, buttonEventData, tooltipParent, out _);

        public static bool TryResolveTooltipLabel(IButton button, ButtonEventData buttonEventData, [NotNullWhen(true)] out LocaleString? label)
        {
            var eventData = new ResolveTooltipLabelEvent(button, buttonEventData);

            _resolveTooltipLabel?.TryInvokeAll(eventData);

            eventData.Label ??= "Test Locale Label";

            label = eventData.Label;
            return eventData.HasLabel;
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