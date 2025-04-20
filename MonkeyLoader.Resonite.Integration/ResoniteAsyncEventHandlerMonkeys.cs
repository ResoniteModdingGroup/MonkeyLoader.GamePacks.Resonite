using HarmonyLib;
using MonkeyLoader.Events;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Represents the base class for patchers that run after Resonite's assemblies have been loaded and that hook into the game's lifecycle.<br/>
    /// Specifically, to act as an <see cref="IAsyncEventHandler{TEvent}">async event handler</see> for the <typeparamref name="TEvent"/>.
    /// </summary>
    /// <typeparam name="TMonkey">The type of the actual patcher.</typeparam>
    /// <typeparam name="TEvent">The <see cref="AsyncEvent"/> type to handle.</typeparam>
    /// <inheritdoc/>
    public abstract class ResoniteAsyncEventHandlerMonkey<TMonkey, TEvent>
            : ResoniteMonkey<TMonkey>, IAsyncEventHandler<TEvent>
        where TMonkey : ResoniteAsyncEventHandlerMonkey<TMonkey, TEvent>, new()
        where TEvent : AsyncEvent
    {
        /// <inheritdoc/>
        public abstract int Priority { get; }

        /// <inheritdoc/>
        protected ResoniteAsyncEventHandlerMonkey()
        { }

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

    /// <inheritdoc cref="ResoniteAsyncEventHandlerMonkey{TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ResoniteAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2>
            : ResoniteAsyncEventHandlerMonkey<TMonkey, TEvent1>, IAsyncEventHandler<TEvent2>
        where TMonkey : ResoniteAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2>, new()
        where TEvent1 : AsyncEvent
        where TEvent2 : AsyncEvent
    {
        /// <inheritdoc/>
        protected ResoniteAsyncEventHandlerMonkey()
        { }

        Task IAsyncEventHandler<TEvent2>.Handle(TEvent2 eventData)
        {
            if (AppliesTo(eventData))
                return Handle(eventData);

            return Task.CompletedTask;
        }

        /// <inheritdoc cref="ResoniteAsyncEventHandlerMonkey{TMonkey, TEvent}.AppliesTo"/>
        protected virtual bool AppliesTo(TEvent2 eventData) => Enabled;

        /// <inheritdoc cref="ResoniteAsyncEventHandlerMonkey{TMonkey, TEvent}.Handle"/>
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

    /// <inheritdoc cref="ResoniteAsyncEventHandlerMonkey{TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ResoniteAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3>
            : ResoniteAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2>, IAsyncEventHandler<TEvent3>
        where TMonkey : ResoniteAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3>, new()
        where TEvent1 : AsyncEvent
        where TEvent2 : AsyncEvent
        where TEvent3 : AsyncEvent
    {
        /// <inheritdoc/>
        protected ResoniteAsyncEventHandlerMonkey()
        { }

        Task IAsyncEventHandler<TEvent3>.Handle(TEvent3 eventData)
        {
            if (AppliesTo(eventData))
                return Handle(eventData);

            return Task.CompletedTask;
        }

        /// <inheritdoc cref="ResoniteAsyncEventHandlerMonkey{TMonkey, TEvent}.AppliesTo"/>
        protected virtual bool AppliesTo(TEvent3 eventData) => Enabled;

        /// <inheritdoc cref="ResoniteAsyncEventHandlerMonkey{TMonkey, TEvent}.Handle"/>
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

    /// <inheritdoc cref="ResoniteAsyncEventHandlerMonkey{TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ResoniteAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4>
        : ResoniteAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3>, IAsyncEventHandler<TEvent4>
        where TMonkey : ResoniteAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4>, new()
        where TEvent1 : AsyncEvent
        where TEvent2 : AsyncEvent
        where TEvent3 : AsyncEvent
        where TEvent4 : AsyncEvent
    {
        /// <inheritdoc/>
        protected ResoniteAsyncEventHandlerMonkey()
        { }

        Task IAsyncEventHandler<TEvent4>.Handle(TEvent4 eventData)
        {
            if (AppliesTo(eventData))
                return Handle(eventData);

            return Task.CompletedTask;
        }

        /// <inheritdoc cref="ResoniteAsyncEventHandlerMonkey{TMonkey, TEvent}.AppliesTo"/>
        protected virtual bool AppliesTo(TEvent4 eventData) => Enabled;

        /// <inheritdoc cref="ResoniteAsyncEventHandlerMonkey{TMonkey, TEvent}.Handle"/>
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

    /// <inheritdoc cref="ResoniteAsyncEventHandlerMonkey{TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ResoniteAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>
            : ResoniteAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4>, IAsyncEventHandler<TEvent5>
        where TMonkey : ResoniteAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>, new()
        where TEvent1 : AsyncEvent
        where TEvent2 : AsyncEvent
        where TEvent3 : AsyncEvent
        where TEvent4 : AsyncEvent
        where TEvent5 : AsyncEvent
    {
        /// <inheritdoc/>
        protected ResoniteAsyncEventHandlerMonkey()
        { }

        Task IAsyncEventHandler<TEvent5>.Handle(TEvent5 eventData)
        {
            if (AppliesTo(eventData))
                return Handle(eventData);

            return Task.CompletedTask;
        }

        /// <inheritdoc cref="ResoniteAsyncEventHandlerMonkey{TMonkey, TEvent}.AppliesTo"/>
        protected virtual bool AppliesTo(TEvent5 eventData) => Enabled;

        /// <inheritdoc cref="ResoniteAsyncEventHandlerMonkey{TMonkey, TEvent}.Handle"/>
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
    /// Specifically, to act as an <see cref="IAsyncEventHandler{TEvent}">async event handler</see> for the <c>TEvent</c> generic parameter(s).
    /// </summary>
    /// <typeparam name="TMonkey">The type of the actual patcher.</typeparam>
    /// <typeparam name="TEvent1">The first <see cref="AsyncEvent"/> type to handle.</typeparam>
    /// <typeparam name="TEvent2">The second <see cref="AsyncEvent"/> type to handle.</typeparam>
    /// <typeparam name="TEvent3">The third <see cref="AsyncEvent"/> type to handle.</typeparam>
    /// <typeparam name="TEvent4">The fourth <see cref="AsyncEvent"/> type to handle.</typeparam>
    /// <typeparam name="TEvent5">The fifth <see cref="AsyncEvent"/> type to handle.</typeparam>
    /// <typeparam name="TEvent6">The sixth <see cref="AsyncEvent"/> type to handle.</typeparam>
    /// <inheritdoc cref="ResoniteMonkey{TMonkey}"/>
    public abstract class ResoniteAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6>
            : ResoniteAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>, IAsyncEventHandler<TEvent6>
        where TMonkey : ResoniteAsyncEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6>, new()
        where TEvent1 : AsyncEvent
        where TEvent2 : AsyncEvent
        where TEvent3 : AsyncEvent
        where TEvent4 : AsyncEvent
        where TEvent5 : AsyncEvent
        where TEvent6 : AsyncEvent
    {
        /// <inheritdoc/>
        protected ResoniteAsyncEventHandlerMonkey()
        { }

        Task IAsyncEventHandler<TEvent6>.Handle(TEvent6 eventData)
        {
            if (AppliesTo(eventData))
                return Handle(eventData);

            return Task.CompletedTask;
        }

        /// <inheritdoc cref="ResoniteAsyncEventHandlerMonkey{TMonkey, TEvent}.AppliesTo"/>
        protected virtual bool AppliesTo(TEvent6 eventData) => Enabled;

        /// <inheritdoc cref="ResoniteAsyncEventHandlerMonkey{TMonkey, TEvent}.Handle"/>
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