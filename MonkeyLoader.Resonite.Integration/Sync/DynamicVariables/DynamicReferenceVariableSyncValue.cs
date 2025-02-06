using FrooxEngine;
using System;

namespace MonkeyLoader.Resonite.Sync.DynamicVariables
{
    public sealed class DynamicReferenceVariableSyncValue<T> : DynamicVariableSyncValue<T, DynamicReferenceVariable<T>>, ILinkedDynamicReferenceVariableSyncValue<T>
        where T : class, IWorldElement
    {
        public override T Value
        {
            get => base.Value;
            set
            {
                if (!IsValidReference(value))
                    throw new InvalidOperationException($"Reference {value.GetReferenceLabel()} is not in world {SyncObject.LinkObject.World.Name}");

                base.Value = value;
            }
        }

        public DynamicReferenceVariableSyncValue(T value) : base(value)
        { }

        public bool IsValidReference(T value)
            => value is null || value.World == SyncObject.LinkObject.World;

        protected override void AddOnChangedHandler()
            => DynamicVariable.Reference.OnTargetChange += reference => DirectValue = reference.Target;
    }

    public interface ILinkedDynamicReferenceVariableSyncValue<T> : ILinkedDynamicVariableSyncValue<T>
            where T : class, IWorldElement
    {
        public new DynamicReferenceVariable<T> DynamicVariable { get; }
    }
}