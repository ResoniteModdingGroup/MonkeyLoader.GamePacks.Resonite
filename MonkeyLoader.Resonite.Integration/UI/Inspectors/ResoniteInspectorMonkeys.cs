using FrooxEngine;
using MonkeyLoader.Events;
using MonkeyLoader.Patching;
using System.Diagnostics.CodeAnalysis;

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
        /// Gets the base or interface type that matching workers must derive from or implement.
        /// </summary>
        /// <remarks>
        /// For <see cref="Type.IsGenericType">generic types</see>, this will be the
        /// <see cref="Type.GetGenericTypeDefinition">open generic type definition</see>.
        /// </remarks>
        public Type BaseType { get; }

        /// <summary>
        /// Allows creating only a single <typeparamref name="TMonkey"/> instance of this custom inspector segment
        /// that gets added to <see cref="WorkerInspector"/>s for a given (open) generic base type.
        /// </summary>
        /// <param name="baseType">The (open) generic base type to check for. Can be an inheritance or interface implementation.</param>
        protected ResoniteInspectorMonkey(Type baseType)
        {
            BaseType = !baseType.IsGenericType ? baseType
                : baseType.GetGenericTypeDefinition();
        }

        /// <remarks>
        /// Ensures that this monkey is <see cref="MonkeyBase{T}.Enabled">enabled</see>
        /// and that the worker given in the event derives from or implements the <see cref="BaseType">BaseType</see>.
        /// </remarks>
        /// <inheritdoc/>
        protected override bool AppliesTo(TEvent eventData)
        {
            if (!base.AppliesTo(eventData))
                return false;

            var type = eventData.Worker.GetType();

            if (!_matchCache.TryGetValue(type, out var matches))
            {
                matches = (!BaseType.IsGenericType && type.IsAssignableTo(BaseType))
                    || MatchesGenericBaseType(type, out _);

                _matchCache.Add(type, matches);
            }

            return matches;
        }

        private bool MatchesGenericBaseType(Type type, [NotNullWhen(true)] out Type? concreteBaseType)
        {
            concreteBaseType = null;

            if (type is null)
                return false;

            if (BaseType.IsInterface)
            {
                foreach (var implementedInterface in type.GetInterfaces())
                {
                    if (!implementedInterface.IsGenericType || implementedInterface.GetGenericTypeDefinition() != BaseType)
                        continue;

                    concreteBaseType = implementedInterface;
                    return true;
                }
            }

            while (type is not null)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == BaseType)
                {
                    concreteBaseType = type;
                    return true;
                }

                type = type.BaseType!;
            }

            return false;
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