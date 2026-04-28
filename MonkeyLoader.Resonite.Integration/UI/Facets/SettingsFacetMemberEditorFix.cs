using EnumerableToolkit;
using FrooxEngine;

namespace MonkeyLoader.Resonite.UI.Facets
{
    internal sealed class SettingsFacetMemberEditorFix : ResoniteEventHandlerMonkey<SettingsFacetMemberEditorFix, FacetPresetLoadedEvent>
    {
        /// <summary>
        /// Defines the text of the Comment used to indicate that this fix has been applied.<br/>
        /// This must not be changed, as it may be applied multiple times otherwise.
        /// </summary>
        private const string FixedMarker = "Editors Fixed";

        public override bool CanBeDisabled => true;

        public override int Priority => HarmonyLib.Priority.Normal;

        public override Sequence<string> SubgroupPath => FacetPresetHelper.SubgroupPath;

        protected override bool AppliesTo(FacetPresetLoadedEvent eventData)
            => base.AppliesTo(eventData)
            && eventData.FacetPreset is SettingsFacetPreset
            && eventData is not TemplateFacetPresetFallbackBuiltEvent
            && eventData.Facet.Slot.GetComponent<Comment>(comment => comment.Text == FixedMarker) is null;

        protected override void Handle(FacetPresetLoadedEvent eventData)
        {
            if (eventData.Facet.Slot.GetComponentInChildren<RootCategoryView>() is not RootCategoryView rootCategoryView)
                return;

            if (rootCategoryView.ItemsManager.TemplateMapper.Target is not DataFeedItemMapper itemMapper)
                return;

            var templates = itemMapper.Mappings
                .Select(mapping => mapping.Template.Target?.Slot)
                .OfType<Slot>();

            foreach (var template in templates)
            {
                foreach (var primitiveEditor in template.GetComponentsInChildren<PrimitiveMemberEditor>())
                {
                    if (primitiveEditor._textEditor.Target is not TextEditor textEditor)
                        continue;

                    textEditor.EditingStarted.Target = primitiveEditor.EditingStarted;
                    textEditor.EditingChanged.Target = primitiveEditor.EditingChanged;
                    textEditor.EditingFinished.Target = primitiveEditor.EditingFinished;
                }
            }

            // Add marker to prevent multiple applications
            eventData.Facet.Slot.AttachComponent<Comment>().Text.Value = FixedMarker;
            Logger.Info(() => "Injected editor fix into settings facet!");
        }
    }
}