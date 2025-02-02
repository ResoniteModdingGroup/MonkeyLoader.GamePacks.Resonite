using FrooxEngine;
using MonkeyLoader.Resonite.Configuration;
using System;

namespace MonkeyLoader.Resonite.Sync.DynamicVariables
{
    public sealed class DynamicTypeVariableSyncValue : DynamicVariableSyncValue<Type, DynamicTypeVariable>, IDynamicTypeVariableSyncValue
    {
        public override Type Value
        {
            get => base.Value;
            set
            {
                if (!IsSupportedType(value))
                    throw new InvalidOperationException($"Type {value.CompactDescription()} is not supported by world {SyncObject.LinkObject.World.Name}");

                base.Value = value;
            }
        }

        public DynamicTypeVariableSyncValue(Type value) : base(value)
        { }

        public bool IsSupportedType(Type type)
            => type is null || SyncObject.LinkObject.World.Types.IsSupported(type);

        protected override void AddOnChangeHandler()
            => DynamicVariable.Value.OnValueChange += field => DirectValue = field.Value;
    }

    public interface IDynamicTypeVariableSyncValue : ILinkedDynamicVariableSyncValue<Type>
    {
        public new DynamicTypeVariable DynamicVariable { get; }
    }
}