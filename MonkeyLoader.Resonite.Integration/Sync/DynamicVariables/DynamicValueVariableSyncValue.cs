using Elements.Core;
using FrooxEngine;
using System;

namespace MonkeyLoader.Resonite.Sync.DynamicVariables
{
    public sealed class DynamicValueVariableSyncValue<T> : DynamicVariableSyncValue<T, DynamicValueVariable<T>>, ILinkedDynamicValueVariableSyncValue<T>
    {
        public DynamicValueVariableSyncValue(T value) : base(value)
        {
            if (!Coder<T>.IsEnginePrimitive)
                throw new InvalidOperationException($"Type {typeof(T).CompactDescription()} is not an Engine Primitive!");
        }

        protected override void AddOnChangedHandler()
            => DynamicVariable.Value.OnValueChange += field => DirectValue = field.Value;
    }

    public interface ILinkedDynamicValueVariableSyncValue<T> : ILinkedDynamicVariableSyncValue<T>
    {
        public new DynamicValueVariable<T> DynamicVariable { get; }
    }
}