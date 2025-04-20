using MonkeyLoader.Configuration;
using MonkeyLoader.Events;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Represents the base class for patchers that run after Resonite's assemblies have been loaded and that hook into the game's lifecycle,
    /// while using a <c><typeparamref name="TConfigSection"/> <see cref="ConfigSection">ConfigSection</see></c>.<br/>
    /// Specifically, to act as an <see cref="ICancelableAsyncEventHandler{TEvent}">async event handler</see> for cancelable <typeparamref name="TEvent"/>s.
    /// </summary>
    /// <typeparam name="TMonkey">The type of the actual patcher.</typeparam>
    /// <typeparam name="TConfigSection">The type of the config section to load.</typeparam>
    /// <typeparam name="TEvent">The <see cref="CancelableAsyncEvent"/> type to handle.</typeparam>
    /// <inheritdoc/>
    public abstract class ConfiguredResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TConfigSection, TEvent>
            : ResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TEvent>, IConfiguredMonkey<TConfigSection>
        where TMonkey : ConfiguredResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TConfigSection, TEvent>, new()
        where TConfigSection : ConfigSection, new()
        where TEvent : CancelableAsyncEvent
    {
        /// <inheritdoc cref="ConfiguredMonkey{TMonkey, TConfigSection}.ConfigSection"/>
        public static TConfigSection ConfigSection { get; private set; } = null!;

        TConfigSection IConfiguredMonkey<TConfigSection>.ConfigSection => ConfigSection;
        ConfigSection IConfiguredMonkey.ConfigSection => ConfigSection;

        /// <inheritdoc/>
        protected ConfiguredResoniteCancelableAsyncEventHandlerMonkey()
        { }

        /// <inheritdoc cref="ConfiguredResoniteMonkey{TMonkey, TConfigSection}.OnLoaded"/>
        protected override bool OnLoaded()
        {
            ConfigSection = Config.LoadSection<TConfigSection>();

            return base.OnLoaded();
        }
    }

    /// <inheritdoc cref="ConfiguredResoniteCancelableAsyncEventHandlerMonkey{TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ConfiguredResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2>
            : ResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2>, IConfiguredMonkey<TConfigSection>
        where TMonkey : ConfiguredResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2>, new()
        where TConfigSection : ConfigSection, new()
        where TEvent1 : CancelableAsyncEvent
        where TEvent2 : CancelableAsyncEvent
    {
        /// <inheritdoc cref="ConfiguredMonkey{TMonkey, TConfigSection}.ConfigSection"/>
        public static TConfigSection ConfigSection { get; private set; } = null!;

        TConfigSection IConfiguredMonkey<TConfigSection>.ConfigSection => ConfigSection;
        ConfigSection IConfiguredMonkey.ConfigSection => ConfigSection;

        /// <inheritdoc/>
        protected ConfiguredResoniteCancelableAsyncEventHandlerMonkey()
        { }

        /// <inheritdoc cref="ConfiguredResoniteMonkey{TMonkey, TConfigSection}.OnLoaded"/>
        protected override bool OnLoaded()
        {
            ConfigSection = Config.LoadSection<TConfigSection>();

            return base.OnLoaded();
        }
    }

    /// <inheritdoc cref="ConfiguredResoniteCancelableAsyncEventHandlerMonkey{TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ConfiguredResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3>
            : ResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3>, IConfiguredMonkey<TConfigSection>
        where TMonkey : ConfiguredResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3>, new()
        where TConfigSection : ConfigSection, new()
        where TEvent1 : CancelableAsyncEvent
        where TEvent2 : CancelableAsyncEvent
        where TEvent3 : CancelableAsyncEvent
    {
        /// <inheritdoc cref="ConfiguredMonkey{TMonkey, TConfigSection}.ConfigSection"/>
        public static TConfigSection ConfigSection { get; private set; } = null!;

        TConfigSection IConfiguredMonkey<TConfigSection>.ConfigSection => ConfigSection;
        ConfigSection IConfiguredMonkey.ConfigSection => ConfigSection;

        /// <inheritdoc/>
        protected ConfiguredResoniteCancelableAsyncEventHandlerMonkey()
        { }

        /// <inheritdoc cref="ConfiguredResoniteMonkey{TMonkey, TConfigSection}.OnLoaded"/>
        protected override bool OnLoaded()
        {
            ConfigSection = Config.LoadSection<TConfigSection>();

            return base.OnLoaded();
        }
    }

    /// <inheritdoc cref="ConfiguredResoniteCancelableAsyncEventHandlerMonkey{TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ConfiguredResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4>
        : ResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4>, IConfiguredMonkey<TConfigSection>
        where TMonkey : ConfiguredResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4>, new()
        where TConfigSection : ConfigSection, new()
        where TEvent1 : CancelableAsyncEvent
        where TEvent2 : CancelableAsyncEvent
        where TEvent3 : CancelableAsyncEvent
        where TEvent4 : CancelableAsyncEvent
    {
        /// <inheritdoc cref="ConfiguredMonkey{TMonkey, TConfigSection}.ConfigSection"/>
        public static TConfigSection ConfigSection { get; private set; } = null!;

        TConfigSection IConfiguredMonkey<TConfigSection>.ConfigSection => ConfigSection;
        ConfigSection IConfiguredMonkey.ConfigSection => ConfigSection;

        /// <inheritdoc/>
        protected ConfiguredResoniteCancelableAsyncEventHandlerMonkey()
        { }

        /// <inheritdoc cref="ConfiguredResoniteMonkey{TMonkey, TConfigSection}.OnLoaded"/>
        protected override bool OnLoaded()
        {
            ConfigSection = Config.LoadSection<TConfigSection>();

            return base.OnLoaded();
        }
    }

    /// <inheritdoc cref="ConfiguredResoniteCancelableAsyncEventHandlerMonkey{TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ConfiguredResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>
            : ResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>, IConfiguredMonkey<TConfigSection>
        where TMonkey : ConfiguredResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>, new()
        where TConfigSection : ConfigSection, new()
        where TEvent1 : CancelableAsyncEvent
        where TEvent2 : CancelableAsyncEvent
        where TEvent3 : CancelableAsyncEvent
        where TEvent4 : CancelableAsyncEvent
        where TEvent5 : CancelableAsyncEvent
    {
        /// <inheritdoc cref="ConfiguredMonkey{TMonkey, TConfigSection}.ConfigSection"/>
        public static TConfigSection ConfigSection { get; private set; } = null!;

        TConfigSection IConfiguredMonkey<TConfigSection>.ConfigSection => ConfigSection;
        ConfigSection IConfiguredMonkey.ConfigSection => ConfigSection;

        /// <inheritdoc/>
        protected ConfiguredResoniteCancelableAsyncEventHandlerMonkey()
        { }

        /// <inheritdoc cref="ConfiguredResoniteMonkey{TMonkey, TConfigSection}.OnLoaded"/>
        protected override bool OnLoaded()
        {
            ConfigSection = Config.LoadSection<TConfigSection>();

            return base.OnLoaded();
        }
    }

    /// <summary>
    /// Represents the base class for patchers that run after Resonite's assemblies have been loaded and that hook into the game's lifecycle.<br/>
    /// Specifically, to act as an <see cref="ICancelableAsyncEventHandler{TEvent}">async event handler</see> for the cancelable <c>TEvent</c> generic parameter(s).
    /// </summary>
    /// <typeparam name="TMonkey">The type of the actual patcher.</typeparam>
    /// <typeparam name="TConfigSection">The type of the config section to load.</typeparam>
    /// <typeparam name="TEvent1">The first <see cref="CancelableAsyncEvent"/> type to handle.</typeparam>
    /// <typeparam name="TEvent2">The second <see cref="CancelableAsyncEvent"/> type to handle.</typeparam>
    /// <typeparam name="TEvent3">The third <see cref="CancelableAsyncEvent"/> type to handle.</typeparam>
    /// <typeparam name="TEvent4">The fourth <see cref="CancelableAsyncEvent"/> type to handle.</typeparam>
    /// <typeparam name="TEvent5">The fifth <see cref="CancelableAsyncEvent"/> type to handle.</typeparam>
    /// <typeparam name="TEvent6">The sixth <see cref="CancelableAsyncEvent"/> type to handle.</typeparam>
    /// <inheritdoc cref="ResoniteMonkey{TMonkey}"/>
    public abstract class ConfiguredResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6>
            : ResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6>, IConfiguredMonkey<TConfigSection>
        where TMonkey : ConfiguredResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6>, new()
        where TConfigSection : ConfigSection, new()
        where TEvent1 : CancelableAsyncEvent
        where TEvent2 : CancelableAsyncEvent
        where TEvent3 : CancelableAsyncEvent
        where TEvent4 : CancelableAsyncEvent
        where TEvent5 : CancelableAsyncEvent
        where TEvent6 : CancelableAsyncEvent
    {
        /// <inheritdoc cref="ConfiguredMonkey{TMonkey, TConfigSection}.ConfigSection"/>
        public static TConfigSection ConfigSection { get; private set; } = null!;

        TConfigSection IConfiguredMonkey<TConfigSection>.ConfigSection => ConfigSection;
        ConfigSection IConfiguredMonkey.ConfigSection => ConfigSection;

        /// <inheritdoc/>
        protected ConfiguredResoniteCancelableAsyncEventHandlerMonkey()
        { }

        /// <inheritdoc cref="ConfiguredResoniteMonkey{TMonkey, TConfigSection}.OnLoaded"/>
        protected override bool OnLoaded()
        {
            ConfigSection = Config.LoadSection<TConfigSection>();

            return base.OnLoaded();
        }
    }
}