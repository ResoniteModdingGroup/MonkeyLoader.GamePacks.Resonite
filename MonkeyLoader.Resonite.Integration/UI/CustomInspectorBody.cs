using FrooxEngine.UIX;
using FrooxEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.UI
{
    /// <summary>
    /// Adds the custom inspector elements of this segment to the <see cref="WorkerInspector"/> being build.
    /// </summary>
    /// <param name="ui">The <see cref="UIBuilder"/> used to build the inspector.</param>
    /// <param name="worker">The <typeparamref name="TWorker"/> for which an inspector is being build.</param>
    /// <param name="allowDuplicate">Whether the <paramref name="worker"/> is allowed to be duplicated.</param>
    /// <param name="allowDestroy">Whether the <paramref name="worker"/> is allowed to be destroyed.</param>
    /// <param name="memberFilter">A predicate that determines if a <see cref="ISyncMember">member</see> should be shown.</param>
    public delegate void BuildInspectorBodyUI<TWorker>(UIBuilder ui, TWorker worker, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter) where TWorker : Worker;

    /// <summary>
    /// Adds the custom inspector elements of this segment to the <see cref="WorkerInspector"/> being build.
    /// </summary>
    /// <param name="ui">The <see cref="UIBuilder"/> used to build the inspector.</param>
    /// <param name="worker">The <see cref="Worker"/> for which an inspector is being build.</param>
    /// <param name="allowDuplicate">Whether the <paramref name="worker"/> is allowed to be duplicated.</param>
    /// <param name="allowDestroy">Whether the <paramref name="worker"/> is allowed to be destroyed.</param>
    /// <param name="memberFilter">A predicate that determines if a <see cref="ISyncMember">member</see> should be shown.</param>
    public delegate void BuildInspectorBodyUI(UIBuilder ui, Worker worker, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter);

    /// <summary>
    /// Base class for custom inspector segments that get added to <see cref="WorkerInspector"/>s
    /// for specific <typeparamref name="TWorker"/>s, which add elements to the body of the <see cref="WorkerInspector"/> being build.
    /// </summary>
    /// <inheritdoc/>
    public abstract class CustomInspectorBody<TWorker> : CustomInspectorSegment<TWorker>, ICustomInspectorBody
        where TWorker : Worker
    {
        /// <remarks>
        /// Ensures that the given worker is a <typeparamref name="TWorker"/>, passing the call to the specific
        /// <see cref="BuildInspectorBodyUI(UIBuilder, TWorker, bool, bool, Predicate{ISyncMember})">BuildInspectorUI</see> method.
        /// </remarks>
        /// <inheritdoc/>
        public void BuildInspectorBodyUI(UIBuilder ui, Worker worker, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter)
            => BuildInspectorBodyUI(ui, (TWorker)worker, allowDuplicate, allowDestroy, memberFilter);

        /// <summary>
        /// Adds custom inspector elements to the body of the <see cref="WorkerInspector"/> being build.
        /// </summary>
        /// <param name="ui">The <see cref="UIBuilder"/> used to build the inspector.</param>
        /// <param name="worker">The <typeparamref name="TWorker"/> for which an inspector is being build.</param>
        /// <param name="allowDuplicate">Whether the <paramref name="worker"/> is allowed to be duplicated.</param>
        /// <param name="allowDestroy">Whether the <paramref name="worker"/> is allowed to be destroyed.</param>
        /// <param name="memberFilter">A predicate that determines if a <see cref="ISyncMember">member</see> should be shown.</param>
        public abstract void BuildInspectorBodyUI(UIBuilder ui, TWorker worker, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter);
    }

    /// <summary>
    /// Base class for custom inspector segments that get added to <see cref="WorkerInspector"/>s
    /// for <see cref="Worker"/>s with a specific (open) generic type,
    /// which add elements to the body of the <see cref="WorkerInspector"/> being build.
    /// </summary>
    public abstract class CustomInspectorBody : CustomInspectorSegment, ICustomInspectorBody
    {
        /// <inheritdoc/>
        protected CustomInspectorBody(Type baseType) : base(baseType) { }

        /// <inheritdoc/>
        public abstract void BuildInspectorBodyUI(UIBuilder ui, Worker worker, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter);
    }

    /// <summary>
    /// Defines the interface for custom inspector segments used by the <see cref="CustomInspectorInjector"/>,
    /// which add elements to the body of the <see cref="WorkerInspector"/> being build.
    /// </summary>
    /// <inheritdoc/>
    public interface ICustomInspectorBody : ICustomInspectorSegment
    {
        /// <summary>
        /// Adds custom inspector elements to the body of the <see cref="WorkerInspector"/> being build.
        /// </summary>
        /// <param name="ui">The <see cref="UIBuilder"/> used to build the inspector.</param>
        /// <param name="worker">The <see cref="Worker"/> for which an inspector is being build.</param>
        /// <param name="allowDuplicate">Whether the <paramref name="worker"/> is allowed to be duplicated.</param>
        /// <param name="allowDestroy">Whether the <paramref name="worker"/> is allowed to be destroyed.</param>
        /// <param name="memberFilter">A predicate that determines if a <see cref="ISyncMember">member</see> should be shown.</param>
        public void BuildInspectorBodyUI(UIBuilder ui, Worker worker, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter);
    }
}