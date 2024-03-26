using FrooxEngine;
using FrooxEngine.UIX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.UI
{
    /// <summary>
    /// Adds custom inspector elements to the header of the <see cref="WorkerInspector"/> being build.
    /// </summary>
    /// <param name="headerPosition"></param>
    /// <param name="ui">The <see cref="UIBuilder"/> used to build the inspector.</param>
    /// <param name="worker">The <typeparamref name="TWorker"/> for which an inspector is being build.</param>
    /// <param name="allowDuplicate">Whether the <paramref name="worker"/> is allowed to be duplicated.</param>
    /// <param name="allowDestroy">Whether the <paramref name="worker"/> is allowed to be destroyed.</param>
    /// <param name="memberFilter">A predicate that determines if a <see cref="ISyncMember">member</see> should be shown.</param>
    public delegate void BuildInspectorHeaderUI<TWorker>(InspectorHeaderPosition headerPosition, UIBuilder ui,
        TWorker worker, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter);

    /// <summary>
    /// Adds custom inspector elements to the header of the <see cref="WorkerInspector"/> being build.
    /// </summary>
    /// <param name="headerPosition"></param>
    /// <param name="ui">The <see cref="UIBuilder"/> used to build the inspector.</param>
    /// <param name="worker">The <see cref="Worker"/> for which an inspector is being build.</param>
    /// <param name="allowDuplicate">Whether the <paramref name="worker"/> is allowed to be duplicated.</param>
    /// <param name="allowDestroy">Whether the <paramref name="worker"/> is allowed to be destroyed.</param>
    /// <param name="memberFilter">A predicate that determines if a <see cref="ISyncMember">member</see> should be shown.</param>
    public delegate void BuildInspectorHeaderUI(InspectorHeaderPosition headerPosition, UIBuilder ui,
        Worker worker, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter);

    /// <summary>
    /// Base class for custom inspector segments that get added to <see cref="WorkerInspector"/>s
    /// for specific <typeparamref name="TWorker"/>s, which add elements to the header of the <see cref="WorkerInspector"/> being build.
    /// </summary>
    /// <inheritdoc/>
    public abstract class CustomInspectorHeader<TWorker> : CustomInspectorSegment<TWorker>, ICustomInspectorHeader
        where TWorker : Worker
    {
        /// <remarks>
        /// Ensures that the given worker is a <typeparamref name="TWorker"/>, passing the call to the specific
        /// <see cref="BuildInspectorHeaderUI(InspectorHeaderPosition, UIBuilder,
        /// TWorker, bool, bool, Predicate{ISyncMember})">BuildInspectorUI</see> method.
        /// </remarks>
        /// <inheritdoc/>
        public void BuildInspectorHeaderUI(InspectorHeaderPosition headerPosition, UIBuilder ui,
            Worker worker, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter)
            => BuildInspectorHeaderUI(headerPosition, ui, (TWorker)worker, allowDuplicate, allowDestroy, memberFilter);

        /// <summary>
        /// Adds custom inspector elements to the header of the <see cref="WorkerInspector"/> being build.
        /// </summary>
        /// <param name="headerPosition"></param>
        /// <param name="ui">The <see cref="UIBuilder"/> used to build the inspector.</param>
        /// <param name="worker">The <typeparamref name="TWorker"/> for which an inspector is being build.</param>
        /// <param name="allowDuplicate">Whether the <paramref name="worker"/> is allowed to be duplicated.</param>
        /// <param name="allowDestroy">Whether the <paramref name="worker"/> is allowed to be destroyed.</param>
        /// <param name="memberFilter">A predicate that determines if a <see cref="ISyncMember">member</see> should be shown.</param>
        public abstract void BuildInspectorHeaderUI(InspectorHeaderPosition headerPosition, UIBuilder ui,
            TWorker worker, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter);
    }

    /// <summary>
    /// Base class for custom inspector segments that get added to <see cref="WorkerInspector"/>s
    /// for <see cref="Worker"/>s with a specific (open) generic type,
    /// which add elements to the header of the <see cref="WorkerInspector"/> being build.
    /// </summary>
    /// <inheritdoc/>
    public abstract class CustomInspectorHeader : CustomInspectorSegment, ICustomInspectorHeader
    {
        /// <inheritdoc/>
        protected CustomInspectorHeader(Type baseType) : base(baseType) { }

        /// <inheritdoc/>
        public abstract void BuildInspectorHeaderUI(InspectorHeaderPosition headerPosition, UIBuilder ui, Worker worker, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter);
    }

    /// <summary>
    /// Defines the interface for custom inspector segments used by the <see cref="CustomInspectorInjector"/>,
    /// which add elements to the header of the <see cref="WorkerInspector"/> being build.
    /// </summary>
    /// <inheritdoc/>
    public interface ICustomInspectorHeader : ICustomInspectorSegment
    {
        /// <summary>
        /// Adds custom inspector elements to the header of the <see cref="WorkerInspector"/> being build.
        /// </summary>
        /// <param name="headerPosition"></param>
        /// <param name="ui">The <see cref="UIBuilder"/> used to build the inspector.</param>
        /// <param name="worker">The <see cref="Worker"/> for which an inspector is being build.</param>
        /// <param name="allowDuplicate">Whether the <paramref name="worker"/> is allowed to be duplicated.</param>
        /// <param name="allowDestroy">Whether the <paramref name="worker"/> is allowed to be destroyed.</param>
        /// <param name="memberFilter">A predicate that determines if a <see cref="ISyncMember">member</see> should be shown.</param>
        public void BuildInspectorHeaderUI(InspectorHeaderPosition headerPosition, UIBuilder ui,
            Worker worker, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter);
    }
}