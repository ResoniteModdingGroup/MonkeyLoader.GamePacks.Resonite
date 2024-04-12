﻿using FrooxEngine;
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
    /// <typeparam name="TEvent">The type of <see cref="BuildInspectorEvent"/>.</typeparam>
    /// <typeparam name="TWorker">The specific (base-) type of <see cref="Worker"/> that this custom inspector segment applies to.</typeparam>
    public abstract class CustomInspectorSegment<TEvent, TWorker>
        where TEvent : BuildInspectorEvent
        where TWorker : Worker
    {
        /// <inheritdoc/>
        public abstract int Priority { get; }

        /// <remarks>
        /// Ensures that the worker given in the event is a <typeparamref name="TWorker"/>.
        /// </remarks>
        /// <inheritdoc/>
        protected virtual bool AppliesTo(TEvent eventData) => eventData.Worker is TWorker;
    }

    /// <summary>
    /// Base class for custom inspector segments that get added to <see cref="WorkerInspector"/>s
    /// for <see cref="Worker"/>s with a specific (open) generic type.
    /// </summary>
    public abstract class CustomInspectorSegment<TEvent> where TEvent : BuildInspectorEvent
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

        public void Handle(TEvent eventData)
        {
            if (AppliesTo(eventData))
                BuildInspector(eventData);
        }

        /// <inheritdoc/>
        protected virtual bool AppliesTo(TEvent eventData)
        {
            var type = eventData.Worker.GetType();

            if (!_matchCache.TryGetValue(type, out var matches))
            {
                matches = MatchesGenericBaseType(type, out _);
                _matchCache.Add(type, matches);
            }

            return matches;
        }

        protected abstract void BuildInspector(TEvent eventData);

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
}