using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Configuration;
using MonkeyLoader.Events;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.UI
{
    /// <summary>
    /// Represents the base class for patchers that run after Resonite's assemblies have been loaded and that hook into the game's lifecycle,
    /// while using a <c><typeparamref name="TConfigSection"/> <see cref="ConfigSection">ConfigSection</see></c>.<br/>
    /// Specifically, to act as an <see cref="IEventHandler{TEvent}">event handler</see> for <see cref="BuildInspectorEvent"/>s
    /// for <see cref="Worker"/>s of a(n open) generic base type.
    /// </summary>
    /// <inheritdoc/>
    public abstract class ConfiguredResoniteInspectorMonkey<TMonkey, TConfigSection, TEvent> : ResoniteInspectorMonkey<TMonkey, TEvent>
        where TMonkey : ConfiguredResoniteInspectorMonkey<TMonkey, TConfigSection, TEvent>, new()
        where TConfigSection : ConfigSection, new()
        where TEvent : BuildInspectorEvent
    {
        /// <summary>
        /// Gets the loaded config section for this patcher after it has been <see cref="MonkeyBase.Run">run</see>.
        /// </summary>
        public static TConfigSection ConfigSection { get; private set; } = null!;

        /// <inheritdoc/>
        protected ConfiguredResoniteInspectorMonkey(Type baseType) : base(baseType)
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
    /// Specifically, to act as an <see cref="IEventHandler{TEvent}">event handler</see> for <see cref="BuildInspectorEvent"/>s
    /// for <see cref="Worker"/>s of a specific (base) type.
    /// </summary>
    /// <inheritdoc/>
    public abstract class ConfiguredResoniteInspectorMonkey<TMonkey, TConfigSection, TEvent, TWorker> : ResoniteInspectorMonkey<TMonkey, TEvent, TWorker>
        where TMonkey : ConfiguredResoniteInspectorMonkey<TMonkey, TConfigSection, TEvent, TWorker>, new()
        where TConfigSection : ConfigSection, new()
        where TEvent : BuildInspectorEvent
        where TWorker : Worker
    {
        /// <summary>
        /// Gets the loaded config section for this patcher after it has been <see cref="MonkeyBase.Run">run</see>.
        /// </summary>
        protected static TConfigSection ConfigSection { get; private set; } = null!;

        /// <inheritdoc/>
        protected ConfiguredResoniteInspectorMonkey()
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