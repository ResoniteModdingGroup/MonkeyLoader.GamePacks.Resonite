using HarmonyLib;
using MonkeyLoader.Events;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Represents the base class for patchers that run after Resonite's assemblies have been loaded and that hook into the game's lifecycle.<br/>
    /// Specifically, to act as an <see cref="ICancelableAsyncEventHandler{TEvent}">async event handler</see> for the cancelable <typeparamref name="TEvent"/>.
    /// </summary>
    /// <typeparam name="TMonkey">The type of the actual patcher.</typeparam>
    /// <typeparam name="TEvent">The <see cref="CancelableAsyncEvent"/> type to handle.</typeparam>
    /// <inheritdoc/>
    public abstract class ResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TEvent>
            : ResoniteMonkey<TMonkey>, ICancelableAsyncEventHandler<TEvent>
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
        /// this monkey as an event handler and removes all <see cref="Harmony"/> patches done
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

    /// <inheritdoc cref="ResoniteCancelableAsyncEventHandlerMonkey{TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2>
            : ResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TEvent1>, ICancelableAsyncEventHandler<TEvent2>
        where TMonkey : ResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2>, new()
        where TEvent1 : CancelableAsyncEvent
        where TEvent2 : CancelableAsyncEvent
    {
        Task ICancelableAsyncEventHandler<TEvent2>.Handle(TEvent2 eventData)
        {
            if (AppliesTo(eventData))
                return Handle(eventData);

            return Task.CompletedTask;
        }

        /// <inheritdoc cref="ResoniteCancelableAsyncEventHandlerMonkey{TMonkey, TEvent}.AppliesTo"/>
        protected virtual bool AppliesTo(TEvent2 eventData) => Enabled;

        /// <inheritdoc cref="ResoniteCancelableAsyncEventHandlerMonkey{TMonkey, TEvent}.Handle"/>
        protected abstract Task Handle(TEvent2 eventData);

        /// <inheritdoc/>
        protected override bool OnEngineReady()
        {
            Mod.RegisterEventHandler<TEvent2>(this);

            return base.OnEngineReady();
        }

        /// <inheritdoc/>
        protected override bool OnShutdown(bool applicationExiting)
        {
            if (!applicationExiting)
                Mod.UnregisterEventHandler<TEvent2>(this);

            return base.OnShutdown(applicationExiting);
        }
    }

    /// <inheritdoc cref="ResoniteCancelableAsyncEventHandlerMonkey{TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3>
            : ResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2>, ICancelableAsyncEventHandler<TEvent3>
        where TMonkey : ResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3>, new()
        where TEvent1 : CancelableAsyncEvent
        where TEvent2 : CancelableAsyncEvent
        where TEvent3 : CancelableAsyncEvent
    {
        Task ICancelableAsyncEventHandler<TEvent3>.Handle(TEvent3 eventData)
        {
            if (AppliesTo(eventData))
                return Handle(eventData);

            return Task.CompletedTask;
        }

        /// <inheritdoc cref="ResoniteCancelableAsyncEventHandlerMonkey{TMonkey, TEvent}.AppliesTo"/>
        protected virtual bool AppliesTo(TEvent3 eventData) => Enabled;

        /// <inheritdoc cref="ResoniteCancelableAsyncEventHandlerMonkey{TMonkey, TEvent}.Handle"/>
        protected abstract Task Handle(TEvent3 eventData);

        /// <inheritdoc/>
        protected override bool OnEngineReady()
        {
            Mod.RegisterEventHandler<TEvent3>(this);

            return base.OnEngineReady();
        }

        /// <inheritdoc/>
        protected override bool OnShutdown(bool applicationExiting)
        {
            if (!applicationExiting)
                Mod.UnregisterEventHandler<TEvent3>(this);

            return base.OnShutdown(applicationExiting);
        }
    }

    /// <inheritdoc cref="ResoniteCancelableAsyncEventHandlerMonkey{TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4> : ResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3>,
            ICancelableAsyncEventHandler<TEvent4>
        where TMonkey : ResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4>, new()
        where TEvent1 : CancelableAsyncEvent
        where TEvent2 : CancelableAsyncEvent
        where TEvent3 : CancelableAsyncEvent
        where TEvent4 : CancelableAsyncEvent
    {
        Task ICancelableAsyncEventHandler<TEvent4>.Handle(TEvent4 eventData)
        {
            if (AppliesTo(eventData))
                return Handle(eventData);

            return Task.CompletedTask;
        }

        /// <inheritdoc cref="ResoniteCancelableAsyncEventHandlerMonkey{TMonkey, TEvent}.AppliesTo"/>
        protected virtual bool AppliesTo(TEvent4 eventData) => Enabled;

        /// <inheritdoc cref="ResoniteCancelableAsyncEventHandlerMonkey{TMonkey, TEvent}.Handle"/>
        protected abstract Task Handle(TEvent4 eventData);

        /// <inheritdoc/>
        protected override bool OnEngineReady()
        {
            Mod.RegisterEventHandler<TEvent4>(this);

            return base.OnEngineReady();
        }

        /// <inheritdoc/>
        protected override bool OnShutdown(bool applicationExiting)
        {
            if (!applicationExiting)
                Mod.UnregisterEventHandler<TEvent4>(this);

            return base.OnShutdown(applicationExiting);
        }
    }

    /// <inheritdoc cref="ResoniteCancelableAsyncEventHandlerMonkey{TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>
            : ResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4>, ICancelableAsyncEventHandler<TEvent5>
        where TMonkey : ResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>, new()
        where TEvent1 : CancelableAsyncEvent
        where TEvent2 : CancelableAsyncEvent
        where TEvent3 : CancelableAsyncEvent
        where TEvent4 : CancelableAsyncEvent
        where TEvent5 : CancelableAsyncEvent
    {
        Task ICancelableAsyncEventHandler<TEvent5>.Handle(TEvent5 eventData)
        {
            if (AppliesTo(eventData))
                return Handle(eventData);

            return Task.CompletedTask;
        }

        /// <inheritdoc cref="ResoniteCancelableAsyncEventHandlerMonkey{TMonkey, TEvent}.AppliesTo"/>
        protected virtual bool AppliesTo(TEvent5 eventData) => Enabled;

        /// <inheritdoc cref="ResoniteCancelableAsyncEventHandlerMonkey{TMonkey, TEvent}.Handle"/>
        protected abstract Task Handle(TEvent5 eventData);

        /// <inheritdoc/>
        protected override bool OnEngineReady()
        {
            Mod.RegisterEventHandler<TEvent5>(this);

            return base.OnEngineReady();
        }

        /// <inheritdoc/>
        protected override bool OnShutdown(bool applicationExiting)
        {
            if (!applicationExiting)
                Mod.UnregisterEventHandler<TEvent5>(this);

            return base.OnShutdown(applicationExiting);
        }
    }

    /// <summary>
    /// Represents the base class for patchers that run after Resonite's assemblies have been loaded and that hook into the game's lifecycle.<br/>
    /// Specifically, to act as an <see cref="ICancelableAsyncEventHandler{TEvent}">async event handler</see> for the cancelable <c>TEvent</c> generic parameter(s).
    /// </summary>
    /// <typeparam name="TMonkey">The type of the actual patcher.</typeparam>
    /// <typeparam name="TEvent1">The first <see cref="CancelableAsyncEvent"/> type to handle.</typeparam>
    /// <typeparam name="TEvent2">The second <see cref="CancelableAsyncEvent"/> type to handle.</typeparam>
    /// <typeparam name="TEvent3">The third <see cref="CancelableAsyncEvent"/> type to handle.</typeparam>
    /// <typeparam name="TEvent4">The fourth <see cref="CancelableAsyncEvent"/> type to handle.</typeparam>
    /// <typeparam name="TEvent5">The fifth <see cref="CancelableAsyncEvent"/> type to handle.</typeparam>
    /// <typeparam name="TEvent6">The sixth <see cref="CancelableAsyncEvent"/> type to handle.</typeparam>
    /// <inheritdoc cref="ResoniteMonkey{TMonkey}"/>
    public abstract class ResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6>
            : ResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>, ICancelableAsyncEventHandler<TEvent6>
        where TMonkey : ResoniteCancelableAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6>, new()
        where TEvent1 : CancelableAsyncEvent
        where TEvent2 : CancelableAsyncEvent
        where TEvent3 : CancelableAsyncEvent
        where TEvent4 : CancelableAsyncEvent
        where TEvent5 : CancelableAsyncEvent
        where TEvent6 : CancelableAsyncEvent
    {
        Task ICancelableAsyncEventHandler<TEvent6>.Handle(TEvent6 eventData)
        {
            if (AppliesTo(eventData))
                return Handle(eventData);

            return Task.CompletedTask;
        }

        /// <inheritdoc cref="ResoniteCancelableAsyncEventHandlerMonkey{TMonkey, TEvent}.AppliesTo"/>
        protected virtual bool AppliesTo(TEvent6 eventData) => Enabled;

        /// <inheritdoc cref="ResoniteCancelableAsyncEventHandlerMonkey{TMonkey, TEvent}.Handle"/>
        protected abstract Task Handle(TEvent6 eventData);

        /// <inheritdoc/>
        protected override bool OnEngineReady()
        {
            Mod.RegisterEventHandler<TEvent6>(this);

            return base.OnEngineReady();
        }

        /// <inheritdoc/>
        protected override bool OnShutdown(bool applicationExiting)
        {
            if (!applicationExiting)
                Mod.UnregisterEventHandler<TEvent6>(this);

            return base.OnShutdown(applicationExiting);
        }
    }
}