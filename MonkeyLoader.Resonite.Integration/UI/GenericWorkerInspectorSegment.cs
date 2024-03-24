using FrooxEngine;
using FrooxEngine.UIX;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.UI
{
    /// <summary>
    /// Base class for custom inspector segments that get added to <see cref="WorkerInspector"/>s for specific <typeparamref name="TWorker"/>s
    /// and also do a test for a given (open) generic base type.
    /// </summary>
    /// <typeparam name="TWorker">The specific (base-) type of <see cref="Worker"/> that this custom inspector segment applies to.</typeparam>
    public abstract class GenericWorkerInspectorSegment<TWorker> : GenericWorkerInspectorSegment where TWorker : Worker
    {
        /// <inheritdoc/>
        protected GenericWorkerInspectorSegment(Type baseType) : base(baseType)
        { }

        /// <remarks>
        /// Ensures that the given worker is a <typeparamref name="TWorker"/>.
        /// </remarks>
        /// <inheritdoc/>
        public override bool AppliesTo(Worker worker) => worker is TWorker && base.AppliesTo(worker);

        /// <remarks>
        /// Ensures that the given worker is a <typeparamref name="TWorker"/>, passing the call to the specific
        /// <see cref="BuildInspectorUI(TWorker, UIBuilder, Predicate{ISyncMember})">BuildInspectorUI</see> method.
        /// </remarks>
        /// <inheritdoc/>
        public override void BuildInspectorUI(Worker worker, UIBuilder ui, Predicate<ISyncMember> memberFilter)
            => BuildInspectorUI((TWorker)worker, ui, memberFilter);

        /// <summary>
        /// Adds the custom inspector elements of this segment to the <see cref="WorkerInspector"/> being build.
        /// </summary>
        /// <param name="worker">The <typeparamref name="TWorker"/> for which an inspector is being build.</param>
        /// <param name="ui">The <see cref="UIBuilder"/> used to build the inspector.</param>
        /// <param name="memberFilter">A predicate that determines if a <see cref="ISyncMember">member</see> should be shown.</param>
        public abstract void BuildInspectorUI(TWorker worker, UIBuilder ui, Predicate<ISyncMember> memberFilter);
    }

    /// <summary>
    /// Base class for custom inspector segments that get added
    /// to <see cref="WorkerInspector"/>s for a given (open) generic base type.
    /// </summary>
    public abstract class GenericWorkerInspectorSegment : ICustomInspectorSegment
    {
        private readonly Dictionary<Type, bool> _matchCache = new();

        /// <summary>
        /// Gets the <see cref="Type.GetGenericTypeDefinition">generic type definition</see> of the base type.
        /// </summary>
        public Type BaseType { get; }

        /// <summary>
        /// Gets the <see cref="Type.GetGenericArguments">generic arguments</see> that the base type had (if any).
        /// </summary>
        public Type[] GenericArguments { get; }

        /// <inheritdoc/>
        public abstract int Priority { get; }

        /// <summary>
        /// Creates a new instance of a custom inspector segment that get added
        /// to <see cref="WorkerInspector"/>s for a given (open) generic base type.
        /// </summary>
        /// <param name="baseType">The (open) generic base type to check for.</param>
        /// <exception cref="ArgumentException">When the <paramref name="baseType"/> isn't generic.</exception>
        public GenericWorkerInspectorSegment(Type baseType)
        {
            if (!baseType.IsGenericType)
                throw new ArgumentException($"Type isn't generic: {baseType.FullName}", nameof(baseType));

            BaseType = baseType.GetGenericTypeDefinition();
            GenericArguments = baseType.GetGenericArguments();
        }

        /// <inheritdoc/>
        public virtual bool AppliesTo(Worker worker)
        {
            var type = worker.GetType();

            if (!_matchCache.TryGetValue(type, out var matches))
            {
                matches = MatchesBaseType(type);
                _matchCache.Add(type, matches);
            }

            return matches;
        }

        /// <inheritdoc/>
        public abstract void BuildInspectorUI(Worker worker, UIBuilder ui, Predicate<ISyncMember> memberFilter);

        private bool MatchesBaseType(Type type)
        {
            if (!MatchesGenericBaseType(type, out var concreteBaseType))
                return false;

            var concreteArguments = concreteBaseType.GetGenericArguments();

            // TODO: This doesn't respect nested open generics or assignability
            return concreteArguments.Take(GenericArguments.Length).SequenceEqual(GenericArguments);
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
}