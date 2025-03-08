using FrooxEngine;
using MonkeyLoader.Sync;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text;

namespace MonkeyLoader.Resonite.Sync.DynamicVariables
{
    /// <summary>
    /// Implements a MonkeySync method that uses a <see cref="DynamicReferenceVariable{T}"/>
    /// to sync the <see cref="User"/> that triggers the <see cref="Function">Function</see>.
    /// </summary>
    public sealed class DynamicVariableSyncMethod : DynamicReferenceVariableSyncValueBase<DynamicVariableSyncMethod, User>,
        IUnlinkedMonkeySyncMethod<DynamicVariableSpace, ILinkedDynamicVariableSpaceSyncObject, DynamicVariableSyncMethod>,
        ILinkedMonkeySyncMethod<DynamicVariableSpace, ILinkedDynamicVariableSpaceSyncObject, User>
    {
        /// <inheritdoc/>
        public MonkeySyncFunc<User> Function { get; }

        /// <summary>
        /// Creates a new sync method instance that wraps a <see cref="User"/> reference,
        /// changes of which can trigger the target <paramref name="function"/>.
        /// </summary>
        /// <param name="function">The delegate that can be triggered by this sync method.</param>
        public DynamicVariableSyncMethod(MonkeySyncFunc<User> function) : base(null!)
        {
            Function = function;
        }

        /// <summary>
        /// Creates a new sync method instance that wraps a <see cref="User"/> reference,
        /// changes of which can trigger the target <paramref name="action"/>.
        /// </summary>
        /// <param name="action">The delegate that can be triggered by this sync method.</param>
        public DynamicVariableSyncMethod(MonkeySyncAction action) : this(_ => action())
        { }

        /// <inheritdoc/>

        protected override void AddOnChangedHandler()
        {
            DynamicVariable.Reference.OnTargetChange += reference =>
            {
                DirectValue = reference;

                if (reference.Target is null
                 || (reference.Target != LinkObject.LocalUser && !SyncObject.IsMainExecutingUser))
                    return;

                LinkObject.RunSynchronously(() =>
                {
                    Function(Value);
                    Value = null!;
                });
            };
        }
    }
}