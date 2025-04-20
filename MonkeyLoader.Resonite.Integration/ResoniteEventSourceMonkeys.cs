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
    /// Specifically, to act as an <see cref="IEventSource{TEvent}">event source</see> for <typeparamref name="TEvent"/>s.
    /// </summary>
    /// <inheritdoc/>
    public abstract class ResoniteEventSourceMonkey<TMonkey, TEvent>
            : ResoniteMonkey<TMonkey>, IEventSource<TEvent>
        where TMonkey : ResoniteEventSourceMonkey<TMonkey, TEvent>, new()
        where TEvent : SyncEvent
    {
        private static EventDispatching<TEvent>? _eventDispatching;

        /// <inheritdoc/>
        protected ResoniteEventSourceMonkey()
        { }

        /// <summary>
        /// Dispatches the <typeparamref name="TEvent"/> with the given data.
        /// </summary>
        /// <param name="eventData">The event data to dispatch.</param>
        protected void Dispatch(TEvent eventData)
            => _eventDispatching?.Invoke(eventData);

        /// <remarks>
        /// Override <see cref="ResoniteMonkey{TMonkey}.OnLoaded">OnLoaded</see>() to patch before anything is initialized,
        /// but strongly consider also overriding this method if you do that.<br/>
        /// Otherwise your patches will be applied twice, if you're using <c>[<see cref="HarmonyPatchCategory"/>(nameof(MyPatcher))]</c> attributes.
        /// <para/>
        /// <i>By default:</i> <see cref="Mod.RegisterEventSource{TEvent}(IEventSource{TEvent})">Registers</see>
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
        /// <i>By default:</i> <see cref="Mod.UnregisterEventSource{TEvent}(IEventSource{TEvent})">Unregisters</see>
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

        event EventDispatching<TEvent>? IEventSource<TEvent>.Dispatching
        {
            add => _eventDispatching += value;
            remove => _eventDispatching -= value;
        }
    }

    /// <inheritdoc cref="ResoniteEventSourceMonkey{TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ResoniteEventSourceMonkey<TMonkey, TEvent1, TEvent2>
            : ResoniteEventSourceMonkey<TMonkey, TEvent1>, IEventSource<TEvent2>
        where TMonkey : ResoniteEventSourceMonkey<TMonkey, TEvent1, TEvent2>, new()
        where TEvent1 : SyncEvent
        where TEvent2 : SyncEvent
    {
        private static EventDispatching<TEvent2>? _eventDispatching;

        /// <inheritdoc/>
        protected ResoniteEventSourceMonkey()
        { }

        /// <summary>
        /// Dispatches the <typeparamref name="TEvent2"/> with the given data.
        /// </summary>
        /// <inheritdoc cref="ResoniteEventSourceMonkey{TMonkey, TEvent}.Dispatch"/>
        protected void Dispatch(TEvent2 eventData)
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

        event EventDispatching<TEvent2>? IEventSource<TEvent2>.Dispatching
        {
            add => _eventDispatching += value;
            remove => _eventDispatching -= value;
        }
    }

    /// <inheritdoc cref="ResoniteEventSourceMonkey{TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ResoniteEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3>
            : ResoniteEventSourceMonkey<TMonkey, TEvent1, TEvent2>, IEventSource<TEvent3>
        where TMonkey : ResoniteEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3>, new()
        where TEvent1 : SyncEvent
        where TEvent2 : SyncEvent
        where TEvent3 : SyncEvent
    {
        private static EventDispatching<TEvent3>? _eventDispatching;

        /// <inheritdoc/>
        protected ResoniteEventSourceMonkey()
        { }

        /// <summary>
        /// Dispatches the <typeparamref name="TEvent3"/> with the given data.
        /// </summary>
        /// <inheritdoc cref="ResoniteEventSourceMonkey{TMonkey, TEvent}.Dispatch"/>
        protected void Dispatch(TEvent3 eventData)
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

        event EventDispatching<TEvent3>? IEventSource<TEvent3>.Dispatching
        {
            add => _eventDispatching += value;
            remove => _eventDispatching -= value;
        }
    }

    /// <inheritdoc cref="ResoniteEventSourceMonkey{TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ResoniteEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4>
            : ResoniteEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3>, IEventSource<TEvent4>
        where TMonkey : ResoniteEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4>, new()
        where TEvent1 : SyncEvent
        where TEvent2 : SyncEvent
        where TEvent3 : SyncEvent
        where TEvent4 : SyncEvent
    {
        private static EventDispatching<TEvent4>? _eventDispatching;

        /// <inheritdoc/>
        protected ResoniteEventSourceMonkey()
        { }

        /// <summary>
        /// Dispatches the <typeparamref name="TEvent4"/> with the given data.
        /// </summary>
        /// <inheritdoc cref="ResoniteEventSourceMonkey{TMonkey, TEvent}.Dispatch"/>
        protected void Dispatch(TEvent4 eventData)
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

        event EventDispatching<TEvent4>? IEventSource<TEvent4>.Dispatching
        {
            add => _eventDispatching += value;
            remove => _eventDispatching -= value;
        }
    }

    /// <inheritdoc cref="ResoniteEventSourceMonkey{TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ResoniteEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>
            : ResoniteEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4>, IEventSource<TEvent5>
        where TMonkey : ResoniteEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>, new()
        where TEvent1 : SyncEvent
        where TEvent2 : SyncEvent
        where TEvent3 : SyncEvent
        where TEvent4 : SyncEvent
        where TEvent5 : SyncEvent
    {
        private static EventDispatching<TEvent5>? _eventDispatching;

        /// <inheritdoc/>
        protected ResoniteEventSourceMonkey()
        { }

        /// <summary>
        /// Dispatches the <typeparamref name="TEvent5"/> with the given data.
        /// </summary>
        /// <inheritdoc cref="ResoniteEventSourceMonkey{TMonkey, TEvent}.Dispatch"/>
        protected void Dispatch(TEvent5 eventData)
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

        event EventDispatching<TEvent5>? IEventSource<TEvent5>.Dispatching
        {
            add => _eventDispatching += value;
            remove => _eventDispatching -= value;
        }
    }

    /// <summary>
    /// Represents the base class for patchers that run after Resonite's assemblies have been loaded and that hook into the game's lifecycle.<br/>
    /// Specifically, to act as an <see cref="IEventSource{TEvent}">event source</see> for the <c>TEvent</c> generic parameter(s).
    /// </summary>
    /// <typeparam name="TMonkey">The type of the actual patcher.</typeparam>
    /// <typeparam name="TEvent1">The first <see cref="SyncEvent"/> type to dispatch.</typeparam>
    /// <typeparam name="TEvent2">The second <see cref="SyncEvent"/> type to dispatch.</typeparam>
    /// <typeparam name="TEvent3">The third <see cref="SyncEvent"/> type to dispatch.</typeparam>
    /// <typeparam name="TEvent4">The fourth <see cref="SyncEvent"/> type to dispatch.</typeparam>
    /// <typeparam name="TEvent5">The fifth <see cref="SyncEvent"/> type to dispatch.</typeparam>
    /// <typeparam name="TEvent6">The sixth <see cref="SyncEvent"/> type to dispatch.</typeparam>
    /// <inheritdoc cref="ResoniteMonkey{TMonkey}"/>
    public abstract class ResoniteEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6>
            : ResoniteEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>, IEventSource<TEvent6>
        where TMonkey : ResoniteEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6>, new()
        where TEvent1 : SyncEvent
        where TEvent2 : SyncEvent
        where TEvent3 : SyncEvent
        where TEvent4 : SyncEvent
        where TEvent5 : SyncEvent
        where TEvent6 : SyncEvent
    {
        private static EventDispatching<TEvent6>? _eventDispatching;

        /// <inheritdoc/>
        protected ResoniteEventSourceMonkey()
        { }

        /// <summary>
        /// Dispatches the <typeparamref name="TEvent6"/> with the given data.
        /// </summary>
        /// <inheritdoc cref="ResoniteEventSourceMonkey{TMonkey, TEvent}.Dispatch"/>
        protected void Dispatch(TEvent6 eventData)
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

        event EventDispatching<TEvent6>? IEventSource<TEvent6>.Dispatching
        {
            add => _eventDispatching += value;
            remove => _eventDispatching -= value;
        }
    }
}