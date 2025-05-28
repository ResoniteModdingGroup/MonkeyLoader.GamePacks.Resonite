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
    /// Specifically, to act as an <see cref="IAsyncEventSource{TEvent}">async event source</see> for <typeparamref name="TEvent"/>s.
    /// </summary>
    /// <inheritdoc/>
    public abstract class ResoniteAsyncEventSourceMonkey<TMonkey, TEvent>
            : ResoniteMonkey<TMonkey>, IAsyncEventSource<TEvent>
        where TMonkey : ResoniteAsyncEventSourceMonkey<TMonkey, TEvent>, new()
        where TEvent : AsyncEvent
    {
        private static AsyncEventDispatching<TEvent>? _eventDispatching;

        /// <inheritdoc/>
        protected ResoniteAsyncEventSourceMonkey()
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
        /// <i>By default:</i> <see cref="Mod.RegisterEventSource{TEvent}(IAsyncEventSource{TEvent})">Registers</see>
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
        /// <i>By default:</i> <see cref="Mod.UnregisterEventSource{TEvent}(IAsyncEventSource{TEvent})">Unregisters</see>
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

        event AsyncEventDispatching<TEvent>? IAsyncEventSource<TEvent>.Dispatching
        {
            add => _eventDispatching += value;
            remove => _eventDispatching -= value;
        }
    }

    /// <inheritdoc cref="ResoniteAsyncEventSourceMonkey{TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ResoniteAsyncEventSourceMonkey<TMonkey, TEvent1, TEvent2>
            : ResoniteAsyncEventSourceMonkey<TMonkey, TEvent1>, IAsyncEventSource<TEvent2>
        where TMonkey : ResoniteAsyncEventSourceMonkey<TMonkey, TEvent1, TEvent2>, new()
        where TEvent1 : AsyncEvent
        where TEvent2 : AsyncEvent
    {
        private static AsyncEventDispatching<TEvent2>? _eventDispatching;

        /// <inheritdoc/>
        protected ResoniteAsyncEventSourceMonkey()
        { }

        /// <summary>
        /// Dispatches the <typeparamref name="TEvent2"/> with the given data.
        /// </summary>
        /// <inheritdoc cref="ResoniteAsyncEventSourceMonkey{TMonkey, TEvent}.DispatchAsync"/>
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

        event AsyncEventDispatching<TEvent2>? IAsyncEventSource<TEvent2>.Dispatching
        {
            add => _eventDispatching += value;
            remove => _eventDispatching -= value;
        }
    }

    /// <inheritdoc cref="ResoniteAsyncEventSourceMonkey{TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ResoniteAsyncEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3>
            : ResoniteAsyncEventSourceMonkey<TMonkey, TEvent1, TEvent2>, IAsyncEventSource<TEvent3>
        where TMonkey : ResoniteAsyncEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3>, new()
        where TEvent1 : AsyncEvent
        where TEvent2 : AsyncEvent
        where TEvent3 : AsyncEvent
    {
        private static AsyncEventDispatching<TEvent3>? _eventDispatching;

        /// <inheritdoc/>
        protected ResoniteAsyncEventSourceMonkey()
        { }

        /// <summary>
        /// Dispatches the <typeparamref name="TEvent3"/> with the given data.
        /// </summary>
        /// <inheritdoc cref="ResoniteAsyncEventSourceMonkey{TMonkey, TEvent}.DispatchAsync"/>
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

        event AsyncEventDispatching<TEvent3>? IAsyncEventSource<TEvent3>.Dispatching
        {
            add => _eventDispatching += value;
            remove => _eventDispatching -= value;
        }
    }

    /// <inheritdoc cref="ResoniteAsyncEventSourceMonkey{TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ResoniteAsyncEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4>
            : ResoniteAsyncEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3>, IAsyncEventSource<TEvent4>
        where TMonkey : ResoniteAsyncEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4>, new()
        where TEvent1 : AsyncEvent
        where TEvent2 : AsyncEvent
        where TEvent3 : AsyncEvent
        where TEvent4 : AsyncEvent
    {
        private static AsyncEventDispatching<TEvent4>? _eventDispatching;

        /// <inheritdoc/>
        protected ResoniteAsyncEventSourceMonkey()
        { }

        /// <summary>
        /// Dispatches the <typeparamref name="TEvent4"/> with the given data.
        /// </summary>
        /// <inheritdoc cref="ResoniteAsyncEventSourceMonkey{TMonkey, TEvent}.DispatchAsync"/>
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

        event AsyncEventDispatching<TEvent4>? IAsyncEventSource<TEvent4>.Dispatching
        {
            add => _eventDispatching += value;
            remove => _eventDispatching -= value;
        }
    }

    /// <inheritdoc cref="ResoniteAsyncEventSourceMonkey{TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ResoniteAsyncEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>
            : ResoniteAsyncEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4>, IAsyncEventSource<TEvent5>
        where TMonkey : ResoniteAsyncEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>, new()
        where TEvent1 : AsyncEvent
        where TEvent2 : AsyncEvent
        where TEvent3 : AsyncEvent
        where TEvent4 : AsyncEvent
        where TEvent5 : AsyncEvent
    {
        private static AsyncEventDispatching<TEvent5>? _eventDispatching;

        /// <inheritdoc/>
        protected ResoniteAsyncEventSourceMonkey()
        { }

        /// <summary>
        /// Dispatches the <typeparamref name="TEvent5"/> with the given data.
        /// </summary>
        /// <inheritdoc cref="ResoniteAsyncEventSourceMonkey{TMonkey, TEvent}.DispatchAsync"/>
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

        event AsyncEventDispatching<TEvent5>? IAsyncEventSource<TEvent5>.Dispatching
        {
            add => _eventDispatching += value;
            remove => _eventDispatching -= value;
        }
    }

    /// <summary>
    /// Represents the base class for patchers that run after Resonite's assemblies have been loaded and that hook into the game's lifecycle.<br/>
    /// Specifically, to act as an <see cref="IAsyncEventSource{TEvent}">async event source</see> for the <c>TEvent</c> generic parameter(s).
    /// </summary>
    /// <typeparam name="TMonkey">The type of the actual patcher.</typeparam>
    /// <typeparam name="TEvent1">The first <see cref="AsyncEvent"/> type to dispatch.</typeparam>
    /// <typeparam name="TEvent2">The second <see cref="AsyncEvent"/> type to dispatch.</typeparam>
    /// <typeparam name="TEvent3">The third <see cref="AsyncEvent"/> type to dispatch.</typeparam>
    /// <typeparam name="TEvent4">The fourth <see cref="AsyncEvent"/> type to dispatch.</typeparam>
    /// <typeparam name="TEvent5">The fifth <see cref="AsyncEvent"/> type to dispatch.</typeparam>
    /// <typeparam name="TEvent6">The sixth <see cref="AsyncEvent"/> type to dispatch.</typeparam>
    /// <inheritdoc cref="ResoniteMonkey{TMonkey}"/>
    public abstract class ResoniteAsyncEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6>
            : ResoniteAsyncEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>, IAsyncEventSource<TEvent6>
        where TMonkey : ResoniteAsyncEventSourceMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6>, new()
        where TEvent1 : AsyncEvent
        where TEvent2 : AsyncEvent
        where TEvent3 : AsyncEvent
        where TEvent4 : AsyncEvent
        where TEvent5 : AsyncEvent
        where TEvent6 : AsyncEvent
    {
        private static AsyncEventDispatching<TEvent6>? _eventDispatching;

        /// <inheritdoc/>
        protected ResoniteAsyncEventSourceMonkey()
        { }

        /// <summary>
        /// Dispatches the <typeparamref name="TEvent6"/> with the given data.
        /// </summary>
        /// <inheritdoc cref="ResoniteAsyncEventSourceMonkey{TMonkey, TEvent}.DispatchAsync"/>
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

        event AsyncEventDispatching<TEvent6>? IAsyncEventSource<TEvent6>.Dispatching
        {
            add => _eventDispatching += value;
            remove => _eventDispatching -= value;
        }
    }
}