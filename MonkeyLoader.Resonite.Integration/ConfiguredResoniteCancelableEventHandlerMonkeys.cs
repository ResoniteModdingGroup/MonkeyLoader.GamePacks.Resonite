using HarmonyLib;
using MonkeyLoader.Configuration;
using MonkeyLoader.Events;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Represents the base class for patchers that run after Resonite's assemblies have been loaded and that hook into the game's lifecycle,
    /// while using a <c><typeparamref name="TConfigSection"/> <see cref="ConfigSection">ConfigSection</see></c>.<br/>
    /// Specifically, to act as an <see cref="ICancelableEventHandler{TEvent}">event handler</see> for cancelable <typeparamref name="TEvent"/>s.
    /// </summary>
    /// <inheritdoc/>
    public abstract class ConfiguredResoniteCancelableEventHandlerMonkey<TMonkey, TConfigSection, TEvent> : ResoniteCancelableEventHandlerMonkey<TMonkey, TEvent>, IConfiguredMonkey<TConfigSection>
        where TMonkey : ConfiguredResoniteCancelableEventHandlerMonkey<TMonkey, TConfigSection, TEvent>, new()
        where TConfigSection : ConfigSection, new()
        where TEvent : CancelableSyncEvent
    {
        /// <inheritdoc cref="ConfiguredMonkey{TMonkey, TConfigSection}.ConfigSection"/>
        public static TConfigSection ConfigSection { get; private set; } = null!;

        TConfigSection IConfiguredMonkey<TConfigSection>.ConfigSection => ConfigSection;
        ConfigSection IConfiguredMonkey.ConfigSection => ConfigSection;

        /// <inheritdoc/>
        protected ConfiguredResoniteCancelableEventHandlerMonkey()
        { }

        /// <inheritdoc cref="ConfiguredResoniteMonkey{TMonkey, TConfigSection}.OnLoaded"/>
        protected override bool OnLoaded()
        {
            ConfigSection = Config.LoadSection<TConfigSection>();

            return base.OnLoaded();
        }
    }

    /// <inheritdoc cref="ConfiguredResoniteCancelableEventHandlerMonkey{TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ConfiguredResoniteCancelableEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2>
            : ResoniteCancelableEventHandlerMonkey<TMonkey, TEvent1, TEvent2>, IConfiguredMonkey<TConfigSection>
        where TMonkey : ConfiguredResoniteCancelableEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2>, new()
        where TConfigSection : ConfigSection, new()
        where TEvent1 : CancelableSyncEvent
        where TEvent2 : CancelableSyncEvent
    {
        /// <inheritdoc cref="ConfiguredMonkey{TMonkey, TConfigSection}.ConfigSection"/>
        public static TConfigSection ConfigSection { get; private set; } = null!;

        TConfigSection IConfiguredMonkey<TConfigSection>.ConfigSection => ConfigSection;
        ConfigSection IConfiguredMonkey.ConfigSection => ConfigSection;

        /// <inheritdoc/>
        protected ConfiguredResoniteCancelableEventHandlerMonkey()
        { }

        /// <inheritdoc cref="ConfiguredResoniteMonkey{TMonkey, TConfigSection}.OnLoaded"/>
        protected override bool OnLoaded()
        {
            ConfigSection = Config.LoadSection<TConfigSection>();

            return base.OnLoaded();
        }
    }

    /// <inheritdoc cref="ConfiguredResoniteCancelableEventHandlerMonkey{TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ConfiguredResoniteCancelableEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3>
            : ResoniteCancelableEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3>, IConfiguredMonkey<TConfigSection>
        where TMonkey : ConfiguredResoniteCancelableEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3>, new()
        where TConfigSection : ConfigSection, new()
        where TEvent1 : CancelableSyncEvent
        where TEvent2 : CancelableSyncEvent
        where TEvent3 : CancelableSyncEvent
    {
        /// <inheritdoc cref="ConfiguredMonkey{TMonkey, TConfigSection}.ConfigSection"/>
        public static TConfigSection ConfigSection { get; private set; } = null!;

        TConfigSection IConfiguredMonkey<TConfigSection>.ConfigSection => ConfigSection;
        ConfigSection IConfiguredMonkey.ConfigSection => ConfigSection;

        /// <inheritdoc/>
        protected ConfiguredResoniteCancelableEventHandlerMonkey()
        { }

        /// <inheritdoc cref="ConfiguredResoniteMonkey{TMonkey, TConfigSection}.OnLoaded"/>
        protected override bool OnLoaded()
        {
            ConfigSection = Config.LoadSection<TConfigSection>();

            return base.OnLoaded();
        }
    }

    /// <inheritdoc cref="ConfiguredResoniteCancelableEventHandlerMonkey{TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ConfiguredResoniteCancelableEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4>
        : ResoniteCancelableEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4>, IConfiguredMonkey<TConfigSection>
        where TMonkey : ConfiguredResoniteCancelableEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4>, new()
        where TConfigSection : ConfigSection, new()
        where TEvent1 : CancelableSyncEvent
        where TEvent2 : CancelableSyncEvent
        where TEvent3 : CancelableSyncEvent
        where TEvent4 : CancelableSyncEvent
    {
        /// <inheritdoc cref="ConfiguredMonkey{TMonkey, TConfigSection}.ConfigSection"/>
        public static TConfigSection ConfigSection { get; private set; } = null!;

        TConfigSection IConfiguredMonkey<TConfigSection>.ConfigSection => ConfigSection;
        ConfigSection IConfiguredMonkey.ConfigSection => ConfigSection;

        /// <inheritdoc/>
        protected ConfiguredResoniteCancelableEventHandlerMonkey()
        { }

        /// <inheritdoc cref="ConfiguredResoniteMonkey{TMonkey, TConfigSection}.OnLoaded"/>
        protected override bool OnLoaded()
        {
            ConfigSection = Config.LoadSection<TConfigSection>();

            return base.OnLoaded();
        }
    }

    /// <inheritdoc cref="ConfiguredResoniteCancelableEventHandlerMonkey{TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ConfiguredResoniteCancelableEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>
            : ResoniteCancelableEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>, IConfiguredMonkey<TConfigSection>
        where TMonkey : ConfiguredResoniteCancelableEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>, new()
        where TConfigSection : ConfigSection, new()
        where TEvent1 : CancelableSyncEvent
        where TEvent2 : CancelableSyncEvent
        where TEvent3 : CancelableSyncEvent
        where TEvent4 : CancelableSyncEvent
        where TEvent5 : CancelableSyncEvent
    {
        /// <inheritdoc cref="ConfiguredMonkey{TMonkey, TConfigSection}.ConfigSection"/>
        public static TConfigSection ConfigSection { get; private set; } = null!;

        TConfigSection IConfiguredMonkey<TConfigSection>.ConfigSection => ConfigSection;
        ConfigSection IConfiguredMonkey.ConfigSection => ConfigSection;

        /// <inheritdoc/>
        protected ConfiguredResoniteCancelableEventHandlerMonkey()
        { }

        /// <inheritdoc cref="ConfiguredResoniteMonkey{TMonkey, TConfigSection}.OnLoaded"/>
        protected override bool OnLoaded()
        {
            ConfigSection = Config.LoadSection<TConfigSection>();

            return base.OnLoaded();
        }
    }

    /// <summary>
    /// Represents the base class for patchers that run after Resonite's assemblies have been loaded and that hook into the game's lifecycle,
    /// while using a <c><typeparamref name="TConfigSection"/> <see cref="ConfigSection">ConfigSection</see></c>.<br/>
    /// Specifically, to act as an <see cref="ICancelableEventHandler{TEvent}">event handler</see> for the cancelable <c>TEvent</c> generic parameter(s).
    /// </summary>
    /// <typeparam name="TMonkey">The type of the actual patcher.</typeparam>
    /// <typeparam name="TConfigSection">The type of the config section to load.</typeparam>
    /// <typeparam name="TEvent1">The first <see cref="CancelableSyncEvent"/> type to handle.</typeparam>
    /// <typeparam name="TEvent2">The second <see cref="CancelableSyncEvent"/> type to handle.</typeparam>
    /// <typeparam name="TEvent3">The third <see cref="CancelableSyncEvent"/> type to handle.</typeparam>
    /// <typeparam name="TEvent4">The fourth <see cref="CancelableSyncEvent"/> type to handle.</typeparam>
    /// <typeparam name="TEvent5">The fifth <see cref="CancelableSyncEvent"/> type to handle.</typeparam>
    /// <typeparam name="TEvent6">The sixth <see cref="CancelableSyncEvent"/> type to handle.</typeparam>
    /// <inheritdoc cref="ResoniteMonkey{TMonkey}"/>
    public abstract class ConfiguredResoniteCancelableEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6>
            : ResoniteCancelableEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6>, IConfiguredMonkey<TConfigSection>
        where TMonkey : ConfiguredResoniteCancelableEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6>, new()
        where TConfigSection : ConfigSection, new()
        where TEvent1 : CancelableSyncEvent
        where TEvent2 : CancelableSyncEvent
        where TEvent3 : CancelableSyncEvent
        where TEvent4 : CancelableSyncEvent
        where TEvent5 : CancelableSyncEvent
        where TEvent6 : CancelableSyncEvent
    {
        /// <inheritdoc cref="ConfiguredMonkey{TMonkey, TConfigSection}.ConfigSection"/>
        public static TConfigSection ConfigSection { get; private set; } = null!;

        TConfigSection IConfiguredMonkey<TConfigSection>.ConfigSection => ConfigSection;
        ConfigSection IConfiguredMonkey.ConfigSection => ConfigSection;

        /// <inheritdoc/>
        protected ConfiguredResoniteCancelableEventHandlerMonkey()
        { }

        /// <inheritdoc cref="ConfiguredResoniteMonkey{TMonkey, TConfigSection}.OnLoaded"/>
        protected override bool OnLoaded()
        {
            ConfigSection = Config.LoadSection<TConfigSection>();

            return base.OnLoaded();
        }
    }
}