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
    /// Specifically, to act as a <see cref="ICancelableAsyncEventSource{TEvent}">cancelable async event source</see> for <typeparamref name="TEvent"/>s.
    /// </summary>
    /// <inheritdoc/>
    public abstract class ResoniteCancelableAsyncEventSourceMonkey<TMonkey, TEvent>
            : ResoniteMonkey<TMonkey>, ICancelableAsyncEventSource<TEvent>
        where TMonkey : ResoniteCancelableAsyncEventSourceMonkey<TMonkey, TEvent>, new()
        where TEvent : CancelableAsyncEvent
    {
        private static CancelableAsyncEventDispatching<TEvent>? _eventDispatching;

        /// <inheritdoc/>
        protected ResoniteCancelableAsyncEventSourceMonkey()
        { }

        /// <summary>
        /// Dispatches the <typeparamref name="TEvent"/> with the given data.
        /// </summary>
        /// <param name="eventData">The event data to dispatch.</param>
        protected static async Task DispatchAsync(TEvent eventData)
            => await (_eventDispatching?.Invoke(eventData) ?? Task.CompletedTask);

        /// <remarks>
        /// Override <see cref="ResoniteMonkey{TMonkey}.OnLoaded">OnLoaded</see>() to patch before anything is initialized,
        /// but strongly consider also overriding this method if you do that.<br/>
        /// Otherwise your patches will be applied twice, if you're using <c>[<see cref="HarmonyPatchCategory"/>(nameof(MyPatcher))]</c> attributes.
        /// <para/>
        /// <i>By default:</i> <see cref="Mod.RegisterEventSource{TEvent}(ICancelableAsyncEventSource{TEvent})">Registers</see>
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
        /// <i>By default:</i> <see cref="Mod.UnregisterEventSource{TEvent}(ICancelableAsyncEventSource{TEvent})">Unregisters</see>
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

        event CancelableAsyncEventDispatching<TEvent>? ICancelableAsyncEventSource<TEvent>.Dispatching
        {
            add => _eventDispatching += value;
            remove => _eventDispatching -= value;
        }
    }

    /// <inheritdoc cref="ResoniteCancelableAsyncEventSourceMonkey{TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ResoniteCancelableAsyncEventSourceMonkey<TMonkey, TEvent1, TEvent2>
            : ResoniteCancelableAsyncEventSourceMonkey<TMonkey, TEvent1>, ICancelableAsyncEventSource<TEvent2>
        where TMonkey : ResoniteCancelableAsyncEventSourceMonkey<TMonkey, TEvent1, TEvent2>, new()
        where TEvent1 : CancelableAsyncEvent
        where TEvent2 : CancelableAsyncEvent
    {
        private static CancelableAsyncEventDispatching<TEvent2>? _eventDispatching;

        /// <inheritdoc/>
        protected ResoniteCancelableAsyncEventSourceMonkey()
        { }

        /// <summary>
        /// Dispatches the <typeparamref name="TEvent2"/> with the given data.
        /// </summary>
        /// <inheritdoc cref="ResoniteCancelableAsyncEventSourceMonkey{TMonkey, TEvent}.DispatchAsync"/>
        protected static async Task DispatchAsync(TEvent2 eventData)
            => await (_eventDispatching?.Invoke(eventData) ?? Task.CompletedTask);

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

        event CancelableAsyncEventDispatching<TEvent2>? ICancelableAsyncEventSource<TEvent2>.Dispatching
        {
            add => _eventDispatching += value;
            remove => _eventDispatching -= value;
        }
    }

    /// <inheritdoc cref="ResoniteCancelableAsyncEventSourceMonkey{TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ResoniteCancelableAsyncEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3>
            : ResoniteCancelableAsyncEventSourceMonkey<TMonkey, TEvent1, TEvent2>, ICancelableAsyncEventSource<TEvent3>
        where TMonkey : ResoniteCancelableAsyncEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3>, new()
        where TEvent1 : CancelableAsyncEvent
        where TEvent2 : CancelableAsyncEvent
        where TEvent3 : CancelableAsyncEvent
    {
        private static CancelableAsyncEventDispatching<TEvent3>? _eventDispatching;

        /// <inheritdoc/>
        protected ResoniteCancelableAsyncEventSourceMonkey()
        { }

        /// <summary>
        /// Dispatches the <typeparamref name="TEvent3"/> with the given data.
        /// </summary>
        /// <inheritdoc cref="ResoniteCancelableAsyncEventSourceMonkey{TMonkey, TEvent}.DispatchAsync"/>
        protected static async Task DispatchAsync(TEvent3 eventData)
            => await (_eventDispatching?.Invoke(eventData) ?? Task.CompletedTask);

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

        event CancelableAsyncEventDispatching<TEvent3>? ICancelableAsyncEventSource<TEvent3>.Dispatching
        {
            add => _eventDispatching += value;
            remove => _eventDispatching -= value;
        }
    }

    /// <inheritdoc cref="ResoniteCancelableAsyncEventSourceMonkey{TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ResoniteCancelableAsyncEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4>
            : ResoniteCancelableAsyncEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3>, ICancelableAsyncEventSource<TEvent4>
        where TMonkey : ResoniteCancelableAsyncEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4>, new()
        where TEvent1 : CancelableAsyncEvent
        where TEvent2 : CancelableAsyncEvent
        where TEvent3 : CancelableAsyncEvent
        where TEvent4 : CancelableAsyncEvent
    {
        private static CancelableAsyncEventDispatching<TEvent4>? _eventDispatching;

        /// <inheritdoc/>
        protected ResoniteCancelableAsyncEventSourceMonkey()
        { }

        /// <summary>
        /// Dispatches the <typeparamref name="TEvent4"/> with the given data.
        /// </summary>
        /// <inheritdoc cref="ResoniteCancelableAsyncEventSourceMonkey{TMonkey, TEvent}.DispatchAsync"/>
        protected static async Task DispatchAsync(TEvent4 eventData)
            => await (_eventDispatching?.Invoke(eventData) ?? Task.CompletedTask);

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

        event CancelableAsyncEventDispatching<TEvent4>? ICancelableAsyncEventSource<TEvent4>.Dispatching
        {
            add => _eventDispatching += value;
            remove => _eventDispatching -= value;
        }
    }

    /// <inheritdoc cref="ResoniteCancelableAsyncEventSourceMonkey{TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ResoniteCancelableAsyncEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>
            : ResoniteCancelableAsyncEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4>, ICancelableAsyncEventSource<TEvent5>
        where TMonkey : ResoniteCancelableAsyncEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>, new()
        where TEvent1 : CancelableAsyncEvent
        where TEvent2 : CancelableAsyncEvent
        where TEvent3 : CancelableAsyncEvent
        where TEvent4 : CancelableAsyncEvent
        where TEvent5 : CancelableAsyncEvent
    {
        private static CancelableAsyncEventDispatching<TEvent5>? _eventDispatching;

        /// <inheritdoc/>
        protected ResoniteCancelableAsyncEventSourceMonkey()
        { }

        /// <summary>
        /// Dispatches the <typeparamref name="TEvent5"/> with the given data.
        /// </summary>
        /// <inheritdoc cref="ResoniteCancelableAsyncEventSourceMonkey{TMonkey, TEvent}.DispatchAsync"/>
        protected static async Task DispatchAsync(TEvent5 eventData)
            => await (_eventDispatching?.Invoke(eventData) ?? Task.CompletedTask);

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

        event CancelableAsyncEventDispatching<TEvent5>? ICancelableAsyncEventSource<TEvent5>.Dispatching
        {
            add => _eventDispatching += value;
            remove => _eventDispatching -= value;
        }
    }

    /// <summary>
    /// Represents the base class for patchers that run after Resonite's assemblies have been loaded and that hook into the game's lifecycle.<br/>
    /// Specifically, to act as a <see cref="ICancelableAsyncEventSource{TEvent}">cancelable async event source</see> for the <c>TEvent</c> generic parameter(s).
    /// </summary>
    /// <typeparam name="TMonkey">The type of the actual patcher.</typeparam>
    /// <typeparam name="TEvent1">The first <see cref="CancelableAsyncEvent"/> type to dispatch.</typeparam>
    /// <typeparam name="TEvent2">The second <see cref="CancelableAsyncEvent"/> type to dispatch.</typeparam>
    /// <typeparam name="TEvent3">The third <see cref="CancelableAsyncEvent"/> type to dispatch.</typeparam>
    /// <typeparam name="TEvent4">The fourth <see cref="CancelableAsyncEvent"/> type to dispatch.</typeparam>
    /// <typeparam name="TEvent5">The fifth <see cref="CancelableAsyncEvent"/> type to dispatch.</typeparam>
    /// <typeparam name="TEvent6">The sixth <see cref="CancelableAsyncEvent"/> type to dispatch.</typeparam>
    /// <inheritdoc cref="ResoniteMonkey{TMonkey}"/>
    public abstract class ResoniteCancelableAsyncEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6>
            : ResoniteCancelableAsyncEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>, ICancelableAsyncEventSource<TEvent6>
        where TMonkey : ResoniteCancelableAsyncEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6>, new()
        where TEvent1 : CancelableAsyncEvent
        where TEvent2 : CancelableAsyncEvent
        where TEvent3 : CancelableAsyncEvent
        where TEvent4 : CancelableAsyncEvent
        where TEvent5 : CancelableAsyncEvent
        where TEvent6 : CancelableAsyncEvent
    {
        private static CancelableAsyncEventDispatching<TEvent6>? _eventDispatching;

        /// <inheritdoc/>
        protected ResoniteCancelableAsyncEventSourceMonkey()
        { }

        /// <summary>
        /// Dispatches the <typeparamref name="TEvent6"/> with the given data.
        /// </summary>
        /// <inheritdoc cref="ResoniteCancelableAsyncEventSourceMonkey{TMonkey, TEvent}.DispatchAsync"/>
        protected static async Task DispatchAsync(TEvent6 eventData)
            => await (_eventDispatching?.Invoke(eventData) ?? Task.CompletedTask);

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

        event CancelableAsyncEventDispatching<TEvent6>? ICancelableAsyncEventSource<TEvent6>.Dispatching
        {
            add => _eventDispatching += value;
            remove => _eventDispatching -= value;
        }
    }
}