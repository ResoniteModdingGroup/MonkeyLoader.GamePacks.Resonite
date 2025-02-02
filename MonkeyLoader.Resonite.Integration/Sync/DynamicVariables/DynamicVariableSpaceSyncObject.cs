using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Sync;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLoader.Resonite.Sync.DynamicVariables
{
    public abstract class DynamicVariableSpaceSyncObject<TSyncObject> : MonkeySyncObject<TSyncObject, IUnlinkedDynamicVariableSyncValue, DynamicVariableSpace>
        where TSyncObject : DynamicVariableSpaceSyncObject<TSyncObject>
    {
        private readonly Dictionary<string, DynamicValueVariableSyncValue<bool>> _syncMethodTogglesByName = new(StringComparer.Ordinal);

        /// <summary>
        /// Gets the <see cref="MonkeySyncRegistry"/> information for this MonkeySync object.
        /// </summary>
        public static MonkeySyncObjectRegistration<DynamicVariableSpace> Registration { get; }

        /// <summary>
        /// Gets the <see cref="DynamicVariableSpace.SpaceName">SpaceName</see> that the
        /// <see cref="MonkeySyncObject{TSyncObject, TSyncValue, TLink}.LinkObject">LinkObject</see>
        /// for this MonkeySync object must have.
        /// </summary>
        public static string VariableSpaceName { get; }

        /// <inheritdoc/>
        public override bool IsLinkValid => LinkObject!.FilterWorldElement()?.SpaceName.Value == VariableSpaceName;

        static DynamicVariableSpaceSyncObject()
        {
            Registration = MonkeySyncRegistry.RegisterSyncObject(nameof(TestObject), typeof(TestObject), () => new TestObject(TestMonkey.Logger));
            VariableSpaceName = $"{DynamicVariableSpaceLink.SpaceNamePrefix}{Registration.Name}";
        }

        /// <inheritdoc/>
        protected override sealed bool EstablishLinkFor(string methodName, Action<TSyncObject> syncMethod, bool fromRemote)
        {
            var syncValue = new DynamicValueVariableSyncValue<bool>(false);

            if (!syncValue.EstablishLinkFor(this, methodName, fromRemote))
                return false;

            syncValue.Changed += (_, _) => syncMethod((TSyncObject)this);

            return true;
        }

        /// <inheritdoc/>
        protected override sealed bool EstablishLinkWith(DynamicVariableSpace linkObject, bool fromRemote)
        {
            if (fromRemote)
            {
                if (linkObject.SpaceName != VariableSpaceName)
                    return false;
            }
            else
            {
                linkObject.OnlyDirectBinding.Value = true;
                linkObject.SpaceName.Value = VariableSpaceName;
            }

            linkObject.SpaceName.OnValueChange += field => LinkObject.RunSynchronously(OnLinkInvalidated);

            return base.EstablishLinkWith(linkObject, fromRemote);
        }

        /// <inheritdoc/>
        protected override sealed void OnDisposing() => base.OnDisposing();

        /// <inheritdoc/>
        protected override sealed bool TryRestoreLink()
        {
            if (LinkObject?.IsDestroyed ?? true)
                return false;

            LinkObject.SpaceName.Value = VariableSpaceName;

            return base.TryRestoreLink();
        }

        /// <inheritdoc/>
        protected override sealed bool TryRestoreLinkFor(string propertyName, IUnlinkedDynamicVariableSyncValue syncValue) => throw new NotImplementedException();

        /// <inheritdoc/>
        protected override sealed bool TryRestoreLinkFor(string methodName, Action<TSyncObject> syncMethod) => throw new NotImplementedException();
    }
}