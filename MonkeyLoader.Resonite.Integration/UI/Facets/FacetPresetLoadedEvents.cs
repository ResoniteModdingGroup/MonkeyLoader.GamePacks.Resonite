using FrooxEngine;
using FrooxEngine.UIX;
using MonkeyLoader.Events;

namespace MonkeyLoader.Resonite.UI.Facets
{
    /// <summary>
    /// Represents the base event data for the <see cref="FrooxEngine.FacetPreset"/> Loaded event,
    /// which is triggered whenever a facet preset has been loaded or attached.
    /// </summary>
    /// <remarks><para>
    /// This event can be used by monkeys to react to <see cref="FrooxEngine.Facet"/> presets
    /// being loaded from saved objects and templates, or newly attached,
    /// allowing them to inject things into system facets.
    /// </para><para>
    /// It is recommended to add an indicator of a successful injection
    /// (e.g. a <see cref="Comment"/> with a distinct text),
    /// as the facets are likely to be saved with the dash.<br/>
    /// At the same time, they may change with updates, so any injections should be implemented defensively.
    /// In particular, all requirements should be validated before starting to inject content.
    /// </para></remarks>
    [DispatchableBaseEvent, SubscribableBaseEvent]
    public class FacetPresetLoadedEvent : SyncEvent
    {
        /// <summary>
        /// Gets the <see cref="FrooxEngine.UIX.Canvas"/> of the loaded <see cref="Facet">Facet</see>.
        /// </summary>
        public Canvas Canvas => Facet.Canvas.Target;

        /// <summary>
        /// Gets the <see cref="FrooxEngine.Facet"/> loaded by the <see cref="FacetPreset">FacetPreset</see>.
        /// </summary>
        public Facet Facet { get; }

        /// <summary>
        /// Gets the facet preset that was loaded.
        /// </summary>
        public FacetPreset FacetPreset { get; }

        /// <summary>
        /// Gets whether the facet preset was freshly loaded.
        /// </summary>
        /// <value><see langword="false"/> if the facet was loaded from a saved object; otherwise, false.</value>
        public bool IsFreshLoad { get; }

        /// <summary>
        /// Gets whether the <see cref="FacetPreset">FacetPreset</see> is a <see cref="TemplateFacetPreset"/>.
        /// </summary>
        /// <value><see langword="true"/> if the <see cref="FacetPreset">FacetPreset</see> is a <see cref="TemplateFacetPreset"/>; otherwise, <see langword="false"/>.</value>
        public virtual bool IsTemplate => false;

        internal FacetPresetLoadedEvent(FacetPreset facetPreset, bool isFreshLoad)
        {
            FacetPreset = facetPreset;
            Facet = facetPreset.Facet;

            IsFreshLoad = isFreshLoad;
        }
    }

    /// <summary>
    /// Represents the event data for the <see cref="TemplateFacetPreset"/> Fallback Built event,
    /// which is triggered when the template object couldn't be loaded and a
    /// <see cref="TemplateFacetPreset.BuildFallback(Facet, Slot, int?)">fallback</see> is built instead.
    /// </summary>
    /// <inheritdoc/>
    public class TemplateFacetPresetFallbackBuiltEvent : TemplateFacetPresetLoadedEvent
    {
        /// <inheritdoc/>
        public override bool HasLoadedTemplate => false;

        internal TemplateFacetPresetFallbackBuiltEvent(TemplateFacetPreset facetPreset)
            : base(facetPreset, true)
        { }
    }

    /// <summary>
    /// Represents the base event data for the <see cref="TemplateFacetPreset"/> Loaded event,
    /// which is triggered when a template facet preset is (un)successfully loaded or attached.
    /// </summary>
    /// <inheritdoc/>
    [DispatchableBaseEvent, SubscribableBaseEvent]
    public class TemplateFacetPresetLoadedEvent : FacetPresetLoadedEvent
    {
        /// <summary>
        /// Gets the template facet preset that was loaded.
        /// </summary>
        public new TemplateFacetPreset FacetPreset { get; }

        /// <summary>
        /// Gets whether the <see cref="TemplateFacetPreset"/>'s
        /// <see cref="TemplateFacetPreset.BuildFallback(Facet, Slot, int?)">fallback</see> was built.
        /// </summary>
        /// <value><see langword="true"/> if the fallback was built; otherwise, <see langword="false"/>.</value>
        public virtual bool HasBuiltFallback => false;

        /// <summary>
        /// Gets whether the <see cref="TemplateFacetPreset"/>'s template was successfully loaded.
        /// </summary>
        /// <value><see langword="true"/> if the template was loaded; otherwise, <see langword="false"/>.</value>
        public virtual bool HasLoadedTemplate => false;

        /// <inheritdoc/>
        public override sealed bool IsTemplate => true;

        internal TemplateFacetPresetLoadedEvent(TemplateFacetPreset facetPreset, bool isFreshLoad)
            : base(facetPreset, isFreshLoad)
        {
            FacetPreset = facetPreset;
        }
    }

    /// <summary>
    /// Represents the event data for the <see cref="TemplateFacetPreset"/> Template Loaded event,
    /// which is triggered when the template object was loaded successfully.
    /// </summary>
    /// <inheritdoc/>
    public class TemplateFacetPresetTemplateLoadedEvent : TemplateFacetPresetLoadedEvent
    {
        /// <inheritdoc/>
        public override bool HasLoadedTemplate => true;

        internal TemplateFacetPresetTemplateLoadedEvent(TemplateFacetPreset facetPreset)
            : base(facetPreset, true)
        { }
    }
}