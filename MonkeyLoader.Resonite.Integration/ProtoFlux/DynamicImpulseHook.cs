using FrooxEngine;
using FrooxEngine.ProtoFlux;
using HarmonyLib;
using MonkeyLoader.Events;
using MonkeyLoader.Resonite.ProtoFlux.Events;
using ProtoFlux.Runtimes.Execution.Nodes.Actions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.ProtoFlux
{
    /// <summary>
    /// Handles generating events for locally triggered dynamic impulses.
    /// </summary>
    [HarmonyPatchCategory(nameof(DynamicImpulseHook))]
    [HarmonyPatch(typeof(ProtoFluxHelper), nameof(ProtoFluxHelper.DynamicImpulseHandler), MethodType.Setter)]
    public sealed class DynamicImpulseHook : ResoniteAsyncEventSourceMonkey<DynamicImpulseHook, AsyncDynamicImpulseEvent, AsyncDynamicImpulseWithArgumentEvent>,
        IEventSource<SyncDynamicImpulseEvent>, IEventSource<SyncDynamicImpulseWithArgumentEvent>
    {
        private static EventDispatching<SyncDynamicImpulseEvent>? _syncDynamicImpulseEventDispatching;
        private static EventDispatching<SyncDynamicImpulseWithArgumentEvent>? _syncDynamicImpulseWithArgumentEvent;

        /// <summary>
        /// Gets the the <see cref="IDynamicImpulseHandler"/> that was originally
        /// <see cref="ProtoFluxHelper.RegisterDynamicImpulseHandler">registered</see>
        /// with the <see cref="ProtoFluxHelper"/>.
        /// </summary>
        /// <remarks>
        /// Use this if you must trigger dynamic impulses without creating events.
        /// </remarks>
        public static IDynamicImpulseHandler ImpulseHandler { get; private set; } = null!;

        /// <inheritdoc/>
        protected override void OnDisabled()
        {
            if (ImpulseHandler is not null)
                ProtoFluxHelper.DynamicImpulseHandler = ImpulseHandler;
        }

        /// <inheritdoc/>
        protected override void OnEnabled()
        {
            if (ProtoFluxHelper.DynamicImpulseHandler != DynamicImpulseEventSourceHandler.Instance)
                ImpulseHandler = ProtoFluxHelper.DynamicImpulseHandler;

            ProtoFluxHelper.DynamicImpulseHandler = DynamicImpulseEventSourceHandler.Instance;
        }

        /// <inheritdoc/>
        protected override bool OnEngineReady()
        {
            if (Enabled)
                OnEnabled();

            _ = DynamicImpulseHelperWrapper.Instance;
            Mod.RegisterEventSource<SyncDynamicImpulseEvent>(this);
            Mod.RegisterEventSource<SyncDynamicImpulseWithArgumentEvent>(this);

            return base.OnEngineReady();
        }

        /// <inheritdoc/>
        protected override bool OnShutdown(bool applicationExiting)
        {
            if (!applicationExiting)
            {
                OnDisabled();

                Mod.UnregisterEventSource<SyncDynamicImpulseEvent>(this);
                Mod.UnregisterEventSource<SyncDynamicImpulseWithArgumentEvent>(this);
            }

            return base.OnShutdown(applicationExiting);
        }

        private static void Prefix(ref IDynamicImpulseHandler value)
        {
            if (!Enabled || value == DynamicImpulseEventSourceHandler.Instance)
                return;

            ImpulseHandler = value;
            value = DynamicImpulseEventSourceHandler.Instance;
        }

        event EventDispatching<SyncDynamicImpulseWithArgumentEvent>? IEventSource<SyncDynamicImpulseWithArgumentEvent>.Dispatching
        {
            add => _syncDynamicImpulseWithArgumentEvent += value;
            remove => _syncDynamicImpulseWithArgumentEvent -= value;
        }

        event EventDispatching<SyncDynamicImpulseEvent>? IEventSource<SyncDynamicImpulseEvent>.Dispatching
        {
            add => _syncDynamicImpulseEventDispatching += value;
            remove => _syncDynamicImpulseEventDispatching -= value;
        }

        private sealed class DynamicImpulseEventSourceHandler : IDynamicImpulseHandler
        {
            public static DynamicImpulseEventSourceHandler Instance { get; } = new();

            private DynamicImpulseEventSourceHandler()
            { }

            public async Task<int> TriggerAsyncDynamicImpulse(Slot hierarchy, string tag, bool excludeDisabled, FrooxEngineContext sourceContext = null!)
            {
                var eventData = new AsyncDynamicImpulseEvent(new(hierarchy, tag, excludeDisabled, sourceContext));

                await DispatchAsync(eventData);

                return await ImpulseHandler.TriggerAsyncDynamicImpulse(hierarchy, tag, excludeDisabled, sourceContext);
            }

            public async Task<int> TriggerAsyncDynamicImpulseWithArgument<T>(Slot hierarchy, string tag, bool excludeDisabled, T argument, FrooxEngineContext sourceContext = null!)
            {
                var eventData = new AsyncDynamicImpulseWithArgumentEvent<T>(new(hierarchy, tag, excludeDisabled, argument, sourceContext));

                await DispatchAsync(eventData);

                return await ImpulseHandler.TriggerAsyncDynamicImpulseWithArgument(hierarchy, tag, excludeDisabled, argument, sourceContext);
            }

            public int TriggerDynamicImpulse(Slot hierarchy, string tag, bool excludeDisabled, FrooxEngineContext sourceContext = null!)
            {
                var eventData = new SyncDynamicImpulseEvent(new(hierarchy, tag, excludeDisabled, sourceContext));

                _syncDynamicImpulseEventDispatching?.Invoke(eventData);

                return ImpulseHandler.TriggerDynamicImpulse(hierarchy, tag, excludeDisabled, sourceContext);
            }

            public int TriggerDynamicImpulseWithArgument<T>(Slot hierarchy, string tag, bool excludeDisabled, T argument, FrooxEngineContext sourceContext = null!)
            {
                var eventData = new SyncDynamicImpulseWithArgumentEvent<T>(new(hierarchy, tag, excludeDisabled, argument, sourceContext));

                _syncDynamicImpulseWithArgumentEvent?.Invoke(eventData);

                return ImpulseHandler.TriggerDynamicImpulseWithArgument(hierarchy, tag, excludeDisabled, argument, sourceContext);
            }
        }

        private sealed class DynamicImpulseHelperWrapper : DynamicImpulseHelper, IDynamicImpulseHandler
        {
            public static DynamicImpulseHelperWrapper Instance = new(Singleton);
            private readonly DynamicImpulseHelper _helper;

            static DynamicImpulseHelperWrapper()
            {
                Traverse.Create<DynamicImpulseHelper>()
                    .Field<DynamicImpulseHelper>(nameof(Singleton))
                    .Value = Instance;
            }

            private DynamicImpulseHelperWrapper(DynamicImpulseHelper helper)
            {
                _helper = helper;
            }

            async Task<int> IDynamicImpulseHandler.TriggerAsyncDynamicImpulse(Slot hierarchy, string tag, bool excludeDisabled, FrooxEngineContext sourceContext)
            {
                var eventData = new AsyncDynamicImpulseEvent(new(hierarchy, tag, excludeDisabled, sourceContext));

                await DispatchAsync(eventData);

                return await _helper.TriggerAsyncDynamicImpulse(hierarchy, tag, excludeDisabled, sourceContext);
            }

            async Task<int> IDynamicImpulseHandler.TriggerAsyncDynamicImpulseWithArgument<T>(Slot hierarchy, string tag, bool excludeDisabled, T argument, FrooxEngineContext sourceContext)
            {
                var eventData = new AsyncDynamicImpulseWithArgumentEvent<T>(new(hierarchy, tag, excludeDisabled, argument, sourceContext));

                await DispatchAsync(eventData);

                return await _helper.TriggerAsyncDynamicImpulseWithArgument(hierarchy, tag, excludeDisabled, argument, sourceContext);
            }

            int IDynamicImpulseHandler.TriggerDynamicImpulse(Slot hierarchy, string tag, bool excludeDisabled, FrooxEngineContext sourceContext)
            {
                var eventData = new SyncDynamicImpulseEvent(new(hierarchy, tag, excludeDisabled, sourceContext));

                _syncDynamicImpulseEventDispatching?.Invoke(eventData);

                return _helper.TriggerDynamicImpulse(hierarchy, tag, excludeDisabled, sourceContext);
            }

            int IDynamicImpulseHandler.TriggerDynamicImpulseWithArgument<T>(Slot hierarchy, string tag, bool excludeDisabled, T argument, FrooxEngineContext sourceContext)
            {
                var eventData = new SyncDynamicImpulseWithArgumentEvent<T>(new(hierarchy, tag, excludeDisabled, argument, sourceContext));

                _syncDynamicImpulseWithArgumentEvent?.Invoke(eventData);

                return _helper.TriggerDynamicImpulseWithArgument(hierarchy, tag, excludeDisabled, argument, sourceContext);
            }
        }
    }
}