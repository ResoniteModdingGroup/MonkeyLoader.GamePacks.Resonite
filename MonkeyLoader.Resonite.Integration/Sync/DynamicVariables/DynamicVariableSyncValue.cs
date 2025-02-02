using FrooxEngine;
using MonkeyLoader.Sync;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MonkeyLoader.Resonite.Sync.DynamicVariables
{
    public abstract class DynamicVariableSyncValue<T, TVariable> : MonkeySyncValue<DynamicVariableSpace, T>,
            IUnlinkedDynamicVariableSyncValue, ILinkedDynamicVariableSyncValue<T>
        where TVariable : DynamicVariableBase<T>, new()
    {
        /// <summary>
        /// Gets the <typeparamref name="TVariable"/> component syncing this value.
        /// </summary>
        public TVariable DynamicVariable { get; protected set; } = null!;

        IDynamicVariable ILinkedDynamicVariableSyncValue.DynamicVariable => DynamicVariable;

        IDynamicVariable<T> ILinkedDynamicVariableSyncValue<T>.DynamicVariable => DynamicVariable;

        public override T Value
        {
            get => base.Value;
            set
            {
                if (DynamicVariable is not null)
                    DynamicVariable.LocalValue = value;

                base.Value = value;
            }
        }

        public string VariableName { get; private set; } = null!;

        protected T DirectValue
        {
            set => base.Value = value;
        }

        /// <inheritdoc/>
        protected DynamicVariableSyncValue(T value) : base(value)
        { }

        protected abstract void AddOnChangeHandler();

        protected override bool EstablishLinkInternal(bool fromRemote)
        {
            VariableName = $"{SyncObject.LinkObject.SpaceName.Value}/{Name}";

            if (fromRemote)
            {
                DynamicVariable = SyncObject.LinkObject.Slot.GetComponent<TVariable>(MatchesVariable);

                DirectValue = DynamicVariable.LocalValue;
            }
            else
            {
                DynamicVariable = SyncObject.LinkObject.Slot.AttachComponent<TVariable>();
                DynamicVariable.VariableName.Value = VariableName;
                DynamicVariable.LocalValue = Value;
            }

            if (DynamicVariable is null)
                return false;

            AddOnChangeHandler();

            return true;
        }

        private bool MatchesVariable(TVariable variable)
            => variable.IsLinkedToSpace(SyncObject.LinkObject) && (variable.VariableName == VariableName || variable.VariableName == Name);
    }

    public interface ILinkedDynamicVariableSyncValue<T> : ILinkedDynamicVariableSyncValue, ILinkedMonkeySyncValue<DynamicVariableSpace, T>
    {
        public new IDynamicVariable<T> DynamicVariable { get; }
    }

    public interface ILinkedDynamicVariableSyncValue : ILinkedMonkeySyncValue<DynamicVariableSpace>
    {
        public IDynamicVariable DynamicVariable { get; }
        public string VariableName { get; }
    }

    public interface IUnlinkedDynamicVariableSyncValue : ILinkedDynamicVariableSyncValue, IUnlinkedMonkeySyncValue<DynamicVariableSpace>
    { }
}