using FrooxEngine.UIX;
using FrooxEngine;
using System;

namespace MonkeyLoader.Resonite.UI
{
    /// <summary>
    /// Represents the events fired during construction of a <see cref="WorkerInspector"/>'s body.
    /// </summary>
    public sealed class BuildInspectorBodyEvent : BuildInspectorEvent
    {
        /// <summary>
        /// Allows adding custom inspector elements to the body of the <see cref="WorkerInspector"/> being build.
        /// </summary>
        /// <param name="ui">The <see cref="UIBuilder"/> used to build the inspector.</param>
        /// <param name="worker">The <see cref="Worker"/> for which an inspector is being build.</param>
        /// <param name="allowDuplicate">Whether the <paramref name="worker"/> is allowed to be duplicated.</param>
        /// <param name="allowDestroy">Whether the <paramref name="worker"/> is allowed to be destroyed.</param>
        /// <param name="memberFilter">A predicate that determines if a <see cref="ISyncMember">member</see> should be shown.</param>
        internal BuildInspectorBodyEvent(UIBuilder ui,
                Worker worker, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter)
            : base(ui, worker, allowDuplicate, allowDestroy, memberFilter)
        { }
    }

    /// <summary>
    /// Represents the base class for the events fired during construction of a <see cref="WorkerInspector"/>
    /// </summary>
    public abstract class BuildInspectorEvent : BuildUIEvent
    {
        /// <summary>
        /// Gets whether the <see cref="Worker">Worker</see> is allowed to be destroyed.
        /// </summary>
        public bool AllowDestroy { get; }

        /// <summary>
        /// Gets whether the <see cref="Worker">Worker</see> is allowed to be duplicated.
        /// </summary>
        public bool AllowDuplicate { get; }

        /// <summary>
        /// Gets the predicate that determines if a <see cref="ISyncMember">member</see> should be shown,
        /// which was passed into the inspector UI building method initially.
        /// </summary>
        public Predicate<ISyncMember>? MemberFilter { get; }

        /// <summary>
        /// Gets the <see cref="FrooxEngine.Worker"/> for which an inspector is being build.
        /// </summary>
        public Worker Worker { get; }

        /// <summary>
        /// Allows adding custom inspector elements to the <see cref="WorkerInspector"/> being build.
        /// </summary>
        /// <param name="ui">The <see cref="UIBuilder"/> used to build the inspector.</param>
        /// <param name="worker">The <see cref="Worker"/> for which an inspector is being build.</param>
        /// <param name="allowDuplicate">Whether the <paramref name="worker"/> is allowed to be duplicated.</param>
        /// <param name="allowDestroy">Whether the <paramref name="worker"/> is allowed to be destroyed.</param>
        /// <param name="memberFilter">A predicate that determines if a <see cref="ISyncMember">member</see> should be shown.</param>
        protected BuildInspectorEvent(UIBuilder ui, Worker worker, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember>? memberFilter)
            : base(ui)
        {
            Worker = worker;
            AllowDuplicate = allowDuplicate;
            AllowDestroy = allowDestroy;
            MemberFilter = memberFilter;
        }

        /// <summary>
        /// Determines whether the given <see cref="ISyncMember">member</see> should be shown.
        /// </summary>
        /// <remarks>
        /// This uses the <see cref="MemberFilter">MemberFilter</see> or just returns <c>true</c> otherwise.
        /// </remarks>
        /// <param name="member">The member to test.</param>
        /// <returns><c>true</c> if the member should be shown; otherwise, <c>false</c>.</returns>
        public bool FilterMember(ISyncMember member)
            => MemberFilter?.Invoke(member) ?? true;
    }

    /// <summary>
    /// Represents the events fired during construction of a <see cref="WorkerInspector"/>'s header.
    /// </summary>
    public sealed class BuildInspectorHeaderEvent : BuildInspectorEvent
    {
        /// <summary>
        /// Gets the current position in the inspector's header.
        /// </summary>
        public InspectorHeaderPosition HeaderPosition { get; }

        /// <summary>
        /// Allows adding custom inspector elements to the header of the <see cref="WorkerInspector"/> being build.
        /// </summary>
        /// <param name="headerPosition">The current position in the inspector's header.</param>
        /// <param name="ui">The <see cref="UIBuilder"/> used to build the inspector.</param>
        /// <param name="worker">The <see cref="Worker"/> for which an inspector is being build.</param>
        /// <param name="allowDuplicate">Whether the <paramref name="worker"/> is allowed to be duplicated.</param>
        /// <param name="allowDestroy">Whether the <paramref name="worker"/> is allowed to be destroyed.</param>
        /// <param name="memberFilter">A predicate that determines if a <see cref="ISyncMember">member</see> should be shown.</param>
        internal BuildInspectorHeaderEvent(InspectorHeaderPosition headerPosition, UIBuilder ui,
                Worker worker, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter)
            : base(ui, worker, allowDuplicate, allowDestroy, memberFilter)
        {
            HeaderPosition = headerPosition;
        }
    }
}