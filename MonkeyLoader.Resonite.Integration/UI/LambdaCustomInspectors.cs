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
    public sealed class LambdaCustomInspector : CustomInspector, ICustomInspectorHeader, ICustomInspectorBody
    {
        private readonly BuildInspectorBodyUI _buildInspectorBodyUI;
        private readonly BuildInspectorHeaderUI _buildInspectorHeaderUI;

        /// <inheritdoc/>
        public override int Priority { get; }

        /// <summary>
        /// Creates a new instance of this custom inspector segment that uses lambdas
        /// to determine which <see cref="Worker"/>s it applies to and how to build the UI.
        /// </summary>
        /// <param name="baseType">The (open) generic base type to check for.</param>
        /// <param name="buildInspectorHeaderUI">Adds the custom inspector elements of this segment to the <see cref="WorkerInspector"/> header being build.</param>
        /// <param name="buildInspectorBodyUI">Adds the custom inspector elements of this segment to the <see cref="WorkerInspector"/> body being build.</param>
        /// <param name="priority">The priority for this custom inspector segment.</param>
        /// <exception cref="ArgumentException">When the <paramref name="baseType"/> isn't generic.</exception>
        public LambdaCustomInspector(Type baseType, BuildInspectorHeaderUI buildInspectorHeaderUI, BuildInspectorBodyUI buildInspectorBodyUI, int priority = 0)
            : base(baseType)
        {
            _buildInspectorHeaderUI = buildInspectorHeaderUI;
            _buildInspectorBodyUI = buildInspectorBodyUI;
            Priority = priority;
        }

        /// <inheritdoc/>
        public override void BuildInspectorBodyUI(UIBuilder ui, Worker worker, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter)
            => _buildInspectorBodyUI(ui, worker, allowDuplicate, allowDestroy, memberFilter);

        /// <inheritdoc/>
        public override void BuildInspectorHeaderUI(InspectorHeaderPosition headerPosition, UIBuilder ui,
           Worker worker, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter)
            => _buildInspectorHeaderUI(headerPosition, ui, worker, allowDuplicate, allowDestroy, memberFilter);
    }

    /// <summary>
    /// Custom inspector segment that can be added to <see cref="WorkerInspector"/>s
    /// to determine applicability and the <see cref="WorkerInspector"/> body UI building with lambdas.
    /// </summary>
    /// <inheritdoc/>
    public sealed class LambdaCustomInspectorBody : CustomInspectorBody
    {
        private readonly BuildInspectorBodyUI _buildInspectorBodyUI;

        /// <inheritdoc/>
        public override int Priority { get; }

        /// <summary>
        /// Creates a new instance of this custom inspector segment that uses lambdas
        /// to determine which <see cref="Worker"/>s it applies to and how to build the UI.
        /// </summary>
        /// <param name="baseType">The (open) generic base type to check for.</param>
        /// <param name="buildInspectorBodyUI">Adds the custom inspector elements of this segment to the <see cref="WorkerInspector"/> body being build.</param>
        /// <param name="priority">The priority for this custom inspector segment.</param>
        public LambdaCustomInspectorBody(Type baseType, BuildInspectorBodyUI buildInspectorBodyUI, int priority = 0)
            : base(baseType)
        {
            _buildInspectorBodyUI = buildInspectorBodyUI;
            Priority = priority;
        }

        /// <inheritdoc/>
        public override void BuildInspectorBodyUI(UIBuilder ui, Worker worker, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter)
            => _buildInspectorBodyUI(ui, worker, allowDuplicate, allowDestroy, memberFilter);
    }

    /// <summary>
    /// Custom inspector segment that can be added to <see cref="WorkerInspector"/>s
    /// to determine applicability and the <see cref="WorkerInspector"/> header UI building with lambdas.
    /// </summary>
    /// <inheritdoc/>
    public sealed class LambdaCustomInspectorHeader : CustomInspectorHeader
    {
        private readonly BuildInspectorHeaderUI _buildInspectorHeaderUI;

        /// <inheritdoc/>
        public override int Priority { get; }

        /// <summary>
        /// Creates a new instance of this custom inspector segment that uses lambdas
        /// to determine which <see cref="Worker"/>s it applies to and how to build the UI.
        /// </summary>
        /// <param name="baseType">The (open) generic base type to check for.</param>
        /// <param name="buildInspectorHeaderUI">Adds the custom inspector elements of this segment to the <see cref="WorkerInspector"/> header being build.</param>
        /// <param name="priority">The priority for this custom inspector segment.</param>
        public LambdaCustomInspectorHeader(Type baseType, BuildInspectorHeaderUI buildInspectorHeaderUI, int priority = 0)
            : base(baseType)
        {
            _buildInspectorHeaderUI = buildInspectorHeaderUI;
            Priority = priority;
        }

        /// <inheritdoc/>
        public override void BuildInspectorHeaderUI(InspectorHeaderPosition headerPosition, UIBuilder ui,
           Worker worker, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter)
            => _buildInspectorHeaderUI(headerPosition, ui, worker, allowDuplicate, allowDestroy, memberFilter);
    }
}