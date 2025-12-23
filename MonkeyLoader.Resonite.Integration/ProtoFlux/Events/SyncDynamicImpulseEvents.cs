using MonkeyLoader.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLoader.Resonite.ProtoFlux.Events
{
    public sealed class SyncDynamicImpulseEvent : SyncEvent
    {
        public DynamicImpulseInfo Info { get; }

        internal SyncDynamicImpulseEvent(DynamicImpulseInfo info)
        {
            Info = info;
        }
    }

    [DispatchableBaseEvent, SubscribableBaseEvent]
    public abstract class SyncDynamicImpulseWithArgumentEvent : SyncEvent
    {
        public DynamicImpulseWithArgumentInfo Info => InfoCore;

        protected abstract DynamicImpulseWithArgumentInfo InfoCore { get; }

        internal SyncDynamicImpulseWithArgumentEvent()
        { }
    }

    public sealed class SyncDynamicImpulseWithArgumentEvent<T> : SyncDynamicImpulseWithArgumentEvent
    {
        public new DynamicImpulseWithArgumentInfo<T> Info { get; }

        protected override DynamicImpulseWithArgumentInfo InfoCore => Info;

        internal SyncDynamicImpulseWithArgumentEvent(DynamicImpulseWithArgumentInfo<T> info)
        {
            Info = info;
        }
    }
}