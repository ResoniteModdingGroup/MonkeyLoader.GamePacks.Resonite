using Elements.Assets;
using Elements.Core;
using FrooxEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.UI
{
    public sealed class Tooltip
    {
        public bool IsDash { get; }
        public LocaleString Label { get; }
        public Slot Root { get; }
        public float Scale => (IsDash ? 2.5f : 1) * TooltipConfig.Instance.TextScale;
        public TextRenderer TextRenderer { get; }

        public Tooltip(Slot parent, in float3 localPosition, in LocaleString label)
        {
            // text slot for the tooltip
            Root = parent.AddLocalSlot("Local Tooltip");
            Root.LocalPosition = localPosition;

            IsDash = Root.GetComponentInParents<UserspaceRadiantDash>() is not null;

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
            Slot backPanelOffset = Root.AddLocalSlot("bgOffset");
            backPanelOffset.LocalPosition = new float3(0, 0, 1);
            Slot backPanel = backPanelOffset.AddLocalSlot("Background");
            QuadMesh quad = backPanel.AttachComponent<QuadMesh>();
            MeshRenderer meshRenderer = backPanel.AttachComponent<MeshRenderer>();
            meshRenderer.Mesh.Target = quad;
            BoundingBoxDriver sizeDriver = Root.AttachComponent<BoundingBoxDriver>();
            sizeDriver.BoundedSource.Target = TextRenderer;
            sizeDriver.Size.Target = backPanel.Scale_Field;
            sizeDriver.Center.Target = backPanel.Position_Field;
            sizeDriver.Padding.Value = new float3(8 * Scale, 8 * Scale, 0);

            UI_UnlitMaterial mat = backPanel.AttachComponent<UI_UnlitMaterial>();
            mat.Tint.Value = TooltipConfig.Instance.BackgroundColor;
            meshRenderer.Material.Target = mat;

            Root.GlobalScale = Root.World.LocalUserViewScale * new float3(.001f, .001f, .001f);
        }

        public void Close()
        {
            Root.Destroy();
        }

        public void setText(string newLabel)
        {
            TextRenderer.Text.Value = newLabel;
        }
    }
}