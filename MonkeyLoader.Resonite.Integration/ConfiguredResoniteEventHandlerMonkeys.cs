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
    /// Specifically, to act as an <see cref="ICancelableEventHandler{TEvent}">event handler</see> for cancelable <typeparamref name="TEvent"/>s.
    /// </summary>
    /// <inheritdoc/>
    public abstract class ConfiguredResoniteCancelableEventHandlerMonkey<TMonkey, TConfigSection, TEvent> : ResoniteCancelableEventHandlerMonkey<TMonkey, TEvent>, IConfiguredMonkey<TConfigSection>
        where TMonkey : ConfiguredResoniteCancelableEventHandlerMonkey<TMonkey, TConfigSection, TEvent>, new()
        where TConfigSection : ConfigSection, new()
        where TEvent : CancelableSyncEvent
    {
        /// <summary>
        /// Gets the loaded config section for this patcher after it has been <see cref="MonkeyBase.Run">run</see>.
        /// </summary>
        public static TConfigSection ConfigSection { get; private set; } = null!;

        TConfigSection IConfiguredMonkey<TConfigSection>.ConfigSection => ConfigSection;
        ConfigSection IConfiguredMonkey.ConfigSection => ConfigSection;

        /// <inheritdoc/>
        protected ConfiguredResoniteCancelableEventHandlerMonkey()
        { }

        /// <remarks>
        /// Override this method if you need to patch something involved in the initialization of the game.<br/>
        /// For ResoniteMonkeys, the default behavior of<see cref="Monkey{TMonkey}.OnLoaded">OnLoaded</see>()
        /// is moved to <see cref="ResoniteMonkey{TMonkey}.OnEngineReady">OnEngineReady</see>().
        /// <para/>
        /// Strongly consider also overriding <see cref="ResoniteMonkey{TMonkey}.OnEngineReady">OnEngineReady</see>() if you override this method.<br/>
        /// Otherwise your patches will be applied twice, if you're using <c>[<see cref="HarmonyPatchCategory"/>(nameof(MyPatcher))]</c> attributes.
        /// <para/>
        /// <i>By default:</i> Loads this patcher's <c><typeparamref name="TConfigSection"/>
        /// <see cref="ConfigSection">ConfigSection</see></c> and returns <c>true</c>.
        /// </remarks>
        /// <inheritdoc/>
        protected override bool OnLoaded()
        {
            ConfigSection = Config.LoadSection<TConfigSection>();

            return base.OnLoaded();
        }
    }

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
        /// <summary>
        /// Gets the loaded config section for this patcher after it has been <see cref="MonkeyBase.Run">run</see>.
        /// </summary>
        public static TConfigSection ConfigSection { get; private set; } = null!;

        TConfigSection IConfiguredMonkey<TConfigSection>.ConfigSection => ConfigSection;
        ConfigSection IConfiguredMonkey.ConfigSection => ConfigSection;

        /// <inheritdoc/>
        protected ConfiguredResoniteEventHandlerMonkey()
        { }

        /// <remarks>
        /// Override this method if you need to patch something involved in the initialization of the game.<br/>
        /// For ResoniteMonkeys, the default behavior of<see cref="Monkey{TMonkey}.OnLoaded">OnLoaded</see>()
        /// is moved to <see cref="ResoniteMonkey{TMonkey}.OnEngineReady">OnEngineReady</see>().
        /// <para/>
        /// Strongly consider also overriding <see cref="ResoniteMonkey{TMonkey}.OnEngineReady">OnEngineReady</see>() if you override this method.<br/>
        /// Otherwise your patches will be applied twice, if you're using <c>[<see cref="HarmonyPatchCategory"/>(nameof(MyPatcher))]</c> attributes.
        /// <para/>
        /// <i>By default:</i> Loads this patcher's <c><typeparamref name="TConfigSection"/>
        /// <see cref="ConfigSection">ConfigSection</see></c> and returns <c>true</c>.
        /// </remarks>
        /// <inheritdoc/>
        protected override bool OnLoaded()
        {
            ConfigSection = Config.LoadSection<TConfigSection>();

            return base.OnLoaded();
        }
    }
}