using FrooxEngine;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Fully describes the identity of a Dynamic Variable based on itsn <see cref="Type">Type</see>,
    /// <see cref="Name">Name</see>, and the <see cref="Space">Space</see> it's a part of.
    /// </summary>
    public readonly struct DynamicVariableIdentity : IEquatable<DynamicVariableIdentity>
    {
        /// <summary>
        /// Gets the <see cref="IDynamicVariable.VariableName">name</see> of this Dynamic Variable.
        /// </summary>
        public readonly string Name { get; }

        /// <summary>
        /// Gets the <see cref="DynamicVariableSpace"/> that this Dynamic Variable is a part of.
        /// </summary>
        public readonly DynamicVariableSpace Space { get; }

        /// <summary>
        /// Gets the <see cref="System.Type"/> of this Dynamic Variable.
        /// </summary>
        public readonly Type Type { get; }

        /// <summary>
        /// Creates a new Dynamic Variable identity with the given details.
        /// </summary>
        /// <param name="space">The <see cref="DynamicVariableSpace"/> that this Dynamic Variable is a part of.</param>
        /// <param name="type">The <see cref="System.Type"/> of this Dynamic Variable.</param>
        /// <param name="name">The <see cref="IDynamicVariable.VariableName">name</see> of this Dynamic Variable.</param>
        public DynamicVariableIdentity(DynamicVariableSpace space, Type type, string name)
        {
            Space = space;
            Type = type;
            Name = name;
        }

        /// <summary>
        /// Creates a new full Dynamic Variable identity for the given space.
        /// </summary>
        /// <param name="space">The <see cref="DynamicVariableSpace"/> that this Dynamic Variable is a part of.</param>
        /// <param name="variableIdentity">The Dynamic Variable's identity within the given <paramref name="space"/>.</param>
        public DynamicVariableIdentity(DynamicVariableSpace space, DynamicVariableSpace.VariableIdentity variableIdentity)
        {
            Space = space;
            Type = variableIdentity.type;
            Name = variableIdentity.name;
        }

        /// <summary>
        /// Determines whether two Dynamic Variable identities refer to different ones.
        /// </summary>
        /// <param name="left">The first Dynamic Variable identity.</param>
        /// <param name="right">The second Dynamic Varible identity.</param>
        /// <returns><c>true</c> if the identities are different; otherwise, <c>false</c>.</returns>
        public static bool operator !=(DynamicVariableIdentity left, DynamicVariableIdentity right)
            => !left.Equals(right);

        /// <summary>
        /// Determines whether two Dynamic Variable identities refer to the same exact one.
        /// </summary>
        /// <param name="left">The first Dynamic Variable identity.</param>
        /// <param name="right">The second Dynamic Varible identity.</param>
        /// <returns><c>true</c> if the identities are the same; otherwise, <c>false</c>.</returns>
        public static bool operator ==(DynamicVariableIdentity left, DynamicVariableIdentity right)
            => left.Equals(right);

        /// <inheritdoc/>
        public readonly bool Equals(DynamicVariableIdentity other)
            => ReferenceEquals(Space, other.Space) && Type == other.Type && Name == other.Name;

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj)
            => obj is DynamicVariableIdentity other && Equals(other);

        /// <inheritdoc/>
        public override readonly int GetHashCode()
            => HashCode.Combine(Space, Type, Name);

        /// <inheritdoc/>
        public override readonly string ToString()
            => $"Dynamic Variable {Name} of Type {Type.CompactDescription()} on {Space.GetReferenceLabel()}";
    }
}