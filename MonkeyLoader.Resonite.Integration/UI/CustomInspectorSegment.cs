using FrooxEngine;
using FrooxEngine.UIX;
using System;

namespace MonkeyLoader.Resonite.UI
{
    /// <summary>
    /// Adds the custom inspector elements of this segment to the <see cref="WorkerInspector"/> being build.
    /// </summary>
    /// <param name="worker">The <see cref="Worker"/> for which an inspector is being build.</param>
    /// <param name="ui">The <see cref="UIBuilder"/> used to build the inspector.</param>
    /// <param name="memberFilter">A predicate that determines if a <see cref="ISyncMember">member</see> should be shown.</param>
    public delegate void BuildCustomInspectorUI(Worker worker, UIBuilder ui, Predicate<ISyncMember> memberFilter);

    /// <summary>
    /// Adds the custom inspector elements of this segment to the <see cref="WorkerInspector"/> being build.
    /// </summary>
    /// <param name="worker">The <typeparamref name="TWorker"/> for which an inspector is being build.</param>
    /// <param name="ui">The <see cref="UIBuilder"/> used to build the inspector.</param>
    /// <param name="memberFilter">A predicate that determines if a <see cref="ISyncMember">member</see> should be shown.</param>
    public delegate void BuildCustomInspectorUI<TWorker>(TWorker worker, UIBuilder ui, Predicate<ISyncMember> memberFilter) where TWorker : Worker;

    /// <summary>
    /// Base class for custom inspector segments that get added to <see cref="WorkerInspector"/>s for specific <typeparamref name="TWorker"/>s.
    /// </summary>
    /// <typeparam name="TWorker">The specific (base-) type of <see cref="Worker"/> that this custom inspector segment applies to.</typeparam>
    public abstract class CustomInspectorSegment<TWorker> : ICustomInspectorSegment where TWorker : Worker
    {
        /// <inheritdoc/>
        public abstract int Priority { get; }

        /// <remarks>
        /// Ensures that the given worker is a <typeparamref name="TWorker"/>.
        /// </remarks>
        /// <inheritdoc/>
        public bool AppliesTo(Worker worker) => worker is TWorker;

        /// <remarks>
        /// Ensures that the given worker is a <typeparamref name="TWorker"/>, passing the call to the specific
        /// <see cref="BuildInspectorUI(TWorker, UIBuilder, Predicate{ISyncMember})">BuildInspectorUI</see> method.
        /// </remarks>
        /// <inheritdoc/>
        public void BuildInspectorUI(Worker worker, UIBuilder ui, Predicate<ISyncMember> memberFilter)
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
    /// Defines the interface for custom inspector segments used by the <see cref="CustomInspectorInjector"/>.<br/>
    /// <b>Make sure to <see cref="CustomInspectorInjector.RemoveSegment">remove</see> them during Shutdown.</b>
    /// </summary>
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

        /// <summary>
        /// Adds the custom inspector elements of this segment to the <see cref="WorkerInspector"/> being build.
        /// </summary>
        /// <param name="worker">The <see cref="Worker"/> for which an inspector is being build.</param>
        /// <param name="ui">The <see cref="UIBuilder"/> used to build the inspector.</param>
        /// <param name="memberFilter">A predicate that determines if a <see cref="ISyncMember">member</see> should be shown.</param>
        public void BuildInspectorUI(Worker worker, UIBuilder ui, Predicate<ISyncMember> memberFilter);
    }
}