using FrooxEngine;
using FrooxEngine.UIX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace MonkeyLoader.Resonite.UI
{
    /// <summary>
    /// Base class for custom inspector segments that get added to <see cref="WorkerInspector"/>s for specific <typeparamref name="TWorker"/>s.
    /// </summary>
    /// <remarks>
    /// <b>Make sure to <see cref="CustomInspectorInjector.RemoveSegment">remove</see> them during Shutdown.</b>
    /// </remarks>
    /// <typeparam name="TWorker">The specific (base-) type of <see cref="Worker"/> that this custom inspector segment applies to.</typeparam>
    public abstract class CustomInspectorSegment<TWorker> : ICustomInspectorSegment where TWorker : Worker
    {
        /// <inheritdoc/>
        public abstract int Priority { get; }

        /// <remarks>
        /// Ensures that the given worker is a <typeparamref name="TWorker"/>.
        /// </remarks>
        /// <inheritdoc/>
        public virtual bool AppliesTo(Worker worker) => worker is TWorker;
    }

    /// <summary>
    /// Base class for custom inspector segments that get added to <see cref="WorkerInspector"/>s
    /// for <see cref="Worker"/>s with a specific (open) generic type.
    /// </summary>
    /// <remarks>
    /// <b>Make sure to <see cref="CustomInspectorInjector.RemoveSegment">remove</see> them during Shutdown.</b>
    /// </remarks>
    public abstract class CustomInspectorSegment : ICustomInspectorSegment
    {
        private readonly Dictionary<Type, bool> _matchCache = new();

        /// <summary>
        /// Gets the <see cref="Type.GetGenericTypeDefinition">generic type definition</see> of the base type.
        /// </summary>
        public Type BaseType { get; }

        /// <inheritdoc/>
        public abstract int Priority { get; }

        /// <summary>
        /// Creates a new instance of a custom inspector segment that get added
        /// to <see cref="WorkerInspector"/>s for a given (open) generic base type.
        /// </summary>
        /// <param name="baseType">The (open) generic base type to check for.</param>
        /// <exception cref="ArgumentException">When the <paramref name="baseType"/> isn't generic.</exception>
        protected CustomInspectorSegment(Type baseType)
        {
            if (!baseType.IsGenericType)
                throw new ArgumentException($"Type isn't generic: {baseType.FullName}", nameof(baseType));

            BaseType = baseType.GetGenericTypeDefinition();
        }

        /// <inheritdoc/>
        public virtual bool AppliesTo(Worker worker)
        {
            var type = worker.GetType();

            if (!_matchCache.TryGetValue(type, out var matches))
            {
                matches = MatchesGenericBaseType(type, out _);
                _matchCache.Add(type, matches);
            }

            return matches;
        }

        private bool MatchesGenericBaseType(Type type, [NotNullWhen(true)] out Type? concreteBaseType)
        {
            if (type is null)
            {
                concreteBaseType = null;
                return false;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == BaseType)
            {
                concreteBaseType = type;
                return true;
            }

            return MatchesGenericBaseType(type.BaseType, out concreteBaseType);
        }
    }

    /// <summary>
    /// Defines the interface for custom inspector segments used by the <see cref="CustomInspectorInjector"/>.
    /// </summary>
    /// <remarks>
    /// <b>Make sure to <see cref="CustomInspectorInjector.RemoveSegment">remove</see> them during Shutdown.</b>
    /// </remarks>
    public interface ICustomInspectorSegment
    {
        /// <summary>
        /// Gets the priority of this custom inspector segment.
        /// </summary>
        /// <value>
        /// An integer used to sort the custom inspector segments used by the <see cref="CustomInspectorInjector"/>.
        /// </value>
        public int Priority { get; }

        /// <summary>
        /// Determines whether this custom inspector segment applies to the given <see cref="Worker"/>.
        /// </summary>
        /// <param name="worker">The <see cref="Worker"/> for which an inspector is being build.</param>
        /// <returns><c>true</c> if this custom inspector segment applies to the <see cref="Worker"/>; otherwise, <c>false</c>.</returns>
        public bool AppliesTo(Worker worker);
    }
}