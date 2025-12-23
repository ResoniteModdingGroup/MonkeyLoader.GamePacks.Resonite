using MonkeyLoader.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLoader.Resonite.ProtoFlux.Events
{
    public sealed class AsyncDynamicImpulseEvent : AsyncEvent
    {
        public DynamicImpulseInfo Info { get; }

        internal AsyncDynamicImpulseEvent(DynamicImpulseInfo info)
        {
            Info = info;
        }
    }

    [DispatchableBaseEvent, SubscribableBaseEvent]
    public abstract class AsyncDynamicImpulseWithArgumentEvent : AsyncEvent
    {
        public DynamicImpulseWithArgumentInfo Info => InfoCore;

        protected abstract DynamicImpulseWithArgumentInfo InfoCore { get; }

        internal AsyncDynamicImpulseWithArgumentEvent()
        { }
    }

    public sealed class AsyncDynamicImpulseWithArgumentEvent<T> : AsyncDynamicImpulseWithArgumentEvent
    {
        public new DynamicImpulseWithArgumentInfo<T> Info { get; }

        protected override DynamicImpulseWithArgumentInfo InfoCore => Info;

        internal AsyncDynamicImpulseWithArgumentEvent(DynamicImpulseWithArgumentInfo<T> info)
        {
            Info = info;
        }
    }
}