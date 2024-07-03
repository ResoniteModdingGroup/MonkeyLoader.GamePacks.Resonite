using HarmonyLib;
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
    /// Represents the base class for patchers that run after Resonite's assemblies have been loaded and that hook into the game's lifecycle.<br/>
    /// Specifically, to act as an <see cref="ICancelableEventHandler{TEvent}">event handler</see> for cancelable <typeparamref name="TEvent"/>s.
    /// </summary>
    /// <inheritdoc/>
    public abstract class ResoniteCancelableEventHandlerMonkey<TMonkey, TEvent> : ResoniteMonkey<TMonkey>, ICancelableEventHandler<TEvent>
        where TMonkey : ResoniteCancelableEventHandlerMonkey<TMonkey, TEvent>, new()
        where TEvent : CancelableSyncEvent
    {
        /// <inheritdoc/>
        public abstract int Priority { get; }

        /// <inheritdoc/>
        public abstract bool SkipCanceled { get; }

        /// <inheritdoc/>
        protected ResoniteCancelableEventHandlerMonkey()
        { }

        void ICancelableEventHandler<TEvent>.Handle(TEvent eventData)
        {
            if (AppliesTo(eventData))
                Handle(eventData);
        }

        /// <summary>
        /// Determines whether the given event should be handled
        /// by this event handler based on its data.
        /// </summary>
        /// <param name="eventData">An object containing all the relevant information for the event.</param>
        /// <returns><c>true</c> if this event handler applies to the event; otherwise, <c>false</c>.</returns>
        protected abstract bool AppliesTo(TEvent eventData);

        /// <summary>
        /// Handles the given event based on its data, if <see cref="AppliesTo">AppliesTo</see> returned <c>true</c>.
        /// </summary>
        /// <param name="eventData">An object containing all the relevant information for the applicable event.</param>
        protected abstract void Handle(TEvent eventData);

        /// <remarks>
        /// Override <see cref="ResoniteMonkey{TMonkey}.OnLoaded">OnLoaded</see>() to patch before anything is initialized,
        /// but strongly consider also overriding this method if you do that.<br/>
        /// Otherwise your patches will be applied twice, if you're using <c>[<see cref="HarmonyPatchCategory"/>(nameof(MyPatcher))]</c> attributes.
        /// <para/>
        /// <i>By default:</i> <see cref="Mod.RegisterEventHandler{TEvent}(ICancelableEventHandler{TEvent})">Registers</see>
        /// this Monkey as an event handler and applies the <see cref="Harmony"/> patches of the
        /// <see cref="Harmony.PatchCategory(string)">category</see> with this patcher's type's name.<br/>
        /// Easy to apply by using <c>[<see cref="HarmonyPatchCategory"/>(nameof(MyPatcher))]</c> attribute.
        /// </remarks>
        /// <inheritdoc/>
        protected override bool OnEngineReady()
        {
            Mod.RegisterEventHandler(this);

            return base.OnEngineReady();
        }

        /// <remarks>
        /// <i>By default:</i> <see cref="Mod.UnregisterEventHandler{TEvent}(ICancelableEventHandler{TEvent})">Unregisters</see>
        /// this monkey as an event handler for <typeparamref name="TEvent"/>s
        /// and removes all <see cref="Harmony"/> patches done
        /// using this Monkey's <see cref="MonkeyBase.Harmony">Harmony</see> instance,
        /// if not exiting, and returns <c>true</c>.
        /// </remarks>
        /// <inheritdoc/>
        protected override bool OnShutdown(bool applicationExiting)
        {
            if (!applicationExiting)
                Mod.UnregisterEventHandler(this);

            return base.OnShutdown(applicationExiting);
        }
    }

    /// <summary>
    /// Represents the base class for patchers that run after Resonite's assemblies have been loaded and that hook into the game's lifecycle.<br/>
    /// Specifically, to act as an <see cref="IEventHandler{TEvent}">event handler</see> for <typeparamref name="TEvent"/>s.
    /// </summary>
    /// <inheritdoc/>
    public abstract class ResoniteEventHandlerMonkey<TMonkey, TEvent> : ResoniteMonkey<TMonkey>, IEventHandler<TEvent>
        where TMonkey : ResoniteEventHandlerMonkey<TMonkey, TEvent>, new()
        where TEvent : SyncEvent
    {
        /// <inheritdoc/>
        public abstract int Priority { get; }

        /// <inheritdoc/>
        protected ResoniteEventHandlerMonkey()
        { }

        void IEventHandler<TEvent>.Handle(TEvent eventData)
        {
            if (AppliesTo(eventData))
                Handle(eventData);
        }

        /// <summary>
        /// Determines whether the given event should be handled
        /// by this event handler based on its data.
        /// </summary>
        /// <param name="eventData">An object containing all the relevant information for the event.</param>
        /// <returns><c>true</c> if this event handler applies to the event; otherwise, <c>false</c>.</returns>
        protected abstract bool AppliesTo(TEvent eventData);

        /// <summary>
        /// Handles the given event based on its data, if <see cref="AppliesTo">AppliesTo</see> returned <c>true</c>.
        /// </summary>
        /// <param name="eventData">An object containing all the relevant information for the applicable event.</param>
        protected abstract void Handle(TEvent eventData);

        /// <remarks>
        /// Override <see cref="ResoniteMonkey{TMonkey}.OnLoaded">OnLoaded</see>() to patch before anything is initialized,
        /// but strongly consider also overriding this method if you do that.<br/>
        /// Otherwise your patches will be applied twice, if you're using <c>[<see cref="HarmonyPatchCategory"/>(nameof(MyPatcher))]</c> attributes.
        /// <para/>
        /// <i>By default:</i> <see cref="Mod.RegisterEventHandler{TEvent}(IEventHandler{TEvent})">Registers</see>
        /// this Monkey as an event handler and applies the <see cref="Harmony"/> patches of the
        /// <see cref="Harmony.PatchCategory(string)">category</see> with this patcher's type's name.<br/>
        /// Easy to apply by using <c>[<see cref="HarmonyPatchCategory"/>(nameof(MyPatcher))]</c> attribute.
        /// </remarks>
        /// <inheritdoc/>
        protected override bool OnEngineReady()
        {
            Mod.RegisterEventHandler(this);

            return base.OnEngineReady();
        }

        /// <remarks>
        /// <i>By default:</i> <see cref="Mod.UnregisterEventHandler{TEvent}(IEventHandler{TEvent})">Unregisters</see>
        /// this monkey as an event handler for <typeparamref name="TEvent"/>s
        /// and removes all <see cref="Harmony"/> patches done
        /// using this Monkey's <see cref="MonkeyBase.Harmony">Harmony</see> instance,
        /// if not exiting, and returns <c>true</c>.
        /// </remarks>
        /// <inheritdoc/>
        protected override bool OnShutdown(bool applicationExiting)
        {
            if (!applicationExiting)
                Mod.UnregisterEventHandler(this);

            return base.OnShutdown(applicationExiting);
        }
    }
}