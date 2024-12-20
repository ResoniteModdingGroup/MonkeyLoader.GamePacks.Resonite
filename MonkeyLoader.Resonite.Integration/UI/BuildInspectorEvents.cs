using FrooxEngine.UIX;
using FrooxEngine;
using System;
using MonkeyLoader.Resonite.Events;
using MonkeyLoader.Events;

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
        /// <param name="inspector">The <see cref="WorkerInspector"/> being build.</param>
        /// <param name="worker">The <see cref="Worker"/> for which an inspector is being build.</param>
        /// <param name="allowContainer">Whether the <paramref name="worker"/> is allowed to have its <see cref="Slot"/> opened.</param>
        /// <param name="allowDuplicate">Whether the <paramref name="worker"/> is allowed to be duplicated.</param>
        /// <param name="allowDestroy">Whether the <paramref name="worker"/> is allowed to be destroyed.</param>
        /// <param name="memberFilter">A predicate that determines if a <see cref="ISyncMember">member</see> should be shown.</param>
        internal BuildInspectorBodyEvent(UIBuilder ui, WorkerInspector inspector, Worker worker,
                bool allowContainer, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter)
            : base(ui, inspector, worker, allowContainer, allowDuplicate, allowDestroy, memberFilter)
        { }
    }

    /// <summary>
    /// Represents the base class for the events fired during construction of a <see cref="WorkerInspector"/>.
    /// </summary>
    /// <remarks>
    /// This base class is dispatched as an event as well.
    /// </remarks>
    [DispatchableBaseEvent]
    public abstract class BuildInspectorEvent : BuildUIEvent
    {
        /// <summary>
        /// Gets whether the <see cref="Worker">Worker</see> is allowed to have its <see cref="Slot"/> opened.
        /// </summary>
        public bool AllowContainer { get; }

        /// <summary>
        /// Gets whether the <see cref="Worker">Worker</see> is allowed to be destroyed.
        /// </summary>
        public bool AllowDestroy { get; }

        /// <summary>
        /// Gets whether the <see cref="Worker">Worker</see> is allowed to be duplicated.
        /// </summary>
        public bool AllowDuplicate { get; }

        /// <summary>
        /// Gets the <see cref="WorkerInspector"/> being build.
        /// </summary>
        public WorkerInspector Inspector { get; }

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
        /// Allows adding custom inspector elements to the <see cref="Inspector"/> being build.
        /// </summary>
        /// <param name="ui">The <see cref="UIBuilder"/> used to build the inspector.</param>
        /// <param name="inspector">The <see cref="WorkerInspector"/> being build.</param>
        /// <param name="worker">The <see cref="Worker"/> for which an inspector is being build.</param>
        /// <param name="allowContainer">Whether the <paramref name="worker"/> is allowed to have its <see cref="Slot"/> opened.</param>
        /// <param name="allowDuplicate">Whether the <paramref name="worker"/> is allowed to be duplicated.</param>
        /// <param name="allowDestroy">Whether the <paramref name="worker"/> is allowed to be destroyed.</param>
        /// <param name="memberFilter">A predicate that determines if a <see cref="ISyncMember">member</see> should be shown.</param>
        protected BuildInspectorEvent(UIBuilder ui, WorkerInspector inspector, Worker worker,
                bool allowContainer, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember>? memberFilter)
            : base(ui)
        {
            Worker = worker;
            Inspector = inspector;
            AllowContainer = allowContainer;
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
        /// Gets whether the inspector header that's currently being build needs to have a Destroy button created.
        /// </summary>
        public bool CreateDestroyButton => AllowDestroy && !HasDestroyButton;

        /// <summary>
        /// Gets whether the inspector header that's currently being build needs to have a Duplicate button created.
        /// </summary>
        public bool CreateDuplicateButton => AllowDuplicate && !HasDuplicateButton;

        /// <summary>
        /// Gets whether the inspector header that's currently being build needs to and can have an Open Container button created.
        /// </summary>
        public bool CreateOpenContainerButton => AllowContainer && !HasOpenContainerButton && Worker.FindNearestParent<Slot>() != null;

        /// <summary>
        /// Gets whether the inspector header that's currently being build needs to have a Worker Name button created.
        /// </summary>
        public bool CreateWorkerNameButton => !HasWorkerNameButton;

        /// <summary>
        /// Gets whether the inspector header that's currently being build already has a Destroy button.
        /// </summary>
        public bool HasDestroyButton { get; set; }

        /// <summary>
        /// Gets whether the inspector header that's currently being build already has a Duplicate button.
        /// </summary>
        public bool HasDuplicateButton { get; set; }

        /// <summary>
        /// Gets whether the inspector header that's currently being build already has an Open Container button.
        /// </summary>
        public bool HasOpenContainerButton { get; set; }

        /// <summary>
        /// Gets whether the inspector header that's currently being build already has a Worker Name button.
        /// </summary>
        public bool HasWorkerNameButton { get; set; }

        /// <summary>
        /// Allows adding custom inspector elements to the header of the <see cref="WorkerInspector"/> being build.
        /// </summary>
        /// <param name="ui">The <see cref="UIBuilder"/> used to build the inspector.</param>
        /// <param name="inspector">The <see cref="WorkerInspector"/> being build.</param>
        /// <param name="worker">The <see cref="Worker"/> for which an inspector is being build.</param>
        /// <param name="allowContainer">Whether the <paramref name="worker"/> is allowed to have its <see cref="Slot"/> opened.</param>
        /// <param name="allowDuplicate">Whether the <paramref name="worker"/> is allowed to be duplicated.</param>
        /// <param name="allowDestroy">Whether the <paramref name="worker"/> is allowed to be destroyed.</param>
        /// <param name="memberFilter">A predicate that determines if a <see cref="ISyncMember">member</see> should be shown.</param>
        internal BuildInspectorHeaderEvent(UIBuilder ui, WorkerInspector inspector, Worker worker,
                bool allowContainer, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter)
            : base(ui, inspector, worker, allowContainer, allowDuplicate, allowDestroy, memberFilter)
        { }
    }
}