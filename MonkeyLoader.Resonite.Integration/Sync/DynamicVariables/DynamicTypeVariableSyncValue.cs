using FrooxEngine;
using System;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS8631 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match constraint type.

namespace MonkeyLoader.Resonite.Sync.DynamicVariables
{
    /// <summary>
    /// Implements a MonkeySync value that uses a <see cref="DynamicTypeVariable"/>
    /// to sync <see cref="Type"/>s supported by the respective <see cref="World"/>.
    /// </summary>
    /// <inheritdoc/>
    public sealed class DynamicTypeVariableSyncValue : DynamicVariableSyncValue<Type?, DynamicTypeVariable>, IDynamicTypeVariableSyncValue
    {
        /// <value>
        /// A <see cref="Type"/> <see cref="IsSupportedType">supported</see>
        /// by the <see cref="World"/> this value is being synced through.
        /// </value>
        /// <inheritdoc/>
        public override Type? Value
        {
            get => base.Value;
            set
            {
                if (!IsSupportedType(value))
                    throw new InvalidOperationException($"Type {value.CompactDescription()} is not supported by world {SyncObject.LinkObject.World.Name}");

                base.Value = value;
            }
        }

        /// <inheritdoc/>
        public DynamicTypeVariableSyncValue(Type? value) : base(value)
        { }

        /// <summary>
        /// Determines whether the given <see cref="Type"/> is <see cref="TypeManager.IsSupported">supported</see>
        /// by the <see cref="World"/> this value is being synced through.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to check.</param>
        /// <returns><c>true</c> if <paramref name="type"/> is <c>null</c> or supported; otherwise, <c>false</c>.</returns>
        public bool IsSupportedType([NotNullWhen(false)] Type? type)
            => type is null || SyncObject.LinkObject.World.Types.IsSupported(type);

        /// <inheritdoc/>
        protected override void AddOnChangedHandler()
            => DynamicVariable.Value.OnValueChange += field => DirectValue = field.Value;
    }

    /// <summary>
    /// Defines the generic interface for linked MonkeySync values that use a <see cref="DynamicTypeVariable"/>
    /// to sync <see cref="Type"/>s supported by the respective <see cref="World"/>.
    /// </summary>
    /// <inheritdoc/>
    public interface IDynamicTypeVariableSyncValue : ILinkedDynamicVariableSyncValue<Type?>
    {
        /// <inheritdoc/>
        public new DynamicTypeVariable DynamicVariable { get; }
    }
}

#pragma warning restore CS8631 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match constraint type.