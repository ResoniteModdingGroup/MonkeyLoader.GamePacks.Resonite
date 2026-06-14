using FrooxEngine;
using System.Collections.Immutable;

namespace MonkeyLoader.Resonite.Events
{
    /// <summary>
    /// Represents a complete set of information about the <see cref="IGrabbable"/>s
    /// that a <see cref="User"/> is or was holding.
    /// </summary>
    /// <remarks>
    /// The <see cref="IValueSource">value</see> and <see cref="IReferenceSource">reference</see> sources'
    /// <see cref="IValueSource.BoxedValue">boxed values</see> and
    /// <see cref="IReferenceSource.UntypedReference">untyped references</see> respectively,
    /// as well as the <see cref="IDelegateProxy">delegate proxies</see>'
    /// <see cref="IDelegateProxy.UntypedDelegate">untyped delegates</see>
    /// are collected, since they're usually ephemeral and get removed as soon as they're dropped.
    /// All returned elements are automatically filtered to not contain any removed ones.
    /// </remarks>
    public sealed class GrabbableInfo
    {
        private readonly ImmutableArray<object> _boxedValues;
        private readonly ImmutableArray<IGrabbable> _grabbables;

        private readonly ImmutableArray<Delegate> _untypedDelegates;
        private readonly ImmutableArray<IWorldElement> _untypedReferences;

        /// <summary>
        /// Gets an empty instance of the grabble info.
        /// </summary>
        public static GrabbableInfo Empty { get; } = new([]);

        /// <summary>
        /// Gets the <see cref="IValueSource.BoxedValue">boxed values</see> of the
        /// <see cref="IValueSource"/>s that were found in the original <see cref="IGrabbable">grabbables</see>.
        /// </summary>
        public IEnumerable<object> BoxedValues => _boxedValues;

        /// <summary>
        /// Gets the non-removed <see cref="IGrabbable">grabbables</see> from the original ones.
        /// </summary>
        public IEnumerable<IGrabbable> Grabbables => _grabbables.FilterWorldElements();

        /// <summary>
        /// Gets the <see cref="Delegate"/>s with non-removed targets of the
        /// <see cref="IDelegateProxy"/>s that were found in the original <see cref="IGrabbable">grabbables</see>.
        /// </summary>
        public IEnumerable<Delegate> UntypedDelegates => _untypedDelegates.FilterWorldDelegates();

        /// <summary>
        /// Gets the non-removed <see cref="IReferenceSource.UntypedReference">untyped references</see> of the
        /// <see cref="IReferenceSource"/>s that were found in the original <see cref="IGrabbable">grabbables</see>.
        /// </summary>
        public IEnumerable<IWorldElement> UntypedReferences => _untypedReferences.FilterWorldElements();

        /// <summary>
        /// Creates a new instance of this class with the given original <paramref name="grabbables"/>.
        /// </summary>
        /// <param name="grabbables">The grabbables to process and cache the usually ephemeral information from.</param>
        public GrabbableInfo(IEnumerable<IGrabbable> grabbables)
        {
            _grabbables = [.. grabbables.FilterWorldElements()];

            _boxedValues = [.. _grabbables
                .SelectMany(GetComponentsInChildren<IValueSource>)
                .Select(static valueSource => valueSource.BoxedValue)];

            _untypedDelegates = [ .._grabbables
                .SelectMany(GetComponentsInChildren<IDelegateProxy>)
                .Select(static delegateProxy => delegateProxy.UntypedDelegate)];

            _untypedReferences = [ ..grabbables
                .SelectMany(GetComponentsInChildren<IReferenceSource>)
                .Select(static referenceSource => referenceSource.UntypedReference)];
        }

        private static List<T> GetComponentsInChildren<T>(IGrabbable grabbable)
                where T : class, IComponent
            => grabbable.Slot.GetComponentsInChildren<T>();
    }
}