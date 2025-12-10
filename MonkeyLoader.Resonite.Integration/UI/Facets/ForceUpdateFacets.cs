using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using MonkeyLoader.Components;
using MonkeyLoader.Resonite.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.UI.Facets
{
    // This is necessary because of: https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/4701
    // Otherwise, most users wouldn't have the template for the resettable section.
    internal sealed class ForceUpdateFacets : ResoniteEventHandlerMonkey<ForceUpdateFacets, TemplateFacetPresetLoadedEvent>
    {
        private readonly ConfigKeySessionShare<bool> _enabledSessionShare = new();

        private bool _wasEnabledFromDefault;

        public override bool CanBeDisabled => true;

        public override int Priority => HarmonyLib.Priority.Normal;

        protected override bool AppliesTo(TemplateFacetPresetLoadedEvent eventData)
            => true; // Always need to add the button so it shows up when enabled

        protected override void Handle(TemplateFacetPresetLoadedEvent eventData)
        {
            if (_wasEnabledFromDefault && eventData.FacetPreset is SettingsFacetPreset && eventData.Facet.Slot.GetComponentInChildren<FeedResettableGroupInterface>() is not null)
                Enabled = false;

            var ui = new UIBuilder(eventData.Canvas).WithButtonStyle();

            var button = ui.Button(OfficialAssets.Common.Icons.Reload, colorX.White.SetA(0), colorX.White)
                .WithTooltip(Mod.GetLocaleString("Tooltip.RefreshFacet"))
                .WithLocalAction(_ =>
                {
                    var currentSize = eventData.Facet.CurrentSize;
                    eventData.Canvas.Slot.DestroyChildren();
                    eventData.FacetPreset.SetupFacetData(currentSize);

                    eventData.FacetPreset.Build(eventData.Facet, eventData.FacetPreset.SetupBackground(eventData.Facet), null);

                    if (eventData.Facet.Slot.GetComponentInParents<Workspace>() is Workspace workspace)
                        workspace.MarkModified();

                    Enabled = false;
                });

            // Prevent saving, make last item, and sync with enabled
            button.Slot.PersistentSelf = false;
            button.Slot.OrderOffset = long.MaxValue;
            _enabledSessionShare.DriveFromVariable(button.Slot.ActiveSelf_Field);

            // Place into upper-right corner with fixed size
            button.RectTransform.AnchorMin.Value = float2.One;
            button.RectTransform.AnchorMax.Value = float2.One;
            button.RectTransform.OffsetMin.Value = -34 * float2.One;
            button.RectTransform.OffsetMax.Value = -2 * float2.One;

            button.ColorDrivers.Clear();
            button.ColorDrivers.Add().ColorDrive.Target = button.Slot[0].GetComponent<Image>().Tint;
        }

        protected override bool OnComputeDefaultEnabledState()
        {
            _wasEnabledFromDefault = true;

            return true;
        }

        protected override bool OnEngineReady()
        {
            EnabledToggle!.Add(_enabledSessionShare);

            return base.OnEngineReady();
        }
    }
}