using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.UI.Tooltips
{
    /// <summary>
    /// Contains data for the tooltip shown on an <see cref="IButton"/>.
    /// </summary>
    public sealed class Tooltip
    {
        /// <summary>
        /// Gets whether this tooltip only exists for the local user.
        /// </summary>
        public bool IsLocal => Root.IsLocalElement;

        /// <summary>
        /// Gets whether the tooltip is shown on the <see cref="UserspaceRadiantDash">dash</see>.
        /// </summary>
        public bool IsOnDash { get; }

        /// <summary>
        /// Gets the label of the tooltip.
        /// </summary>
        public LocaleString Label { get; }

        /// <summary>
        /// Gets the root slot of the tooltip.
        /// </summary>
        public Slot Root { get; }

        /// <summary>
        /// Gets the scale multiplier for this tooltip.
        /// </summary>
        public float Scale => TooltipConfig.Instance.TextScale;

        /// <summary>
        /// Gets the <see cref="Text"/> that's displaying
        /// the <see cref="Label">label</see> of this tooltip.
        /// </summary>
        public Text Text { get; }

        internal Tooltip(Slot parent, in float3 localPosition, float localScale, in LocaleString label)
        {
            var xSize = MathX.Min(700f * Scale, 700f);
            var ySize = ((100f * Scale) + label.content.Length) * 0.575f;

            // text slot for the tooltip
            Root = TooltipConfig.Instance.EnableNonLocalTooltips ? parent.AddSlot("Tooltip", false) : parent.AddLocalSlot("Local Tooltip", false);
            Root.LocalScale *= localScale;
            Root.LocalPosition = localPosition + (localScale * ((float3.Backward * parent.GlobalScaleToLocal(0.015f)) + parent.GlobalDirectionToLocal(float3.Down * (.1f + (ySize * 0.5f)))));

            IsOnDash = Root.GetComponentInParents<UserspaceRadiantDash>() is not null;

            var ui = RadiantUI_Panel.SetupPanel(Root, label, new float2(xSize, ySize), false, false);

            if (ui.Canvas.Slot.GetComponent<BoxCollider>() is BoxCollider collider)
                collider.Enabled = false;

            if (ui.Canvas.Slot.GetComponentInChildren<Image>() is Image image)
                image.Tint.Value = TooltipConfig.Instance.BackgroundColor;

            if (ui.Canvas.Slot.FindChild("Header", false, false) is Slot headerSlot && headerSlot.GetComponent<RectTransform>() is RectTransform rectTransform)
            {
                rectTransform.AnchorMin.Value = float2.Zero;
                rectTransform.OffsetMin.Value = float2.Zero;
            }

            if (ui.Canvas.Slot.FindChild("Content", false, false) is Slot contentSlot)
                contentSlot.Destroy();

            Text = null!;

            foreach (var text in ui.Canvas.Slot.GetComponentsInChildren<Text>())
            {
                var textSize = 24f;

                if (Scale < 1)
                    textSize *= MathX.Pow(Scale, 1.5f);
                else
                    textSize *= MathX.Pow(Scale, 6);

                text.Size.Value = textSize;
                text.Color.Value = TooltipConfig.Instance.TextColor;

                Text = text;
            }
        }

        internal void Close() => Root.Destroy();
    }
}