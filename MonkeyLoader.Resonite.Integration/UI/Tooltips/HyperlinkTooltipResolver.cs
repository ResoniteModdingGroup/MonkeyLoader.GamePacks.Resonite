using Elements.Core;
using FrooxEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.UI.Tooltips
{
    /// <summary>
    /// Handles resolving the tooltip for <see cref="IButton">buttons</see> with a <see cref="Hyperlink.Reason"/>.
    /// </summary>
    public sealed class HyperlinkTooltipResolver : ResoniteCancelableEventHandlerMonkey<HyperlinkTooltipResolver, ResolveTooltipLabelEvent>
    {
        /// <inheritdoc/>
        public override bool CanBeDisabled => true;

        /// <inheritdoc/>
        public override int Priority => HarmonyLib.Priority.Low;

        /// <inheritdoc/>
        public override bool SkipCanceled => true;

        /// <summary>
        /// Tries to find a <see cref="Hyperlink"/> on the <paramref name="button"/>'s <see cref="Slot"/>
        /// and extracts the tooltip label from its <see cref="Hyperlink.Reason">reason</see> for opening.
        /// </summary>
        /// <param name="button">The button component next to which a <see cref="Hyperlink"/> will be searched.</param>
        /// <param name="label">The found label if a suitable Hyperlink is found; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if a suitable <see cref="Hyperlink"/> was found; otherwise, <c>false</c>.</returns>
        public static bool TryGetTooltipLabel(IButton button, [NotNullWhen(true)] out LocaleString? label)
        {
            if (button.Slot.GetComponent<Hyperlink>() is not Hyperlink hyperlink)
            {
                label = default;
                return false;
            }

            var reason = hyperlink.Reason ?? "None Given";
            label = Mod.GetLocaleString("Tooltip.OpenHyperlink", ("reason", reason), ("url", hyperlink.URL.Value));

            return label is not null;
        }

        /// <inheritdoc/>
        protected override void Handle(ResolveTooltipLabelEvent eventData)
        {
            if (TryGetTooltipLabel(eventData.Button, out var label))
                eventData.Label = label;
        }
    }
}