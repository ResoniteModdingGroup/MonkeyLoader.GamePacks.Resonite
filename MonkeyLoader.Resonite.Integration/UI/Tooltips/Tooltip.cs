using Elements.Assets;
using Elements.Core;
using FrooxEngine;
using System;
using System.Collections.Generic;
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
        public float Scale => (IsOnDash ? 2.5f : 1) * TooltipConfig.Instance.TextScale;

        /// <summary>
        /// Gets the text renderer that's displaying
        /// the <see cref="Label">label</see> of this tooltip.
        /// </summary>
        public TextRenderer TextRenderer { get; }

        internal Tooltip(Slot parent, in float3 localPosition, in LocaleString label)
        {
            // text slot for the tooltip
            Root = TooltipConfig.Instance.EnableNonLocalTooltips ? parent.AddSlot("Tooltip") : parent.AddLocalSlot("Local Tooltip");
            Root.LocalPosition = localPosition;

            IsOnDash = Root.GetComponentInParents<UserspaceRadiantDash>() is not null;

            TextRenderer = Root.AttachComponent<TextRenderer>();
            TextRenderer.Text.SetLocalized(label);
            TextRenderer.VerticalAlign.Value = TextVerticalAlignment.Top;
            TextRenderer.HorizontalAlign.Value = TextHorizontalAlignment.Left;
            TextRenderer.Size.Value = 200 * Scale;
            TextRenderer.Bounded.Value = true;
            TextRenderer.BoundsSize.Value = new float2(700 * Scale, 1);
            TextRenderer.BoundsAlignment.Value = Alignment.TopLeft;
            TextRenderer.Color.Value = TooltipConfig.Instance.TextColor;

            // back panel slot
            var backPanelOffset = TooltipConfig.Instance.EnableNonLocalTooltips ? Root.AddSlot("bgOffset") : Root.AddLocalSlot("bgOffset");
            backPanelOffset.LocalPosition = new float3(0, 0, 1);
            var backPanel = TooltipConfig.Instance.EnableNonLocalTooltips ? backPanelOffset.AddSlot("Background") : backPanelOffset.AddLocalSlot("Background");
            var quad = backPanel.AttachComponent<QuadMesh>();
            var meshRenderer = backPanel.AttachComponent<MeshRenderer>();
            meshRenderer.Mesh.Target = quad;
            var sizeDriver = Root.AttachComponent<BoundingBoxDriver>();
            sizeDriver.BoundedSource.Target = TextRenderer;
            sizeDriver.Size.Target = backPanel.Scale_Field;
            sizeDriver.Center.Target = backPanel.Position_Field;
            sizeDriver.Padding.Value = new float3(8 * Scale, 8 * Scale, 0);

            var mat = backPanel.AttachComponent<UI_UnlitMaterial>();
            mat.Tint.Value = TooltipConfig.Instance.BackgroundColor;
            meshRenderer.Material.Target = mat;

            Root.GlobalScale = Root.World.LocalUserViewScale * new float3(.001f, .001f, .001f);
        }

        internal void Close() => Root.Destroy();
    }
}