﻿using FrooxEngine;
using MonkeyLoader.Sync;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MonkeyLoader.Resonite.Sync.DynamicVariables
{
    /// <summary>
    /// Implements an abstract base for the MonkeySync value implementations for
    /// <see cref="DynamicValueVariableSyncValue{T}">values</see>,
    /// <see cref="DynamicReferenceVariableSyncValue{T}">references</see>, and
    /// <see cref="DynamicTypeVariableSyncValue">Types</see>
    /// using <see cref="DynamicVariableBase{T}">dynamic variable</see> components.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="ILinkedMonkeySyncValue{T}.Value">Value</see>.</typeparam>
    /// <typeparam name="TVariable">The type of the <see cref="DynamicVariableBase{T}"/>-derived component used to sync this value.</typeparam>
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

        /// <remarks>
        /// Also sets the <see cref="DynamicVariableBase{T}.LocalValue">LocalValue</see>
        /// of the linked <see cref="DynamicVariable">DynamicVariable</see>.<br/>
        /// This means that the setter of this property must be called in a
        /// <see cref="World.RunSynchronously">synchronous</see> context.
        /// </remarks>
        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public string VariableName { get; private set; } = null!;

        /// <summary>
        /// Sets the underlying <see cref="MonkeySyncValue{TLink, T}.Value"/> without
        /// attempting to set the linked <see cref="DynamicVariable">DynamicVariable</see>'s.
        /// </summary>
        protected T DirectValue
        {
            set => base.Value = value;
        }

        /// <inheritdoc/>
        protected DynamicVariableSyncValue(T value) : base(value)
        { }

        /// <summary>
        /// Adds the <typeparamref name="TVariable"/>-specific OnChanged handler
        /// to the linked <see cref="DynamicVariable">DynamicVariable</see>'s value.
        /// </summary>
        protected abstract void AddOnChangedHandler();

        /// <remarks>
        /// Attaches a new <typeparamref name="TVariable"/> or finds an existing one
        /// matching the <see cref="VariableName">VariableName</see>.
        /// </remarks>
        /// <inheritdoc/>
        protected override bool EstablishLinkInternal(bool fromRemote)
        {
            if (!SyncObject.LinkObject.World.Types.IsSupported(typeof(TVariable)))
                throw new InvalidOperationException($"Type {typeof(TVariable).CompactDescription()} is not supported by world {SyncObject.LinkObject.World.Name}");

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

            AddOnChangedHandler();

            return true;
        }

        private bool MatchesVariable(TVariable variable)
            => variable.IsLinkedToSpace(SyncObject.LinkObject) && (variable.VariableName == VariableName || variable.VariableName == Name);
    }

    /// <summary>
    /// Defines the generic interface for linked <see cref="DynamicVariableSyncValue{T, TVariable}"/>s.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="ILinkedMonkeySyncValue{T}.Value">Value</see>.</typeparam>
    public interface ILinkedDynamicVariableSyncValue<T> : ILinkedDynamicVariableSyncValue, ILinkedMonkeySyncValue<DynamicVariableSpace, T>
    {
        /// <summary>
        /// Gets the <see cref="IDynamicVariable{T}"/> component syncing this value.
        /// </summary>
        public new IDynamicVariable<T> DynamicVariable { get; }
    }

    /// <summary>
    /// Defines the non-generic interface for <see cref="ILinkedDynamicVariableSyncValue{T}"/>s.
    /// </summary>
    public interface ILinkedDynamicVariableSyncValue : ILinkedMonkeySyncValue<DynamicVariableSpace>
    {
        /// <summary>
        /// Gets the <see cref="IDynamicVariable"/> component syncing this value.
        /// </summary>
        public IDynamicVariable DynamicVariable { get; }

        /// <summary>
        /// Gets the <see cref="IDynamicVariable.VariableName">name</see>
        /// of the linked <see cref="DynamicVariable">DynamicVariable</see>.
        /// </summary>
        public string VariableName { get; }
    }

    /// <summary>
    /// Defines the interface for not yet linked <see cref="DynamicVariableSyncValue{T, TVariable}"/>s.
    /// </summary>
    public interface IUnlinkedDynamicVariableSyncValue : ILinkedDynamicVariableSyncValue, IUnlinkedMonkeySyncValue<DynamicVariableSpace>
    { }
}