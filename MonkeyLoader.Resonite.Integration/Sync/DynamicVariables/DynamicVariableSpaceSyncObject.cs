using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkeyLoader.Resonite.Sync.DynamicVariables
{
    /// <summary>
    /// Implements the abstract base for MonkeySync objects that use the variables of a
    /// <see cref="DynamicVariableSpace"/> to synchronize their values with others.
    /// </summary>
    /// <inheritdoc/>
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
        protected override sealed bool EstablishLink(bool fromRemote)
        {
            if (fromRemote)
            {
                if (LinkObject.SpaceName != VariableSpaceName)
                    return false;
            }

            LinkObject.OnlyDirectBinding.Value = true;
            LinkObject.SpaceName.Value = VariableSpaceName;

            LinkObject.SpaceName.OnValueChange += field => LinkObject.RunSynchronously(OnLinkInvalidated);
            LinkObject.Destroyed += _ => OnLinkInvalidated();

            return base.EstablishLink(fromRemote);
        }

        /// <inheritdoc/>
        protected override sealed bool EstablishLinkFor(Action syncMethod, string methodName, bool fromRemote)
        {
            if (_syncMethodTogglesByName.ContainsKey(methodName))
            {
                DynamicVariableSpaceLink.Logger.Warn(() => $"Attempted to establish link for method [{methodName}] that already has a toggle! Skipping.");
                return true;
            }

            var syncValue = new DynamicValueVariableSyncValue<bool>(false);

            if (!syncValue.EstablishLinkFor(this, methodName, fromRemote))
                return false;

            syncValue.Changed += (_, _) => LinkObject.RunSynchronously(syncMethod);

            syncValues.Add(syncValue);
            _syncMethodTogglesByName.Add(methodName, syncValue);

            return true;
        }

        /// <remarks>
        /// Synchronously destroys all <see cref="IDynamicVariable">dynamic variable</see>
        /// <see cref="Component"/>s linked to the <see cref="DynamicVariableSpace"/>
        /// <see cref="MonkeySyncObject{TSyncObject, TSyncValue, TLink}.LinkObject">LinkObject</see>
        /// and destroys them and the space itself.<br/>
        /// If no components remain on the <see cref="Slot"/>, it is destroyed as well.
        /// </remarks>
        /// <inheritdoc/>
        protected override sealed void OnDisposing()
        {
            LinkObject.RunSynchronously(() =>
            {
                var slot = LinkObject.Slot;

                foreach (var variable in LinkObject.GetLinkedVariables().OfType<Component>())
                    variable.Destroy();

                LinkObject.Destroy();

                if (slot.ComponentCount == 0)
                    slot.Destroy();
            }, true);
        }

        /// <inheritdoc/>
        protected override sealed bool TryRestoreLink()
        {
            if (LinkObject.FilterWorldElement() is null)
                return false;

            LinkObject.OnlyDirectBinding.Value = true;
            LinkObject.SpaceName.Value = VariableSpaceName;

            return base.TryRestoreLink();
        }

        /// <inheritdoc/>
        protected override sealed bool TryRestoreLinkFor(IUnlinkedDynamicVariableSyncValue syncValue)
            => syncValue.TryRestoreLink();

        /// <inheritdoc/>
        protected override sealed bool TryRestoreLinkFor(Action syncMethod, string methodName)
            => true; // Covered by TryRestoreLinkFor(syncValue)
    }
}