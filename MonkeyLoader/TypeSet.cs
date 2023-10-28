using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader
{
    /// <summary>
    /// Represents an array of <see cref="Type"/>s which uses sequence equality semantics.
    /// </summary>
    public readonly struct TypeSet : IEquatable<TypeSet>, IEnumerable<Type>
    {
        private readonly Type[] _types;

        /// <summary>
        /// Gets the number of types in this type set.
        /// </summary>
        public int Length => _types.Length;

        /// <summary>
        /// Creates a new type definition with the given types.
        /// </summary>
        /// <param name="types">The types that form the set.</param>
        public TypeSet(params Type[]? types)
        {
            _types = types ?? Array.Empty<Type>();
        }

        /// <summary>
        /// Creates a new type definition with the given types.
        /// </summary>
        /// <param name="types">The types that form the set.</param>
        public TypeSet(IEnumerable<Type>? types) : this(types?.ToArray())
        { }

        public static implicit operator Type[](in TypeSet typeSet) => typeSet._types.ToArray();

        public static implicit operator TypeSet(Type[]? types) => new(types);

        public static implicit operator TypeSet(Type? type)
        {
            if (type is null)
                return new(Array.Empty<Type>());

            return new(type);
        }

        public static bool operator !=(in TypeSet left, in TypeSet right) => !(left == right);

        public static bool operator ==(in TypeSet left, in TypeSet right)
            => ReferenceEquals(left, right) || left._types.SequenceEqual(right._types);

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is TypeSet set && Equals(set);

        /// <inheritdoc/>
        public bool Equals(TypeSet other) => other == this;

        /// <inheritdoc/>
        public IEnumerator<Type> GetEnumerator() => ((IEnumerable<Type>)_types).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _types.GetEnumerator();

        /// <inheritdoc/>
        public override int GetHashCode()
            => unchecked(_types.Aggregate(0, (acc, type) => (31 * acc) + type.GetHashCode()));
    }
}