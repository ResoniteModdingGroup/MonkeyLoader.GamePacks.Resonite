using FrooxEngine;
using FrooxEngine.UIX;
using System;

namespace MonkeyLoader.Resonite.UI
{
    /// <summary>
    /// Custom inspector segment that uses delegates to determine
    /// which <see cref="Worker"/>s it applies to and how to build the UI.
    /// </summary>
    public sealed class CustomInspectorSegment : ICustomInspectorSegment
    {
        private readonly BuildCustomInspectorUI _buildInspectorUISegment;
        private readonly Predicate<Worker> _predicate;

        /// <inheritdoc/>
        public int Priority { get; }

        /// <summary>
        /// Creates a new instance of a custom inspector segment that uses delegates
        /// to determine which <see cref="Worker"/>s it applies to and how to build the UI.
        /// </summary>
        /// <param name="predicate">Determine whether this segment applies to the given <see cref="Worker"/>.</param>
        /// <param name="buildInspectorUI">Adds the custom inspector elements of this segment to the <see cref="WorkerInspector"/> being build.</param>
        /// <param name="priority">The priority for this custom inspector segment.</param>
        public CustomInspectorSegment(Predicate<Worker> predicate, BuildCustomInspectorUI buildInspectorUI, int priority = 0)
        {
            _predicate = predicate;
            _buildInspectorUISegment = buildInspectorUI;
            Priority = priority;
        }

        /// <inheritdoc/>
        public bool AppliesTo(Worker worker) => _predicate(worker);

        /// <inheritdoc/>
        public void BuildInspectorUI(Worker worker, UIBuilder ui, Predicate<ISyncMember> memberFilter)
            => _buildInspectorUISegment(worker, ui, memberFilter);
    }

    /// <summary>
    /// Custom inspector segment that gets added to <see cref="WorkerInspector"/>s
    /// for a given (open) generic base type and uses a delegate to build the UI.
    /// </summary>
    public sealed class GenericWorkerDelegateInspectorSegment : GenericWorkerInspectorSegment
    {
        private readonly BuildCustomInspectorUI _buildInspectorUI;

        /// <inheritdoc/>
        public override int Priority { get; }

        /// <summary>
        /// Creates a new instance of a custom inspector segment that gets added to
        /// <see cref="WorkerInspector"/>s for a given (open) generic base type and uses a delegate to build the UI.
        /// </summary>
        /// <param name="baseType">The (open) generic base type to check for.</param>
        /// <param name="buildInspectorUI">Adds the custom inspector elements of this segment to the <see cref="WorkerInspector"/> being build.</param>
        /// <param name="priority">The priority for this custom inspector segment.</param>
        public GenericWorkerDelegateInspectorSegment(Type baseType, BuildCustomInspectorUI buildInspectorUI, int priority = 0)
            : base(baseType)
        {
            _buildInspectorUI = buildInspectorUI;
            Priority = priority;
        }

        /// <inheritdoc/>
        public override void BuildInspectorUI(Worker worker, UIBuilder ui, Predicate<ISyncMember> memberFilter)
            => _buildInspectorUI(worker, ui, memberFilter);
    }
}