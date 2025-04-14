using MonkeyLoader.Configuration;
using MonkeyLoader.Events;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Represents the base class for patchers that run after Resonite's assemblies have been loaded and that hook into the game's lifecycle,
    /// while using a <c><typeparamref name="TConfigSection"/> <see cref="ConfigSection">ConfigSection</see></c>.<br/>
    /// Specifically, to act as an <see cref="IAsyncEventHandler{TEvent}">async event handler</see> for <typeparamref name="TEvent"/>s.
    /// </summary>
    /// <typeparam name="TMonkey">The type of the actual patcher.</typeparam>
    /// <typeparam name="TConfigSection">The type of the config section to load.</typeparam>
    /// <typeparam name="TEvent">The <see cref="AsyncEvent"/> type to handle.</typeparam>
    /// <inheritdoc/>
    public abstract class ConfiguredResoniteAsyncEventHandlerMonkey<TMonkey, TConfigSection, TEvent>
            : ResoniteAsyncEventHandlerMonkey<TMonkey, TEvent>, IConfiguredMonkey<TConfigSection>
        where TMonkey : ConfiguredResoniteAsyncEventHandlerMonkey<TMonkey, TConfigSection, TEvent>, new()
        where TConfigSection : ConfigSection, new()
        where TEvent : AsyncEvent
    {
        /// <inheritdoc cref="ConfiguredMonkey{TMonkey, TConfigSection}.ConfigSection"/>
        public static TConfigSection ConfigSection { get; private set; } = null!;

        TConfigSection IConfiguredMonkey<TConfigSection>.ConfigSection => ConfigSection;
        ConfigSection IConfiguredMonkey.ConfigSection => ConfigSection;

        /// <inheritdoc/>
        protected ConfiguredResoniteAsyncEventHandlerMonkey()
        { }

        /// <inheritdoc cref="ConfiguredResoniteMonkey{TMonkey, TConfigSection}.OnLoaded"/>
        protected override bool OnLoaded()
        {
            ConfigSection = Config.LoadSection<TConfigSection>();

            return base.OnLoaded();
        }
    }

    /// <inheritdoc cref="ConfiguredResoniteAsyncEventHandlerMonkey{TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ConfiguredResoniteAsyncEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2>
            : ResoniteAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2>, IConfiguredMonkey<TConfigSection>
        where TMonkey : ConfiguredResoniteAsyncEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2>, new()
        where TConfigSection : ConfigSection, new()
        where TEvent1 : AsyncEvent
        where TEvent2 : AsyncEvent
    {
        /// <inheritdoc cref="ConfiguredMonkey{TMonkey, TConfigSection}.ConfigSection"/>
        public static TConfigSection ConfigSection { get; private set; } = null!;

        TConfigSection IConfiguredMonkey<TConfigSection>.ConfigSection => ConfigSection;
        ConfigSection IConfiguredMonkey.ConfigSection => ConfigSection;

        /// <inheritdoc/>
        protected ConfiguredResoniteAsyncEventHandlerMonkey()
        { }

        /// <inheritdoc cref="ConfiguredResoniteMonkey{TMonkey, TConfigSection}.OnLoaded"/>
        protected override bool OnLoaded()
        {
            ConfigSection = Config.LoadSection<TConfigSection>();

            return base.OnLoaded();
        }
    }

    /// <inheritdoc cref="ConfiguredResoniteAsyncEventHandlerMonkey{TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ConfiguredResoniteAsyncEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3>
            : ResoniteAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3>, IConfiguredMonkey<TConfigSection>
        where TMonkey : ConfiguredResoniteAsyncEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3>, new()
        where TConfigSection : ConfigSection, new()
        where TEvent1 : AsyncEvent
        where TEvent2 : AsyncEvent
        where TEvent3 : AsyncEvent
    {
        /// <inheritdoc cref="ConfiguredMonkey{TMonkey, TConfigSection}.ConfigSection"/>
        public static TConfigSection ConfigSection { get; private set; } = null!;

        TConfigSection IConfiguredMonkey<TConfigSection>.ConfigSection => ConfigSection;
        ConfigSection IConfiguredMonkey.ConfigSection => ConfigSection;

        /// <inheritdoc/>
        protected ConfiguredResoniteAsyncEventHandlerMonkey()
        { }

        /// <inheritdoc cref="ConfiguredResoniteMonkey{TMonkey, TConfigSection}.OnLoaded"/>
        protected override bool OnLoaded()
        {
            ConfigSection = Config.LoadSection<TConfigSection>();

            return base.OnLoaded();
        }
    }

    /// <inheritdoc cref="ConfiguredResoniteAsyncEventHandlerMonkey{TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ConfiguredResoniteAsyncEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4>
        : ResoniteAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4>, IConfiguredMonkey<TConfigSection>
        where TMonkey : ConfiguredResoniteAsyncEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4>, new()
        where TConfigSection : ConfigSection, new()
        where TEvent1 : AsyncEvent
        where TEvent2 : AsyncEvent
        where TEvent3 : AsyncEvent
        where TEvent4 : AsyncEvent
    {
        /// <inheritdoc cref="ConfiguredMonkey{TMonkey, TConfigSection}.ConfigSection"/>
        public static TConfigSection ConfigSection { get; private set; } = null!;

        TConfigSection IConfiguredMonkey<TConfigSection>.ConfigSection => ConfigSection;
        ConfigSection IConfiguredMonkey.ConfigSection => ConfigSection;

        /// <inheritdoc/>
        protected ConfiguredResoniteAsyncEventHandlerMonkey()
        { }

        /// <inheritdoc cref="ConfiguredResoniteMonkey{TMonkey, TConfigSection}.OnLoaded"/>
        protected override bool OnLoaded()
        {
            ConfigSection = Config.LoadSection<TConfigSection>();

            return base.OnLoaded();
        }
    }

    /// <inheritdoc cref="ConfiguredResoniteAsyncEventHandlerMonkey{TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ConfiguredResoniteAsyncEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>
            : ResoniteAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>, IConfiguredMonkey<TConfigSection>
        where TMonkey : ConfiguredResoniteAsyncEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>, new()
        where TConfigSection : ConfigSection, new()
        where TEvent1 : AsyncEvent
        where TEvent2 : AsyncEvent
        where TEvent3 : AsyncEvent
        where TEvent4 : AsyncEvent
        where TEvent5 : AsyncEvent
    {
        /// <inheritdoc cref="ConfiguredMonkey{TMonkey, TConfigSection}.ConfigSection"/>
        public static TConfigSection ConfigSection { get; private set; } = null!;

        TConfigSection IConfiguredMonkey<TConfigSection>.ConfigSection => ConfigSection;
        ConfigSection IConfiguredMonkey.ConfigSection => ConfigSection;

        /// <inheritdoc/>
        protected ConfiguredResoniteAsyncEventHandlerMonkey()
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
    /// Specifically, to act as an <see cref="IAsyncEventHandler{TEvent}">async event handler</see> for the <c>TEvent</c> generic parameter(s).
    /// </summary>
    /// <typeparam name="TMonkey">The type of the actual patcher.</typeparam>
    /// <typeparam name="TConfigSection">The type of the config section to load.</typeparam>
    /// <typeparam name="TEvent1">The first <see cref="AsyncEvent"/> type to handle.</typeparam>
    /// <typeparam name="TEvent2">The second <see cref="AsyncEvent"/> type to handle.</typeparam>
    /// <typeparam name="TEvent3">The third <see cref="AsyncEvent"/> type to handle.</typeparam>
    /// <typeparam name="TEvent4">The fourth <see cref="AsyncEvent"/> type to handle.</typeparam>
    /// <typeparam name="TEvent5">The fifth <see cref="AsyncEvent"/> type to handle.</typeparam>
    /// <typeparam name="TEvent6">The sixth <see cref="AsyncEvent"/> type to handle.</typeparam>
    /// <inheritdoc cref="ResoniteMonkey{TMonkey}"/>
    public abstract class ConfiguredResoniteAsyncEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6>
            : ResoniteAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6>, IConfiguredMonkey<TConfigSection>
        where TMonkey : ConfiguredResoniteAsyncEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6>, new()
        where TConfigSection : ConfigSection, new()
        where TEvent1 : AsyncEvent
        where TEvent2 : AsyncEvent
        where TEvent3 : AsyncEvent
        where TEvent4 : AsyncEvent
        where TEvent5 : AsyncEvent
        where TEvent6 : AsyncEvent
    {
        /// <inheritdoc cref="ConfiguredMonkey{TMonkey, TConfigSection}.ConfigSection"/>
        public static TConfigSection ConfigSection { get; private set; } = null!;

        TConfigSection IConfiguredMonkey<TConfigSection>.ConfigSection => ConfigSection;
        ConfigSection IConfiguredMonkey.ConfigSection => ConfigSection;

        /// <inheritdoc/>
        protected ConfiguredResoniteAsyncEventHandlerMonkey()
        { }

        /// <inheritdoc cref="ConfiguredResoniteMonkey{TMonkey, TConfigSection}.OnLoaded"/>
        protected override bool OnLoaded()
        {
            ConfigSection = Config.LoadSection<TConfigSection>();

            return base.OnLoaded();
        }
    }
}