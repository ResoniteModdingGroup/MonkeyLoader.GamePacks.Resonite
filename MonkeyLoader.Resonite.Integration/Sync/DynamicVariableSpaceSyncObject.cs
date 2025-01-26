using Elements.Core;
using FrooxEngine;
using MonkeyLoader.Sync;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLoader.Resonite.Sync
{
    public abstract class DynamicVariableSpaceSyncObject<TSyncObject> : MonkeySyncObject<TSyncObject, IMonkeySyncValue, DynamicVariableSpace>
        where TSyncObject : DynamicVariableSpaceSyncObject<TSyncObject>
    {
        public override bool IsLinkValid => LinkObject!.FilterWorldElement() is not null;

        protected override bool EstablishLinkFor(string propertyName, IMonkeySyncValue syncValue)
        {
            // need syncValue.ValueType

            var varType = typeof(IWorldElement).IsAssignableFrom(syncValue.Value.GetType()) ? typeof(DynamicReferenceVariable<>) : typeof(DynamicValueVariable<>);
            varType = varType.MakeGenericType(syncValue.Value.GetType());
            var dynVar = LinkObject.Slot.AttachComponent(varType);
            dynVar.TryGetField<string>(nameof(DynamicVariableBase<dummy>.VariableName)).Value = $"{LinkObject.SpaceName.Value}/{propertyName}";
            dynVar.TryGetField(nameof(DynamicValueVariable<dummy>.Value)).BoxedValue = syncValue.Value;

            return true;
        }

        /// <inheritdoc/>
        protected override bool EstablishLinkFor(string methodName, Action<TSyncObject> syncMethod)
        {
            var boolVar = LinkObject!.Slot.AttachComponent<DynamicValueVariable<bool>>();
            boolVar.VariableName.Value = $"{LinkObject.SpaceName.Value}/{methodName}";
            boolVar.Value.Changed += _ => syncMethod((TSyncObject)this);

            return true;
        }
    }
}