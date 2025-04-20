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
    /// Handles resolving the tooltip for <see cref="IButton">buttons</see> with a <see cref="Comment"/>
    /// text that starts with the right <see cref="CommentTextPrefix">prefix</see>.
    /// </summary>
    /// <remarks>
    /// This covers backwards-compatibility to Psychpsyo's old Neos mod
    /// <see href="https://github.com/Psychpsyo/Tooltippery">Tooltippery</see>.
    /// </remarks>
    public sealed class CommentTooltipResolver : ResoniteCancelableEventHandlerMonkey<CommentTooltipResolver, ResolveTooltipLabelEvent>
    {
        /// <summary>
        /// The prefix that a <see cref="Comment"/>'s text must have to define a tooltip label.
        /// </summary>
        public const string CommentTextPrefix = "TooltipperyLabel:";

        /// <inheritdoc/>
        public override bool CanBeDisabled => true;

        /// <inheritdoc/>
        public override int Priority => HarmonyLib.Priority.High;

        /// <inheritdoc/>
        public override bool SkipCanceled => true;

        /// <summary>
        /// Tries to find a <see cref="Comment"/> on the <paramref name="button"/>'s <see cref="Slot"/>
        /// that starts with the right <see cref="CommentTextPrefix">prefix</see>
        /// and extracts the tooltip label from it.
        /// </summary>
        /// <param name="button">The button component next to which a <see cref="Comment"/> will be searched.</param>
        /// <param name="label">The found label if a suitable comment is found; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if a suitable <see cref="Comment"/> was found; otherwise, <c>false</c>.</returns>
        public static bool TryGetTooltipLabel(IButton button, [NotNullWhen(true)] out string? label)
        {
            var commentText = button.Slot.GetComponent<Comment>(IsTooltipComment)?.Text.Value;

            label = commentText?[CommentTextPrefix.Length..];

            return label is not null;
        }

        /// <inheritdoc/>
        protected override void Handle(ResolveTooltipLabelEvent eventData)
        {
            if (TryGetTooltipLabel(eventData.Button, out var label))
                eventData.Label = label;
        }

        private static bool IsTooltipComment(Comment comment)
            => comment.Text.Value?.StartsWith(CommentTextPrefix) ?? false;
    }
}