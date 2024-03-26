using FrooxEngine;
using FrooxEngine.UIX;
using System;

namespace MonkeyLoader.Resonite.UI
{
    /// <summary>
    /// Custom inspector that can be added to <see cref="WorkerInspector"/>s
    /// to determine applicability and the <see cref="WorkerInspector"/> UI building with lambdas.
    /// </summary>
    /// <inheritdoc/>
    public sealed class LambdaInspector : LambdaInspectorSegment, ICustomInspectorHeader, ICustomInspectorBody
    {
        private readonly BuildInspectorBodyUI _buildInspectorBodyUI;
        private readonly BuildInspectorHeaderUI _buildInspectorHeaderUI;

        /// <summary>
        /// Creates a new instance of this custom inspector segment that uses lambdas
        /// to determine which <see cref="Worker"/>s it applies to and how to build the UI.
        /// </summary>
        /// <param name="predicate">Determine whether this segment applies to the given <see cref="Worker"/>.</param>
        /// <param name="buildInspectorHeaderUI">Adds the custom inspector elements of this segment to the <see cref="WorkerInspector"/> header being build.</param>
        /// <param name="buildInspectorBodyUI">Adds the custom inspector elements of this segment to the <see cref="WorkerInspector"/> body being build.</param>
        /// <param name="priority">The priority for this custom inspector segment.</param>
        public LambdaInspector(Predicate<Worker> predicate, BuildInspectorHeaderUI buildInspectorHeaderUI, BuildInspectorBodyUI buildInspectorBodyUI, int priority = 0)
            : base(predicate, priority)
        {
            _buildInspectorHeaderUI = buildInspectorHeaderUI;
            _buildInspectorBodyUI = buildInspectorBodyUI;
        }

        /// <inheritdoc/>
        public void BuildInspectorBodyUI(UIBuilder ui, Worker worker, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter)
            => _buildInspectorBodyUI(ui, worker, allowDuplicate, allowDestroy, memberFilter);

        /// <inheritdoc/>
        public void BuildInspectorHeaderUI(InspectorHeaderPosition headerPosition, UIBuilder ui,
           Worker worker, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter)
            => _buildInspectorHeaderUI(headerPosition, ui, worker, allowDuplicate, allowDestroy, memberFilter);
    }

    /// <summary>
    /// Custom inspector segment that can be added to <see cref="WorkerInspector"/>s
    /// to determine applicability and the <see cref="WorkerInspector"/> body UI building with lambdas.
    /// </summary>
    /// <inheritdoc/>
    public sealed class LambdaInspectorBody : LambdaInspectorSegment, ICustomInspectorBody
    {
        private readonly BuildInspectorBodyUI _buildInspectorBodyUI;

        /// <summary>
        /// Creates a new instance of this custom inspector segment that uses lambdas
        /// to determine which <see cref="Worker"/>s it applies to and how to build the UI.
        /// </summary>
        /// <param name="predicate">Determine whether this segment applies to the given <see cref="Worker"/>.</param>
        /// <param name="buildInspectorBodyUI">Adds the custom inspector elements of this segment to the <see cref="WorkerInspector"/> body being build.</param>
        /// <param name="priority">The priority for this custom inspector segment.</param>
        public LambdaInspectorBody(Predicate<Worker> predicate, BuildInspectorBodyUI buildInspectorBodyUI, int priority = 0)
            : base(predicate, priority)
        {
            _buildInspectorBodyUI = buildInspectorBodyUI;
        }

        /// <inheritdoc/>
        public void BuildInspectorBodyUI(UIBuilder ui, Worker worker, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter)
            => _buildInspectorBodyUI(ui, worker, allowDuplicate, allowDestroy, memberFilter);
    }

    /// <summary>
    /// Custom inspector segment that can be added to <see cref="WorkerInspector"/>s
    /// to determine applicability and the <see cref="WorkerInspector"/> header UI building with lambdas.
    /// </summary>
    /// <inheritdoc/>
    public sealed class LambdaInspectorHeader : LambdaInspectorSegment, ICustomInspectorHeader
    {
        private readonly BuildInspectorHeaderUI _buildInspectorHeaderUI;

        /// <summary>
        /// Creates a new instance of this custom inspector segment that uses lambdas
        /// to determine which <see cref="Worker"/>s it applies to and how to build the UI.
        /// </summary>
        /// <param name="predicate">Determine whether this segment applies to the given <see cref="Worker"/>.</param>
        /// <param name="buildInspectorHeaderUI">Adds the custom inspector elements of this segment to the <see cref="WorkerInspector"/> header being build.</param>
        /// <param name="priority">The priority for this custom inspector segment.</param>
        public LambdaInspectorHeader(Predicate<Worker> predicate, BuildInspectorHeaderUI buildInspectorHeaderUI, int priority = 0)
            : base(predicate, priority)
        {
            _buildInspectorHeaderUI = buildInspectorHeaderUI;
        }

        /// <inheritdoc/>
        public void BuildInspectorHeaderUI(InspectorHeaderPosition headerPosition, UIBuilder ui,
           Worker worker, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter)
            => _buildInspectorHeaderUI(headerPosition, ui, worker, allowDuplicate, allowDestroy, memberFilter);
    }

    /// <summary>
    /// Only here to support <see cref="LambdaInspectorHeader"/>,
    /// <see cref="LambdaInspectorBody"/>, and <see cref="LambdaInspector"/>.
    /// </summary>
    /// <remarks>
    /// <b>Make sure to <see cref="CustomInspectorInjector.RemoveSegment">remove</see> them during Shutdown.</b>
    /// </remarks>
    public abstract class LambdaInspectorSegment : ICustomInspectorSegment
    {
        private readonly Predicate<Worker> _predicate;

        /// <inheritdoc/>
        public int Priority { get; }

        /// <param name="predicate">Determine whether this segment applies to the given <see cref="Worker"/>.</param>
        /// <param name="priority">The priority for this custom inspector segment.</param>
        internal LambdaInspectorSegment(Predicate<Worker> predicate, int priority)
        {
            _predicate = predicate;
            Priority = priority;
        }

        /// <inheritdoc/>
        public bool AppliesTo(Worker worker) => _predicate(worker);
    }
}