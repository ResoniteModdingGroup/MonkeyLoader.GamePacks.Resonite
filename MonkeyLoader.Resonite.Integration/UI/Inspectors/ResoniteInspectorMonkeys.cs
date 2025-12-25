using FrooxEngine;
using MonkeyLoader.Events;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.UI.Inspectors
{
    /// <summary>
    /// Represents the base class for patchers that run after Resonite's assemblies have been loaded and that hook into the game's lifecycle.<br/>
    /// Specifically, to act as an <see cref="IEventHandler{TEvent}">event handler</see> for <see cref="BuildInspectorEvent"/>s
    /// for <see cref="Worker"/>s of a(n open) generic base type.
    /// </summary>
    /// <inheritdoc/>
    public abstract class ResoniteInspectorMonkey<TMonkey, TEvent> : ResoniteEventHandlerMonkey<TMonkey, TEvent>
        where TMonkey : ResoniteInspectorMonkey<TMonkey, TEvent>, new()
        where TEvent : BuildInspectorEvent
    {
        private readonly Dictionary<Type, bool> _matchCache = [];

        /// <summary>
        /// Gets the <see cref="Type.GetGenericTypeDefinition">generic type definition</see> of the base type.
        /// </summary>
        public Type BaseType { get; }

        /// <summary>
        /// Allows creating only a single <typeparamref name="TMonkey"/> instance of this custom inspector segment
        /// that gets added to <see cref="WorkerInspector"/>s for a given (open) generic base type.
        /// </summary>
        /// <param name="baseType">The (open) generic base type to check for.</param>
        /// <exception cref="ArgumentException">When the <paramref name="baseType"/> isn't generic.</exception>
        protected ResoniteInspectorMonkey(Type baseType)
        {
            if (!baseType.IsGenericType)
                throw new ArgumentException($"Type isn't generic: {baseType.FullName}", nameof(baseType));

            BaseType = baseType.GetGenericTypeDefinition();
        }

        /// <remarks>
        /// Ensures that this monkey is <see cref="MonkeyBase{T}.Enabled">enabled</see>
        /// and that the worker given in the event derives from the <see cref="BaseType">base type</see>.
        /// </remarks>
        /// <inheritdoc/>
        protected override bool AppliesTo(TEvent eventData)
        {
            if (!base.AppliesTo(eventData))
                return false;

            var type = eventData.Worker.GetType();

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

            return MatchesGenericBaseType(type.BaseType!, out concreteBaseType);
        }
    }

    /// <summary>
    /// Represents the base class for patchers that run after Resonite's assemblies have been loaded and that hook into the game's lifecycle.<br/>
    /// Specifically, to act as an <see cref="IEventHandler{TEvent}">event handler</see> for <see cref="BuildInspectorEvent"/>s
    /// for <see cref="Worker"/>s of a specific (base) type.
    /// </summary>
    /// <inheritdoc/>
    public abstract class ResoniteInspectorMonkey<TMonkey, TEvent, TWorker> : ResoniteEventHandlerMonkey<TMonkey, TEvent>
        where TMonkey : ResoniteInspectorMonkey<TMonkey, TEvent, TWorker>, new()
        where TEvent : BuildInspectorEvent
        where TWorker : Worker
    {
        /// <inheritdoc/>
        protected ResoniteInspectorMonkey()
        { }

        /// <remarks>
        /// Ensures that this monkey is <see cref="MonkeyBase{T}.Enabled">enabled</see>
        /// and that the worker given in the event is a <typeparamref name="TWorker"/>.
        /// </remarks>
        /// <inheritdoc/>
        protected override bool AppliesTo(TEvent eventData)
            => base.AppliesTo(eventData) && eventData.Worker is TWorker;
    }
}