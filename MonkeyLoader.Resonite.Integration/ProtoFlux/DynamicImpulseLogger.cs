using MonkeyLoader.Events;
using MonkeyLoader.Resonite.ProtoFlux.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.ProtoFlux
{
    internal sealed class DynamicImpulseLogger : ResoniteMonkey<DynamicImpulseLogger>,
        IEventHandler<SyncDynamicImpulseEvent>, IEventHandler<SyncDynamicImpulseWithArgumentEvent>,
        IAsyncEventHandler<AsyncDynamicImpulseEvent>, IAsyncEventHandler<AsyncDynamicImpulseWithArgumentEvent>
    {
        public int Priority => HarmonyLib.Priority.Normal;

        public Task Handle(AsyncDynamicImpulseWithArgumentEvent eventData)
        {
            LogImpulse(eventData.Info, true);
            return Task.CompletedTask;
        }

        public Task Handle(AsyncDynamicImpulseEvent eventData)
        {
            LogImpulse(eventData.Info, true);
            return Task.CompletedTask;
        }

        public void Handle(SyncDynamicImpulseWithArgumentEvent eventData)
            => LogImpulse(eventData.Info, false);

        public void Handle(SyncDynamicImpulseEvent eventData)
            => LogImpulse(eventData.Info, false);

        protected override bool OnEngineReady()
        {
            Mod.RegisterEventHandler<SyncDynamicImpulseEvent>(this);
            Mod.RegisterEventHandler<SyncDynamicImpulseWithArgumentEvent>(this);
            Mod.RegisterEventHandler<AsyncDynamicImpulseEvent>(this);
            Mod.RegisterEventHandler<AsyncDynamicImpulseWithArgumentEvent>(this);

            return base.OnEngineReady();
        }

        protected override bool OnShutdown(bool applicationExiting)
        {
            if (!applicationExiting)
            {
                Mod.UnregisterEventHandler<SyncDynamicImpulseEvent>(this);
                Mod.UnregisterEventHandler<SyncDynamicImpulseWithArgumentEvent>(this);
                Mod.UnregisterEventHandler<AsyncDynamicImpulseEvent>(this);
                Mod.UnregisterEventHandler<AsyncDynamicImpulseWithArgumentEvent>(this);
            }

            return base.OnShutdown(applicationExiting);
        }

        private static void LogImpulse(DynamicImpulseInfo dynamicImpulseInfo, bool asynchronous)
            => Logger.Info(() => dynamicImpulseInfo.ToString(asynchronous));
    }
}