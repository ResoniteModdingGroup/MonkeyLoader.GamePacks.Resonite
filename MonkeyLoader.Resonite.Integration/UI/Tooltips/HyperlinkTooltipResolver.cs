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
        /// <summary>
        /// The key used for the <see cref="LocaleString.content"/> on label-<see cref="LocaleString"/>s,
        /// when the existing label <see cref="LocaleString.isLocaleKey">is not a locale key</see>.
        /// </summary>
        public const string LabelArg = "label";

        /// <summary>
        /// The key used for the <see cref="Hyperlink.Reason"/> on label-<see cref="LocaleString"/>s.
        /// </summary>
        public const string ReasonArg = "reason";

        /// <summary>
        /// The key used for the <see cref="Hyperlink.URL"/> on label-<see cref="LocaleString"/>s.
        /// </summary>
        public const string UrlArg = "url";

        /// <inheritdoc/>
        public override bool CanBeDisabled => true;

        /// <inheritdoc/>
        public override int Priority => HarmonyLib.Priority.VeryLow;

        /// <inheritdoc/>
        public override bool SkipCanceled => !TooltipConfig.Instance.ShowExtendedTooltipForHyperlinks;

        /// <summary>
        /// Tries to find a <see cref="Hyperlink"/> on the <paramref name="button"/>'s <see cref="Slot"/>
        /// and extracts the tooltip label from its <see cref="Hyperlink.Reason">reason</see> for opening.
        /// </summary>
        /// <remarks>
        /// If an <paramref name="existingLabel"/> is set, the hyperlink information will be added to it if present.<br/>
        /// If the <paramref name="existingLabel"/> is already a hyperlink label, it will be used.
        /// </remarks>
        /// <param name="button">The button component next to which a <see cref="Hyperlink"/> will be searched.</param>
        /// <param name="label">The found label if a suitable Hyperlink is found; otherwise, <c>null</c>.</param>
        /// <param name="existingLabel">The optional label already existing for this button.</param>
        /// <returns><c>true</c> if a suitable <see cref="Hyperlink"/> was found; otherwise, <c>false</c>.</returns>
        public static bool TryGetTooltipLabel(IButton button, [NotNullWhen(true)] out LocaleString? label, LocaleString? existingLabel = default)
        {
            if (button.Slot.GetComponent<Hyperlink>() is not Hyperlink hyperlink)
            {
                label = default;
                return false;
            }

            var reason = hyperlink.Reason ?? "null";
            var url = hyperlink.URL.Value?.ToString() ?? "null";

            if (!existingLabel.HasValue)
            {
                label = Mod.GetLocaleString("Tooltip.OpenHyperlink.Direct", (ReasonArg, reason), (UrlArg, url));
                return true;
            }

            var exLabel = existingLabel.Value;

            if (exLabel.arguments.ContainsKey(ReasonArg) && exLabel.arguments.ContainsKey(UrlArg))
            {
                label = exLabel;
                return true;
            }

            if (exLabel.isLocaleKey)
            {
                label = new(exLabel.content, $"{exLabel.format ?? "{0}"}<size=50%><br/><br/></size><i>{reason}<br/>{url}</i>", true, exLabel.isContinuous, exLabel.arguments);
                return true;
            }

            label = Mod.GetLocaleString("Tooltip.OpenHyperlink.WithLabel", (LabelArg, exLabel.content), (ReasonArg, reason), (UrlArg, url));
            return true;
        }

        /// <inheritdoc/>
        protected override void Handle(ResolveTooltipLabelEvent eventData)
        {
            if (TryGetTooltipLabel(eventData.Button, out var label, eventData.Label))
                eventData.Label = label;
        }
    }
}