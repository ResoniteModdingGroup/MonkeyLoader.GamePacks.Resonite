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

        protected override bool EstablishLinkFor(string propertyName, IMonkeySyncValue syncValue, bool fromRemote)
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
        protected override bool EstablishLinkFor(string methodName, Action<TSyncObject> syncMethod, bool fromRemote)
        {
            var boolVar = LinkObject!.Slot.AttachComponent<DynamicValueVariable<bool>>();
            boolVar.VariableName.Value = $"{LinkObject.SpaceName.Value}/{methodName}";
            boolVar.Value.Changed += _ => syncMethod((TSyncObject)this);

            return true;
        }

        /// <inheritdoc/>
        protected override bool EstablishLinkWith(DynamicVariableSpace linkObject, bool fromRemote)
        {
            linkObject.OnlyDirectBinding.Value = true;
            linkObject.SpaceName.Value = $"{DynamicVariableSpaceSync.SpaceNamePrefix}::{typeof(TSyncObject).Name}";

            return base.EstablishLinkWith(linkObject, fromRemote);
        }

        /// <inheritdoc/>
        protected override void OnDisposing() => base.OnDisposing();

        /// <inheritdoc/>
        protected override bool TryRestoreLink() => base.TryRestoreLink();

        /// <inheritdoc/>
        protected override bool TryRestoreLinkFor(string propertyName, IMonkeySyncValue syncValue) => throw new NotImplementedException();

        /// <inheritdoc/>
        protected override bool TryRestoreLinkFor(string methodName, Action<TSyncObject> syncMethod) => throw new NotImplementedException();
    }
}