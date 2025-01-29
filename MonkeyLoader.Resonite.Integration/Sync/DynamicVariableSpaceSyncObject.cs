using Elements.Core;
using FrooxEngine;
using MonkeyLoader.Logging;
using MonkeyLoader.Sync;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLoader.Resonite.Sync
{
    public abstract class DynamicVariableSpaceSyncObject<TSyncObject> : MonkeySyncObject<TSyncObject, IMonkeySyncValue, DynamicVariableSpace>
        where TSyncObject : DynamicVariableSpaceSyncObject<TSyncObject>
    {
        public static RegisteredSyncObject<DynamicVariableSpace> RegisteredSyncObject { get; }

        /// <inheritdoc/>
        public override bool IsLinkValid => LinkObject!.FilterWorldElement() is not null;

        static DynamicVariableSpaceSyncObject()
        {
            RegisteredSyncObject = MonkeySyncRegistry.RegisterSyncObject(nameof(TestObject), typeof(TestObject), () => new TestObject(TestMonkey.Logger));
        }

        /// <inheritdoc/>
        protected override sealed bool EstablishLinkFor(string propertyName, IMonkeySyncValue syncValue, bool fromRemote)
        {
            // need syncValue.ValueType

            var varType = typeof(IWorldElement).IsAssignableFrom(syncValue.ValueType) ? typeof(DynamicReferenceVariable<>) : typeof(DynamicValueVariable<>);
            varType = varType.MakeGenericType(syncValue.ValueType);
            var dynVar = LinkObject.Slot.AttachComponent(varType);
            dynVar.TryGetField<string>(nameof(DynamicVariableBase<dummy>.VariableName)).Value = $"{LinkObject.SpaceName.Value}/{propertyName}";
            dynVar.TryGetField(nameof(DynamicValueVariable<dummy>.Value)).BoxedValue = syncValue.Value!;

            return true;
        }

        /// <inheritdoc/>
        protected override sealed bool EstablishLinkFor(string methodName, Action<TSyncObject> syncMethod, bool fromRemote)
        {
            var boolVar = LinkObject!.Slot.AttachComponent<DynamicValueVariable<bool>>();
            boolVar.VariableName.Value = $"{LinkObject.SpaceName.Value}/{methodName}";
            boolVar.Value.Changed += _ => syncMethod((TSyncObject)this);

            return true;
        }

        /// <inheritdoc/>
        protected override sealed bool EstablishLinkWith(DynamicVariableSpace linkObject, bool fromRemote)
        {
            linkObject.OnlyDirectBinding.Value = true;
            linkObject.SpaceName.Value = $"{DynamicVariableSpaceLink.SpaceNamePrefix}{RegisteredSyncObject.Name}";

            return base.EstablishLinkWith(linkObject, fromRemote);
        }

        /// <inheritdoc/>
        protected override sealed void OnDisposing() => base.OnDisposing();

        /// <inheritdoc/>
        protected override sealed bool TryRestoreLink() => base.TryRestoreLink();

        /// <inheritdoc/>
        protected override sealed bool TryRestoreLinkFor(string propertyName, IMonkeySyncValue syncValue) => throw new NotImplementedException();

        /// <inheritdoc/>
        protected override sealed bool TryRestoreLinkFor(string methodName, Action<TSyncObject> syncMethod) => throw new NotImplementedException();
    }
}