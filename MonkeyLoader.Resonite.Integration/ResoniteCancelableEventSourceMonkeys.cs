using HarmonyLib;
using MonkeyLoader.Events;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Represents the base class for patchers that run after Resonite's assemblies have been loaded and that hook into the game's lifecycle.<br/>
    /// Specifically, to act as a <see cref="ICancelableEventSource{TEvent}">cancelable event source</see> for <typeparamref name="TEvent"/>s.
    /// </summary>
    /// <typeparam name="TMonkey">The type of the actual patcher.</typeparam>
    /// <typeparam name="TEvent">The <see cref="CancelableSyncEvent"/> type to dispatch.</typeparam>
    /// <inheritdoc/>
    public abstract class ResoniteCancelableEventSourceMonkey<TMonkey, TEvent>
            : ResoniteMonkey<TMonkey>, ICancelableEventSource<TEvent>
        where TMonkey : ResoniteCancelableEventSourceMonkey<TMonkey, TEvent>, new()
        where TEvent : CancelableSyncEvent
    {
        private static CancelableEventDispatching<TEvent>? _eventDispatching;

        /// <inheritdoc/>
        protected ResoniteCancelableEventSourceMonkey()
        { }

        /// <summary>
        /// Dispatches the <typeparamref name="TEvent"/> with the given data.
        /// </summary>
        /// <param name="eventData">The event data to dispatch.</param>
        protected static void Dispatch(TEvent eventData)
            => _eventDispatching?.Invoke(eventData);

        /// <remarks>
        /// Override <see cref="ResoniteMonkey{TMonkey}.OnLoaded">OnLoaded</see>() to patch before anything is initialized,
        /// but strongly consider also overriding this method if you do that.<br/>
        /// Otherwise your patches will be applied twice, if you're using <c>[<see cref="HarmonyPatchCategory"/>(nameof(MyPatcher))]</c> attributes.
        /// <para/>
        /// <i>By default:</i> <see cref="Mod.RegisterEventSource{TEvent}(ICancelableEventSource{TEvent})">Registers</see>
        /// this Monkey as an event source and applies the <see cref="Harmony"/> patches of the
        /// <see cref="Harmony.PatchCategory(string)">category</see> with this patcher's type's name.<br/>
        /// Easy to apply by using <c>[<see cref="HarmonyPatchCategory"/>(nameof(MyPatcher))]</c> attribute.
        /// </remarks>
        /// <inheritdoc/>
        protected override bool OnEngineReady()
        {
            Mod.RegisterEventSource(this);

            return base.OnEngineReady();
        }

        /// <remarks>
        /// <i>By default:</i> <see cref="Mod.UnregisterEventSource{TEvent}(ICancelableEventSource{TEvent})">Unregisters</see>
        /// this monkey as an event source for <typeparamref name="TEvent"/>s
        /// and removes all <see cref="Harmony"/> patches done
        /// using this Monkey's <see cref="MonkeyBase.Harmony">Harmony</see> instance,
        /// if not exiting, and returns <c>true</c>.
        /// </remarks>
        /// <inheritdoc/>
        protected override bool OnShutdown(bool applicationExiting)
        {
            if (!applicationExiting)
                Mod.UnregisterEventSource(this);

            return base.OnShutdown(applicationExiting);
        }

        event CancelableEventDispatching<TEvent>? ICancelableEventSource<TEvent>.Dispatching
        {
            add => _eventDispatching += value;
            remove => _eventDispatching -= value;
        }
    }

    /// <inheritdoc cref="ResoniteCancelableEventSourceMonkey{TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ResoniteCancelableEventSourceMonkey<TMonkey, TEvent1, TEvent2>
            : ResoniteCancelableEventSourceMonkey<TMonkey, TEvent1>, ICancelableEventSource<TEvent2>
        where TMonkey : ResoniteCancelableEventSourceMonkey<TMonkey, TEvent1, TEvent2>, new()
        where TEvent1 : CancelableSyncEvent
        where TEvent2 : CancelableSyncEvent
    {
        private static CancelableEventDispatching<TEvent2>? _eventDispatching;

        /// <inheritdoc/>
        protected ResoniteCancelableEventSourceMonkey()
        { }

        /// <summary>
        /// Dispatches the <typeparamref name="TEvent2"/> with the given data.
        /// </summary>
        /// <inheritdoc cref="ResoniteCancelableEventSourceMonkey{TMonkey, TEvent}.Dispatch"/>
        protected static void Dispatch(TEvent2 eventData)
            => _eventDispatching?.Invoke(eventData);

        /// <inheritdoc/>
        protected override bool OnEngineReady()
        {
            Mod.RegisterEventSource<TEvent2>(this);

            return base.OnEngineReady();
        }

        /// <inheritdoc/>
        protected override bool OnShutdown(bool applicationExiting)
        {
            if (!applicationExiting)
                Mod.UnregisterEventSource<TEvent2>(this);

            return base.OnShutdown(applicationExiting);
        }

        event CancelableEventDispatching<TEvent2>? ICancelableEventSource<TEvent2>.Dispatching
        {
            add => _eventDispatching += value;
            remove => _eventDispatching -= value;
        }
    }

    /// <inheritdoc cref="ResoniteCancelableEventSourceMonkey{TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ResoniteCancelableEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3>
            : ResoniteCancelableEventSourceMonkey<TMonkey, TEvent1, TEvent2>, ICancelableEventSource<TEvent3>
        where TMonkey : ResoniteCancelableEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3>, new()
        where TEvent1 : CancelableSyncEvent
        where TEvent2 : CancelableSyncEvent
        where TEvent3 : CancelableSyncEvent
    {
        private static CancelableEventDispatching<TEvent3>? _eventDispatching;

        /// <inheritdoc/>
        protected ResoniteCancelableEventSourceMonkey()
        { }

        /// <summary>
        /// Dispatches the <typeparamref name="TEvent3"/> with the given data.
        /// </summary>
        /// <inheritdoc cref="ResoniteCancelableEventSourceMonkey{TMonkey, TEvent}.Dispatch"/>
        protected static void Dispatch(TEvent3 eventData)
            => _eventDispatching?.Invoke(eventData);

        /// <inheritdoc/>
        protected override bool OnEngineReady()
        {
            Mod.RegisterEventSource<TEvent3>(this);

            return base.OnEngineReady();
        }

        /// <inheritdoc/>
        protected override bool OnShutdown(bool applicationExiting)
        {
            if (!applicationExiting)
                Mod.UnregisterEventSource<TEvent3>(this);

            return base.OnShutdown(applicationExiting);
        }

        event CancelableEventDispatching<TEvent3>? ICancelableEventSource<TEvent3>.Dispatching
        {
            add => _eventDispatching += value;
            remove => _eventDispatching -= value;
        }
    }

    /// <inheritdoc cref="ResoniteCancelableEventSourceMonkey{TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ResoniteCancelableEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4>
            : ResoniteCancelableEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3>, ICancelableEventSource<TEvent4>
        where TMonkey : ResoniteCancelableEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4>, new()
        where TEvent1 : CancelableSyncEvent
        where TEvent2 : CancelableSyncEvent
        where TEvent3 : CancelableSyncEvent
        where TEvent4 : CancelableSyncEvent
    {
        private static CancelableEventDispatching<TEvent4>? _eventDispatching;

        /// <inheritdoc/>
        protected ResoniteCancelableEventSourceMonkey()
        { }

        /// <summary>
        /// Dispatches the <typeparamref name="TEvent4"/> with the given data.
        /// </summary>
        /// <inheritdoc cref="ResoniteCancelableEventSourceMonkey{TMonkey, TEvent}.Dispatch"/>
        protected static void Dispatch(TEvent4 eventData)
            => _eventDispatching?.Invoke(eventData);

        /// <inheritdoc/>
        protected override bool OnEngineReady()
        {
            Mod.RegisterEventSource<TEvent4>(this);

            return base.OnEngineReady();
        }

        /// <inheritdoc/>
        protected override bool OnShutdown(bool applicationExiting)
        {
            if (!applicationExiting)
                Mod.UnregisterEventSource<TEvent4>(this);

            return base.OnShutdown(applicationExiting);
        }

        event CancelableEventDispatching<TEvent4>? ICancelableEventSource<TEvent4>.Dispatching
        {
            add => _eventDispatching += value;
            remove => _eventDispatching -= value;
        }
    }

    /// <inheritdoc cref="ResoniteCancelableEventSourceMonkey{TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ResoniteCancelableEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>
            : ResoniteCancelableEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4>, ICancelableEventSource<TEvent5>
        where TMonkey : ResoniteCancelableEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>, new()
        where TEvent1 : CancelableSyncEvent
        where TEvent2 : CancelableSyncEvent
        where TEvent3 : CancelableSyncEvent
        where TEvent4 : CancelableSyncEvent
        where TEvent5 : CancelableSyncEvent
    {
        private static CancelableEventDispatching<TEvent5>? _eventDispatching;

        /// <inheritdoc/>
        protected ResoniteCancelableEventSourceMonkey()
        { }

        /// <summary>
        /// Dispatches the <typeparamref name="TEvent5"/> with the given data.
        /// </summary>
        /// <inheritdoc cref="ResoniteCancelableEventSourceMonkey{TMonkey, TEvent}.Dispatch"/>
        protected static void Dispatch(TEvent5 eventData)
            => _eventDispatching?.Invoke(eventData);

        /// <inheritdoc/>
        protected override bool OnEngineReady()
        {
            Mod.RegisterEventSource<TEvent5>(this);

            return base.OnEngineReady();
        }

        /// <inheritdoc/>
        protected override bool OnShutdown(bool applicationExiting)
        {
            if (!applicationExiting)
                Mod.UnregisterEventSource<TEvent5>(this);

            return base.OnShutdown(applicationExiting);
        }

        event CancelableEventDispatching<TEvent5>? ICancelableEventSource<TEvent5>.Dispatching
        {
            add => _eventDispatching += value;
            remove => _eventDispatching -= value;
        }
    }

    /// <summary>
    /// Represents the base class for patchers that run after Resonite's assemblies have been loaded and that hook into the game's lifecycle.<br/>
    /// Specifically, to act as a <see cref="ICancelableEventSource{TEvent}">cancelable event source</see> for the <c>TEvent</c> generic parameter(s).
    /// </summary>
    /// <typeparam name="TMonkey">The type of the actual patcher.</typeparam>
    /// <typeparam name="TEvent1">The first <see cref="CancelableSyncEvent"/> type to dispatch.</typeparam>
    /// <typeparam name="TEvent2">The second <see cref="CancelableSyncEvent"/> type to dispatch.</typeparam>
    /// <typeparam name="TEvent3">The third <see cref="CancelableSyncEvent"/> type to dispatch.</typeparam>
    /// <typeparam name="TEvent4">The fourth <see cref="CancelableSyncEvent"/> type to dispatch.</typeparam>
    /// <typeparam name="TEvent5">The fifth <see cref="CancelableSyncEvent"/> type to dispatch.</typeparam>
    /// <typeparam name="TEvent6">The sixth <see cref="CancelableSyncEvent"/> type to dispatch.</typeparam>
    /// <inheritdoc cref="ResoniteMonkey{TMonkey}"/>
    public abstract class ResoniteCancelableEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6>
            : ResoniteCancelableEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>, ICancelableEventSource<TEvent6>
        where TMonkey : ResoniteCancelableEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6>, new()
        where TEvent1 : CancelableSyncEvent
        where TEvent2 : CancelableSyncEvent
        where TEvent3 : CancelableSyncEvent
        where TEvent4 : CancelableSyncEvent
        where TEvent5 : CancelableSyncEvent
        where TEvent6 : CancelableSyncEvent
    {
        private static CancelableEventDispatching<TEvent6>? _eventDispatching;

        /// <inheritdoc/>
        protected ResoniteCancelableEventSourceMonkey()
        { }

        /// <summary>
        /// Dispatches the <typeparamref name="TEvent6"/> with the given data.
        /// </summary>
        /// <inheritdoc cref="ResoniteCancelableEventSourceMonkey{TMonkey, TEvent}.Dispatch"/>
        protected static void Dispatch(TEvent6 eventData)
            => _eventDispatching?.Invoke(eventData);

        /// <inheritdoc/>
        protected override bool OnEngineReady()
        {
            Mod.RegisterEventSource<TEvent6>(this);

            return base.OnEngineReady();
        }

        /// <inheritdoc/>
        protected override bool OnShutdown(bool applicationExiting)
        {
            if (!applicationExiting)
                Mod.UnregisterEventSource<TEvent6>(this);

            return base.OnShutdown(applicationExiting);
        }

        event CancelableEventDispatching<TEvent6>? ICancelableEventSource<TEvent6>.Dispatching
        {
            add => _eventDispatching += value;
            remove => _eventDispatching -= value;
        }
    }
}