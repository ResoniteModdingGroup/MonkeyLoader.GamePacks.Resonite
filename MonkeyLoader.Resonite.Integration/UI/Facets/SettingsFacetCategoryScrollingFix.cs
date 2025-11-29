using Elements.Core;
using FrooxEngine;
using FrooxEngine.FrooxEngine.ProtoFlux.CoreNodes;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Operators;
using FrooxEngine.UIX;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLoader.Resonite.UI.Facets
{
    internal sealed class SettingsFacetCategoryScrollingFix : ResoniteEventHandlerMonkey<SettingsFacetCategoryScrollingFix, FacetPresetLoadedEvent>
    {
        /// <summary>
        /// Defines the text of the Comment used to indicate that this fix has been applied.<br/>
        /// This must not be changed, as it may be applied multiple times otherwise.
        /// </summary>
        private const string FixedMarker = "Category Scrolling Fixed";

        public override int Priority => HarmonyLib.Priority.Normal;

        protected override bool AppliesTo(FacetPresetLoadedEvent eventData)
            => base.AppliesTo(eventData)
            && eventData.FacetPreset is SettingsFacetPreset
            && eventData is not TemplateFacetPresetFallbackBuiltEvent
            && eventData.Facet.Slot.GetComponent<Comment>(comment => comment.Text == FixedMarker) is null;

        protected override void Handle(FacetPresetLoadedEvent eventData)
        {
            if (eventData.Facet.Slot.GetComponentInChildren<RootCategoryView>() is not RootCategoryView rootCategoryView)
                return;

            var scrollAreaSlot = eventData.Canvas.Slot.FindChildInHierarchy("Scroll Area");
            if (scrollAreaSlot?.GetComponent<Mask>() is not Mask mask || scrollAreaSlot?.GetComponent<Image>() is not Image image || scrollAreaSlot?.GetComponent<OverlappingLayout>() is not OverlappingLayout overlappingLayout)
                return;

            var scrollRectSlot = scrollAreaSlot.FindChild("Scroll Rect");
            if (scrollRectSlot?.GetComponent<ContentSizeFitter>() is not ContentSizeFitter contentSizeFitter || scrollRectSlot?.GetComponent<ScrollRect>() is not ScrollRect scrollRect)
                return;

            mask.Enabled = true;
            image.Enabled = true;
            overlappingLayout.Enabled = false;
            var layoutElement = scrollAreaSlot.AttachComponent<LayoutElement>();

            contentSizeFitter.Enabled = true;
            contentSizeFitter.HorizontalFit.Value = SizeFit.PreferredSize;
            contentSizeFitter.VerticalFit.Value = SizeFit.PreferredSize;
            scrollRect.Enabled = true;

            rootCategoryView.CategoryManager.ContainerRoot.Target = null!;
            scrollRectSlot.DestroyChildren();
            var verticalLayoutSlot = scrollRectSlot.AddSlot("Vertical Layout");
            var verticalLayout = verticalLayoutSlot.AttachComponent<VerticalLayout>();
            verticalLayout.ForceExpandHeight.Value = false;
            verticalLayout.VerticalAlign.Value = LayoutVerticalAlignment.Top;

            var float2Field = verticalLayoutSlot.AttachComponent<ValueField<float2>>();
            var rectSizeDriver = verticalLayoutSlot.AttachComponent<RectSizeDriver>();
            rectSizeDriver.TargetSize.Target = float2Field.Value;

            // Create ProtoFlux to drive the layoutElement.MinWidth with the verticalLayout's width
            var valueSource = verticalLayoutSlot.AttachComponent<ValueSource<float2>>();
            valueSource.TrySetRootSource(float2Field.Value);

            var unpack = verticalLayoutSlot.AttachComponent<Unpack_Float2>();
            unpack.V.Target = valueSource;

            var minWidthDriver = verticalLayoutSlot.AttachComponent<ValueFieldDrive<float>>();
            minWidthDriver.TrySetRootTarget(layoutElement.MinWidth);
            minWidthDriver.Value.Target = unpack.X;

            rootCategoryView.CategoryManager.ContainerRoot.Target = verticalLayoutSlot;

            // Add marker to prevent multiple applications
            eventData.Facet.Slot.AttachComponent<Comment>().Text.Value = FixedMarker;
            Logger.Info(() => "Injected scrolling fix into settings facet!");
        }
    }
}