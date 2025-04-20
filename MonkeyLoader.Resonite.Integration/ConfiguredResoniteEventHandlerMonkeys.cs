using HarmonyLib;
using MonkeyLoader.Configuration;
using MonkeyLoader.Events;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Represents the base class for patchers that run after Resonite's assemblies have been loaded and that hook into the game's lifecycle,
    /// while using a <c><typeparamref name="TConfigSection"/> <see cref="ConfigSection">ConfigSection</see></c>.<br/>
    /// Specifically, to act as an <see cref="IEventHandler{TEvent}">event handler</see> for <typeparamref name="TEvent"/>s.
    /// </summary>
    /// <inheritdoc/>
    public abstract class ConfiguredResoniteEventHandlerMonkey<TMonkey, TConfigSection, TEvent> : ResoniteEventHandlerMonkey<TMonkey, TEvent>, IConfiguredMonkey<TConfigSection>
        where TMonkey : ConfiguredResoniteEventHandlerMonkey<TMonkey, TConfigSection, TEvent>, new()
        where TConfigSection : ConfigSection, new()
        where TEvent : SyncEvent
    {
        /// <inheritdoc cref="ConfiguredMonkey{TMonkey, TConfigSection}.ConfigSection"/>
        public static TConfigSection ConfigSection { get; private set; } = null!;

        TConfigSection IConfiguredMonkey<TConfigSection>.ConfigSection => ConfigSection;
        ConfigSection IConfiguredMonkey.ConfigSection => ConfigSection;

        /// <inheritdoc/>
        protected ConfiguredResoniteEventHandlerMonkey()
        { }

        /// <inheritdoc cref="ConfiguredResoniteMonkey{TMonkey, TConfigSection}.OnLoaded"/>
        protected override bool OnLoaded()
        {
            ConfigSection = Config.LoadSection<TConfigSection>();

            return base.OnLoaded();
        }
    }

    /// <inheritdoc cref="ConfiguredResoniteEventHandlerMonkey{TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ConfiguredResoniteEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2>
            : ResoniteEventHandlerMonkey<TMonkey, TEvent1, TEvent2>, IConfiguredMonkey<TConfigSection>
        where TMonkey : ConfiguredResoniteEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2>, new()
        where TConfigSection : ConfigSection, new()
        where TEvent1 : SyncEvent
        where TEvent2 : SyncEvent
    {
        /// <inheritdoc cref="ConfiguredMonkey{TMonkey, TConfigSection}.ConfigSection"/>
        public static TConfigSection ConfigSection { get; private set; } = null!;

        TConfigSection IConfiguredMonkey<TConfigSection>.ConfigSection => ConfigSection;
        ConfigSection IConfiguredMonkey.ConfigSection => ConfigSection;

        /// <inheritdoc/>
        protected ConfiguredResoniteEventHandlerMonkey()
        { }

        /// <inheritdoc cref="ConfiguredResoniteMonkey{TMonkey, TConfigSection}.OnLoaded"/>
        protected override bool OnLoaded()
        {
            ConfigSection = Config.LoadSection<TConfigSection>();

            return base.OnLoaded();
        }
    }

    /// <inheritdoc cref="ConfiguredResoniteEventHandlerMonkey{TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ConfiguredResoniteEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3>
            : ResoniteEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3>, IConfiguredMonkey<TConfigSection>
        where TMonkey : ConfiguredResoniteEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3>, new()
        where TConfigSection : ConfigSection, new()
        where TEvent1 : SyncEvent
        where TEvent2 : SyncEvent
        where TEvent3 : SyncEvent
    {
        /// <inheritdoc cref="ConfiguredMonkey{TMonkey, TConfigSection}.ConfigSection"/>
        public static TConfigSection ConfigSection { get; private set; } = null!;

        TConfigSection IConfiguredMonkey<TConfigSection>.ConfigSection => ConfigSection;
        ConfigSection IConfiguredMonkey.ConfigSection => ConfigSection;

        /// <inheritdoc/>
        protected ConfiguredResoniteEventHandlerMonkey()
        { }

        /// <inheritdoc cref="ConfiguredResoniteMonkey{TMonkey, TConfigSection}.OnLoaded"/>
        protected override bool OnLoaded()
        {
            ConfigSection = Config.LoadSection<TConfigSection>();

            return base.OnLoaded();
        }
    }

    /// <inheritdoc cref="ConfiguredResoniteEventHandlerMonkey{TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ConfiguredResoniteEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4>
        : ResoniteEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4>, IConfiguredMonkey<TConfigSection>
        where TMonkey : ConfiguredResoniteEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4>, new()
        where TConfigSection : ConfigSection, new()
        where TEvent1 : SyncEvent
        where TEvent2 : SyncEvent
        where TEvent3 : SyncEvent
        where TEvent4 : SyncEvent
    {
        /// <inheritdoc cref="ConfiguredMonkey{TMonkey, TConfigSection}.ConfigSection"/>
        public static TConfigSection ConfigSection { get; private set; } = null!;

        TConfigSection IConfiguredMonkey<TConfigSection>.ConfigSection => ConfigSection;
        ConfigSection IConfiguredMonkey.ConfigSection => ConfigSection;

        /// <inheritdoc/>
        protected ConfiguredResoniteEventHandlerMonkey()
        { }

        /// <inheritdoc cref="ConfiguredResoniteMonkey{TMonkey, TConfigSection}.OnLoaded"/>
        protected override bool OnLoaded()
        {
            ConfigSection = Config.LoadSection<TConfigSection>();

            return base.OnLoaded();
        }
    }

    /// <inheritdoc cref="ConfiguredResoniteEventHandlerMonkey{TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ConfiguredResoniteEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>
            : ResoniteEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>, IConfiguredMonkey<TConfigSection>
        where TMonkey : ConfiguredResoniteEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>, new()
        where TConfigSection : ConfigSection, new()
        where TEvent1 : SyncEvent
        where TEvent2 : SyncEvent
        where TEvent3 : SyncEvent
        where TEvent4 : SyncEvent
        where TEvent5 : SyncEvent
    {
        /// <inheritdoc cref="ConfiguredMonkey{TMonkey, TConfigSection}.ConfigSection"/>
        public static TConfigSection ConfigSection { get; private set; } = null!;

        TConfigSection IConfiguredMonkey<TConfigSection>.ConfigSection => ConfigSection;
        ConfigSection IConfiguredMonkey.ConfigSection => ConfigSection;

        /// <inheritdoc/>
        protected ConfiguredResoniteEventHandlerMonkey()
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
    /// Specifically, to act as an <see cref="IEventHandler{TEvent}">event handler</see> for the <c>TEvent</c> generic parameter(s).
    /// </summary>
    /// <typeparam name="TMonkey">The type of the actual patcher.</typeparam>
    /// <typeparam name="TConfigSection">The type of the config section to load.</typeparam>
    /// <typeparam name="TEvent1">The first <see cref="Event"/> type to handle.</typeparam>
    /// <typeparam name="TEvent2">The second <see cref="Event"/> type to handle.</typeparam>
    /// <typeparam name="TEvent3">The third <see cref="Event"/> type to handle.</typeparam>
    /// <typeparam name="TEvent4">The fourth <see cref="Event"/> type to handle.</typeparam>
    /// <typeparam name="TEvent5">The fifth <see cref="Event"/> type to handle.</typeparam>
    /// <typeparam name="TEvent6">The sixth <see cref="Event"/> type to handle.</typeparam>
    /// <inheritdoc cref="ResoniteMonkey{TMonkey}"/>
    public abstract class ConfiguredResoniteEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6>
            : ResoniteEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6>, IConfiguredMonkey<TConfigSection>
        where TMonkey : ConfiguredResoniteEventHandlerMonkey<TMonkey, TConfigSection, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6>, new()
        where TConfigSection : ConfigSection, new()
        where TEvent1 : SyncEvent
        where TEvent2 : SyncEvent
        where TEvent3 : SyncEvent
        where TEvent4 : SyncEvent
        where TEvent5 : SyncEvent
        where TEvent6 : SyncEvent
    {
        /// <inheritdoc cref="ConfiguredMonkey{TMonkey, TConfigSection}.ConfigSection"/>
        public static TConfigSection ConfigSection { get; private set; } = null!;

        TConfigSection IConfiguredMonkey<TConfigSection>.ConfigSection => ConfigSection;
        ConfigSection IConfiguredMonkey.ConfigSection => ConfigSection;

        /// <inheritdoc/>
        protected ConfiguredResoniteEventHandlerMonkey()
        { }

        /// <inheritdoc cref="ConfiguredResoniteMonkey{TMonkey, TConfigSection}.OnLoaded"/>
        protected override bool OnLoaded()
        {
            ConfigSection = Config.LoadSection<TConfigSection>();

            return base.OnLoaded();
        }
    }
}