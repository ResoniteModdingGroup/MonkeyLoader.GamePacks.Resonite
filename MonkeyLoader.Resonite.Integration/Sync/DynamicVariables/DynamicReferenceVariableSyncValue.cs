using FrooxEngine;
using System;
using System.Diagnostics.CodeAnalysis;

namespace MonkeyLoader.Resonite.Sync.DynamicVariables
{
    /// <summary>
    /// Implements a MonkeySync value that uses a <see cref="DynamicReferenceVariable{T}"/>
    /// to sync <typeparamref name="T"/>s valid in the respective <see cref="World"/>.
    /// </summary>
    /// <inheritdoc/>
    public sealed class DynamicReferenceVariableSyncValue<T> : DynamicVariableSyncValue<T, DynamicReferenceVariable<T>>, ILinkedDynamicReferenceVariableSyncValue<T>
        where T : class?, IWorldElement
    {
        /// <value>
        /// A reference to a <typeparamref name="T"/> <see cref="IsValidReference">valid</see>
        /// in the <see cref="World"/> this reference value is being synced through.
        /// </value>
        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public DynamicReferenceVariableSyncValue(T value) : base(value)
        { }

        /// <summary>
        /// Determines whether the given reference <paramref name="value"/> is
        /// valid in the <see cref="World"/> this value is being synced through.
        /// </summary>
        /// <param name="value">The reference value to test.</param>
        /// <returns><c>true</c> if <paramref name="value"/> is <c>null</c> or in the same <see cref="World"/>; otherwise, <c>false</c>.</returns>
        public bool IsValidReference([NotNullWhen(false)] T? value)
            => value is null || value.World == SyncObject.LinkObject.World;

        /// <inheritdoc/>
        protected override void AddOnChangedHandler()
            => DynamicVariable.Reference.OnTargetChange += reference => DirectValue = reference.Target;
    }

    /// <summary>
    /// Defines the generic interface for linked MonkeySync values that use a <see cref="DynamicReferenceVariable{T}"/>
    /// to sync <typeparamref name="T"/>s valid in the respective <see cref="World"/>.
    /// </summary>
    /// <inheritdoc/>
    public interface ILinkedDynamicReferenceVariableSyncValue<T> : ILinkedDynamicVariableSyncValue<T>
            where T : class, IWorldElement
    {
        /// <inheritdoc/>
        public new DynamicReferenceVariable<T> DynamicVariable { get; }
    }
}