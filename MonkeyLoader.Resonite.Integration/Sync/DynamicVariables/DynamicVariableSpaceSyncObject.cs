using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Sync;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLoader.Resonite.Sync.DynamicVariables
{
    public abstract class DynamicVariableSpaceSyncObject<TSyncObject> : MonkeySyncObject<TSyncObject, IMonkeySyncValue, DynamicVariableSpace>
        where TSyncObject : DynamicVariableSpaceSyncObject<TSyncObject>
    {
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
        protected override sealed bool EstablishLinkFor(string propertyName, IMonkeySyncValue syncValue, bool fromRemote)
        {
            try
            {
                var isReference = typeof(IWorldElement).IsAssignableFrom(syncValue.ValueType);

                if (fromRemote)
                {
                    var bareMethod = AccessTools.DeclaredMethod(typeof(DynamicVariableSpaceLink),
                        isReference
                            ? nameof(DynamicVariableSpaceLink.CreateReferenceLink)
                            : nameof(DynamicVariableSpaceLink.CreateValueLink));

                    var genericMethod = bareMethod.MakeGenericMethod(syncValue.ValueType);

                    var variableType = isReference
                        ? typeof(DynamicReferenceVariable<>)
                        : typeof(DynamicValueVariable<>);

                    // This actually needs to consider the name
                    var variable = LinkObject.Slot.GetComponent(variableType.MakeGenericType(syncValue.ValueType));

                    if (variable is null)
                        return false;

                    genericMethod.Invoke(null, [variable, syncValue]);
                }
                else
                {
                    var bareMethod = AccessTools.DeclaredMethod(typeof(DynamicVariableSpaceLink),
                        isReference
                            ? nameof(DynamicVariableSpaceLink.CreateLinkedReferenceVariable)
                            : nameof(DynamicVariableSpaceLink.CreateLinkedValueVariable));

                    var genericMethod = bareMethod.MakeGenericMethod(syncValue.ValueType);

                    genericMethod.Invoke(null, [LinkObject.Slot, $"{VariableSpaceName}/{propertyName}", syncValue]);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc/>
        protected override sealed bool EstablishLinkFor(string methodName, Action<TSyncObject> syncMethod, bool fromRemote)
        {
            var boolVar = LinkObject!.Slot.AttachComponent<DynamicValueVariable<bool>>();
            boolVar.VariableName.Value = $"{VariableSpaceName}/{methodName}";
            boolVar.Value.Changed += _ => syncMethod((TSyncObject)this);

            return true;
        }

        /// <inheritdoc/>
        protected override sealed bool EstablishLinkWith(DynamicVariableSpace linkObject, bool fromRemote)
        {
            linkObject.OnlyDirectBinding.Value = true;

            linkObject.SpaceName.Value = VariableSpaceName;
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
        protected override sealed bool TryRestoreLinkFor(string propertyName, IMonkeySyncValue syncValue) => throw new NotImplementedException();

        /// <inheritdoc/>
        protected override sealed bool TryRestoreLinkFor(string methodName, Action<TSyncObject> syncMethod) => throw new NotImplementedException();
    }
}