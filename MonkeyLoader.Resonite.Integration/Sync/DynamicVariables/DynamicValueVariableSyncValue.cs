using Elements.Core;
using FrooxEngine;
using System;

namespace MonkeyLoader.Resonite.Sync.DynamicVariables
{
    /// <summary>
    /// Implements a MonkeySync value that uses a <see cref="DynamicValueVariable{T}"/>
    /// to sync Resonite <see cref="Coder{T}.IsEnginePrimitive">engine primitives</see>.
    /// </summary>
    /// <inheritdoc/>
    public sealed class DynamicValueVariableSyncValue<T> : DynamicVariableSyncValue<DynamicValueVariableSyncValue<T>, T, DynamicValueVariable<T>>,
        ILinkedDynamicValueVariableSyncValue<T>
    {
        /// <inheritdoc/>
        public DynamicValueVariableSyncValue(T value) : base(value)
        {
            if (!Coder<T>.IsEnginePrimitive)
                throw new InvalidOperationException($"Type {typeof(T).CompactDescription()} is not an Engine Primitive!");
        }

        /// <inheritdoc/>
        protected override void AddOnChangedHandler()
            => DynamicVariable.Value.OnValueChange += field => DirectValue = field.Value;
    }

    /// <summary>
    /// Defines the generic interface for linked MonkeySync values that use a <see cref="DynamicValueVariable{T}"/>
    /// to sync Resonite <see cref="Coder{T}.IsEnginePrimitive">engine primitives</see>.
    /// </summary>
    /// <inheritdoc/>
    public interface ILinkedDynamicValueVariableSyncValue<T> : ILinkedDynamicVariableSyncValue<ILinkedDynamicVariableSpaceSyncObject, T>
    {
        /// <inheritdoc/>
        public new DynamicValueVariable<T> DynamicVariable { get; }
    }
}