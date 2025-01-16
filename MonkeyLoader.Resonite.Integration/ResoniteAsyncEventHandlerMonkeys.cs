using HarmonyLib;
using MonkeyLoader.Events;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Represents the base class for patchers that run after Resonite's assemblies have been loaded and that hook into the game's lifecycle.<br/>
    /// Specifically, to act as an <see cref="IAsyncEventHandler{TEvent}">async event handler</see> for <typeparamref name="TEvent"/>s.
    /// </summary>
    /// <inheritdoc/>
    public abstract class ResoniteAsyncEventHandlerMonkey<TMonkey, TEvent> : ResoniteMonkey<TMonkey>, IAsyncEventHandler<TEvent>
        where TMonkey : ResoniteAsyncEventHandlerMonkey<TMonkey, TEvent>, new()
        where TEvent : AsyncEvent
    {
        /// <inheritdoc/>
        public abstract int Priority { get; }

        Task IAsyncEventHandler<TEvent>.Handle(TEvent eventData)
        {
            if (AppliesTo(eventData))
                return Handle(eventData);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Determines whether the given event should be handled
        /// by this event handler based on its data.
        /// </summary>
        /// <remarks>
        /// <i>By default:</i> Returns this monkey's <see cref="MonkeyBase{TMonkey}.Enabled">Enabled</see> state.
        /// </remarks>
        /// <param name="eventData">An object containing all the relevant information for the event.</param>
        /// <returns><c>true</c> if this event handler applies to the event; otherwise, <c>false</c>.</returns>
        protected virtual bool AppliesTo(TEvent eventData) => Enabled;

        /// <summary>
        /// Handles the given event based on its data, if <see cref="AppliesTo">AppliesTo</see> returned <c>true</c>.
        /// </summary>
        /// <param name="eventData">An object containing all the relevant information for the applicable event.</param>
        protected abstract Task Handle(TEvent eventData);

        /// <remarks>
        /// Override <see cref="ResoniteMonkey{TMonkey}.OnLoaded">OnLoaded</see>() to patch before anything is initialized,
        /// but strongly consider also overriding this method if you do that.<br/>
        /// Otherwise your patches will be applied twice, if you're using <c>[<see cref="HarmonyPatchCategory"/>(nameof(MyPatcher))]</c> attributes.
        /// <para/>
        /// <i>By default:</i> <see cref="Mod.RegisterEventHandler{TEvent}(IAsyncEventHandler{TEvent})">Registers</see>
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
        /// <i>By default:</i> <see cref="Mod.UnregisterEventHandler{TEvent}(IAsyncEventHandler{TEvent})">Unregisters</see>
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
    /// Specifically, to act as an <see cref="ICancelableAsyncEventHandler{TEvent}">async event handler</see> for cancelable <typeparamref name="TEvent"/>s.
    /// </summary>
    /// <inheritdoc/>
    public abstract class ResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TEvent> : ResoniteMonkey<TMonkey>, ICancelableAsyncEventHandler<TEvent>
        where TMonkey : ResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TEvent>, new()
        where TEvent : CancelableAsyncEvent
    {
        /// <inheritdoc/>
        public abstract int Priority { get; }

        /// <inheritdoc/>
        public abstract bool SkipCanceled { get; }

        Task ICancelableAsyncEventHandler<TEvent>.Handle(TEvent eventData)
        {
            if (AppliesTo(eventData))
                return Handle(eventData);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Determines whether the given event should be handled
        /// by this event handler based on its data.
        /// </summary>
        /// <remarks>
        /// <i>By default:</i> Returns this monkey's <see cref="MonkeyBase{TMonkey}.Enabled">Enabled</see> state.
        /// </remarks>
        /// <param name="eventData">An object containing all the relevant information for the event.</param>
        /// <returns><c>true</c> if this event handler applies to the event; otherwise, <c>false</c>.</returns>
        protected virtual bool AppliesTo(TEvent eventData) => Enabled;

        /// <summary>
        /// Handles the given event based on its data, if <see cref="AppliesTo">AppliesTo</see> returned <c>true</c>.
        /// </summary>
        /// <param name="eventData">An object containing all the relevant information for the applicable event.</param>
        protected abstract Task Handle(TEvent eventData);

        /// <remarks>
        /// Override <see cref="ResoniteMonkey{TMonkey}.OnLoaded">OnLoaded</see>() to patch before anything is initialized,
        /// but strongly consider also overriding this method if you do that.<br/>
        /// Otherwise your patches will be applied twice, if you're using <c>[<see cref="HarmonyPatchCategory"/>(nameof(MyPatcher))]</c> attributes.
        /// <para/>
        /// <i>By default:</i> <see cref="Mod.RegisterEventHandler{TEvent}(ICancelableAsyncEventHandler{TEvent})">Registers</see>
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
        /// <i>By default:</i> <see cref="Mod.UnregisterEventHandler{TEvent}(ICancelableAsyncEventHandler{TEvent})">Unregisters</see>
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