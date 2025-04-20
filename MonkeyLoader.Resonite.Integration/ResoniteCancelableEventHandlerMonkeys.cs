using HarmonyLib;
using MonkeyLoader.Events;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Represents the base class for patchers that run after Resonite's assemblies have been loaded and that hook into the game's lifecycle.<br/>
    /// Specifically, to act as an <see cref="ICancelableEventHandler{TEvent}">event handler</see> for cancelable <typeparamref name="TEvent"/>s.
    /// </summary>
    /// <inheritdoc/>
    public abstract class ResoniteCancelableEventHandlerMonkey<TMonkey, TEvent>
            : ResoniteMonkey<TMonkey>, ICancelableEventHandler<TEvent>
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

    /// <inheritdoc cref="ResoniteCancelableEventHandlerMonkey{TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ResoniteCancelableEventHandlerMonkey<TMonkey, TEvent1, TEvent2>
            : ResoniteCancelableEventHandlerMonkey<TMonkey, TEvent1>, ICancelableEventHandler<TEvent2>
        where TMonkey : ResoniteCancelableEventHandlerMonkey<TMonkey, TEvent1, TEvent2>, new()
        where TEvent1 : CancelableSyncEvent
        where TEvent2 : CancelableSyncEvent
    {
        /// <inheritdoc/>
        protected ResoniteCancelableEventHandlerMonkey()
        { }

        void ICancelableEventHandler<TEvent2>.Handle(TEvent2 eventData)
        {
            if (AppliesTo(eventData))
                Handle(eventData);
        }

        /// <inheritdoc cref="ResoniteCancelableEventHandlerMonkey{TMonkey, TEvent}.AppliesTo"/>
        protected virtual bool AppliesTo(TEvent2 eventData) => Enabled;

        /// <inheritdoc cref="ResoniteCancelableEventHandlerMonkey{TMonkey, TEvent}.Handle"/>
        protected abstract void Handle(TEvent2 eventData);

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

    /// <inheritdoc cref="ResoniteCancelableEventHandlerMonkey{TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ResoniteCancelableEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3>
            : ResoniteCancelableEventHandlerMonkey<TMonkey, TEvent1, TEvent2>, ICancelableEventHandler<TEvent3>
        where TMonkey : ResoniteCancelableEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3>, new()
        where TEvent1 : CancelableSyncEvent
        where TEvent2 : CancelableSyncEvent
        where TEvent3 : CancelableSyncEvent
    {
        /// <inheritdoc/>
        protected ResoniteCancelableEventHandlerMonkey()
        { }

        void ICancelableEventHandler<TEvent3>.Handle(TEvent3 eventData)
        {
            if (AppliesTo(eventData))
                Handle(eventData);
        }

        /// <inheritdoc cref="ResoniteCancelableEventHandlerMonkey{TMonkey, TEvent}.AppliesTo"/>
        protected virtual bool AppliesTo(TEvent3 eventData) => Enabled;

        /// <inheritdoc cref="ResoniteCancelableEventHandlerMonkey{TMonkey, TEvent}.Handle"/>
        protected abstract void Handle(TEvent3 eventData);

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

    /// <inheritdoc cref="ResoniteCancelableEventHandlerMonkey{TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ResoniteCancelableEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4>
            : ResoniteCancelableEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3>, ICancelableEventHandler<TEvent4>
        where TMonkey : ResoniteCancelableEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4>, new()
        where TEvent1 : CancelableSyncEvent
        where TEvent2 : CancelableSyncEvent
        where TEvent3 : CancelableSyncEvent
        where TEvent4 : CancelableSyncEvent
    {
        /// <inheritdoc/>
        protected ResoniteCancelableEventHandlerMonkey()
        { }

        void ICancelableEventHandler<TEvent4>.Handle(TEvent4 eventData)
        {
            if (AppliesTo(eventData))
                Handle(eventData);
        }

        /// <inheritdoc cref="ResoniteCancelableEventHandlerMonkey{TMonkey, TEvent}.AppliesTo"/>
        protected virtual bool AppliesTo(TEvent4 eventData) => Enabled;

        /// <inheritdoc cref="ResoniteCancelableEventHandlerMonkey{TMonkey, TEvent}.Handle"/>
        protected abstract void Handle(TEvent4 eventData);

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

    /// <inheritdoc cref="ResoniteCancelableEventHandlerMonkey{TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6}"/>
    public abstract class ResoniteCancelableEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>
            : ResoniteCancelableEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4>, ICancelableEventHandler<TEvent5>
        where TMonkey : ResoniteCancelableEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>, new()
        where TEvent1 : CancelableSyncEvent
        where TEvent2 : CancelableSyncEvent
        where TEvent3 : CancelableSyncEvent
        where TEvent4 : CancelableSyncEvent
        where TEvent5 : CancelableSyncEvent
    {
        /// <inheritdoc/>
        protected ResoniteCancelableEventHandlerMonkey()
        { }

        void ICancelableEventHandler<TEvent5>.Handle(TEvent5 eventData)
        {
            if (AppliesTo(eventData))
                Handle(eventData);
        }

        /// <inheritdoc cref="ResoniteCancelableEventHandlerMonkey{TMonkey, TEvent}.AppliesTo"/>
        protected virtual bool AppliesTo(TEvent5 eventData) => Enabled;

        /// <inheritdoc cref="ResoniteCancelableEventHandlerMonkey{TMonkey, TEvent}.Handle"/>
        protected abstract void Handle(TEvent5 eventData);

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
    /// Specifically, to act as an <see cref="ICancelableEventHandler{TEvent}">event handler</see> for the cancelable <c>TEvent</c> generic parameter(s).
    /// </summary>
    /// <typeparam name="TMonkey">The type of the actual patcher.</typeparam>
    /// <typeparam name="TEvent1">The first <see cref="CancelableSyncEvent"/> type to handle.</typeparam>
    /// <typeparam name="TEvent2">The second <see cref="CancelableSyncEvent"/> type to handle.</typeparam>
    /// <typeparam name="TEvent3">The third <see cref="CancelableSyncEvent"/> type to handle.</typeparam>
    /// <typeparam name="TEvent4">The fourth <see cref="CancelableSyncEvent"/> type to handle.</typeparam>
    /// <typeparam name="TEvent5">The fifth <see cref="CancelableSyncEvent"/> type to handle.</typeparam>
    /// <typeparam name="TEvent6">The sixth <see cref="CancelableSyncEvent"/> type to handle.</typeparam>
    /// <inheritdoc cref="ResoniteMonkey{TMonkey}"/>
    public abstract class ResoniteCancelableEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6>
            : ResoniteCancelableEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>, ICancelableEventHandler<TEvent6>
        where TMonkey : ResoniteCancelableEventHandlerMonkey<TMonkey, TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6>, new()
        where TEvent1 : CancelableSyncEvent
        where TEvent2 : CancelableSyncEvent
        where TEvent3 : CancelableSyncEvent
        where TEvent4 : CancelableSyncEvent
        where TEvent5 : CancelableSyncEvent
        where TEvent6 : CancelableSyncEvent
    {
        /// <inheritdoc/>
        protected ResoniteCancelableEventHandlerMonkey()
        { }

        void ICancelableEventHandler<TEvent6>.Handle(TEvent6 eventData)
        {
            if (AppliesTo(eventData))
                Handle(eventData);
        }

        /// <inheritdoc cref="ResoniteCancelableEventHandlerMonkey{TMonkey, TEvent}.AppliesTo"/>
        protected virtual bool AppliesTo(TEvent6 eventData) => Enabled;

        /// <inheritdoc cref="ResoniteCancelableEventHandlerMonkey{TMonkey, TEvent}.Handle"/>
        protected abstract void Handle(TEvent6 eventData);

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