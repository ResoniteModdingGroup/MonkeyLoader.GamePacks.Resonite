using FrooxEngine;
using FrooxEngine.ProtoFlux;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLoader.Resonite.ProtoFlux.Events
{
    public class DynamicImpulseInfo
    {
        public bool ExcludeDisabled { get; }

        public Slot Hierarchy { get; }

        public FrooxEngineContext? SourceContext { get; }

        public string Tag { get; }

        public DynamicImpulseInfo(Slot hierarchy, string tag, bool excludeDisabled, FrooxEngineContext? sourceContext)
        {
            Hierarchy = hierarchy;
            Tag = tag;
            ExcludeDisabled = excludeDisabled;
            SourceContext = sourceContext;
        }

        /// <param name="asynchronous">Whether the dynamic impulse is asynchronous or not.</param>
        /// <inheritdoc cref="ToString()"/>
        public string ToString(bool asynchronous)
            => $"{(asynchronous ? "Async" : "Sync")} {this}";

        /// <inheritdoc/>
        public override string ToString()
            => $"Dynamic Impulse [{Tag}] {(ExcludeDisabled ? "excluding" : "including")} disabled slots, targetting: {Hierarchy.ParentHierarchyToString()}";
    }

    public abstract class DynamicImpulseWithArgumentInfo : DynamicImpulseInfo
    {
        public object Argument => ArgumentCore;

        protected abstract object ArgumentCore { get; }

        protected DynamicImpulseWithArgumentInfo(Slot hierarchy, string tag, bool excludeDisabled, FrooxEngineContext? sourceContext)
            : base(hierarchy, tag, excludeDisabled, sourceContext)
        { }
    }

    public sealed class DynamicImpulseWithArgumentInfo<T> : DynamicImpulseWithArgumentInfo
    {
        public new T Argument { get; }

        protected override object ArgumentCore => Argument;

        public DynamicImpulseWithArgumentInfo(Slot hierarchy, string tag, bool excludeDisabled, T argument, FrooxEngineContext? sourceContext)
            : base(hierarchy, tag, excludeDisabled, sourceContext)
        {
            Argument = argument;
        }

        /// <inheritdoc/>
        public override string ToString()
            => $"Dynamic Impulse [{Tag}] {(ExcludeDisabled ? "excluding" : "including")} disabled slots with argument [{Argument}], targetting: {Hierarchy.ParentHierarchyToString()}";
    }
}