using FrooxEngine.UIX;
using FrooxEngine;
using System;
using MonkeyLoader.Resonite.Events;
using MonkeyLoader.Events;
using MonkeyLoader;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MonkeyLoader.Resonite.UI.Inspectors
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
        /// The name for the <see cref="DestroyButton">Destroy button</see>.
        /// </summary>
        public const string DestroyButtonName = "Destroy";

        /// <summary>
        /// The name for the <see cref="DuplicateButton">Duplicate button</see>.
        /// </summary>
        public const string DuplicateButtonName = "Duplicate";

        /// <summary>
        /// The name for the <see cref="OpenContainerButton">Open Container button</see>.
        /// </summary>
        public const string OpenContainerButtonName = "OpenContainer";

        /// <summary>
        /// The name for the <see cref="WorkerNameButton">Worker Name button</see>.
        /// </summary>
        public const string WorkerNameButtonName = "WorkerName";

        private readonly Dictionary<string, IButton> _buttons = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets whether the inspector header that's currently being build
        /// still needs to have a <see cref="DestroyButton">Destroy button</see> created.
        /// </summary>
        public bool CreateDestroyButton => AllowDestroy && !HasDestroyButton;

        /// <summary>
        /// Gets whether the inspector header that's currently being build
        /// still needs to have a <see cref="DuplicateButton">Duplicate button</see> created.
        /// </summary>
        public bool CreateDuplicateButton => AllowDuplicate && !HasDuplicateButton;

        /// <summary>
        /// Gets whether the inspector header that's currently being build
        /// still needs to and can have an <see cref="OpenContainerButton">Open Container button</see> created.
        /// </summary>
        public bool CreateOpenContainerButton => AllowContainer && !HasOpenContainerButton && Worker.FindNearestParent<Slot>() != null;

        /// <summary>
        /// Gets whether the inspector header that's currently being build
        /// still needs to have a <see cref="WorkerNameButton">Worker Name button</see> created.
        /// </summary>
        public bool CreateWorkerNameButton => !HasWorkerNameButton;

        /// <summary>
        /// Gets the Destroy button of the inspector header that's currently being build.
        /// </summary>
        /// <value>The button or <c>null</c> if there is none yet.</value>
        [MaybeNull]
        public IButton DestroyButton
        {
            get => GetButton(DestroyButtonName);
            set => SetButton(DestroyButtonName, value);
        }

        /// <summary>
        /// Gets the Duplicate button of the inspector header that's currently being build.
        /// </summary>
        /// <value>The button or <c>null</c> if there is none yet.</value>
        [MaybeNull]
        public IButton DuplicateButton
        {
            get => GetButton(DuplicateButtonName);
            set => SetButton(DuplicateButtonName, value);
        }

        /// <summary>
        /// Gets whether the inspector header that's currently being build
        /// already has a <see cref="DestroyButton">Destroy</see> button.
        /// </summary>
        public bool HasDestroyButton => HasButton(DestroyButtonName);

        /// <summary>
        /// Gets whether the inspector header that's currently being build
        /// already has a <see cref="DuplicateButton">Duplicate</see> button.
        /// </summary>
        public bool HasDuplicateButton => HasButton(DuplicateButtonName);

        /// <summary>
        /// Gets whether the inspector header that's currently being build
        /// already has an <see cref="OpenContainerButton">Open Container</see> button.
        /// </summary>
        public bool HasOpenContainerButton => HasButton(OpenContainerButtonName);

        /// <summary>
        /// Gets whether the inspector header that's currently being build
        /// already has a <see cref="WorkerNameButton">Worker Name</see> button.
        /// </summary>
        public bool HasWorkerNameButton => HasButton(WorkerNameButtonName);

        /// <summary>
        /// Gets the Open Container button of the inspector header that's currently being build.
        /// </summary>
        /// <value>The button or <c>null</c> if there is none yet.</value>
        [MaybeNull]
        public IButton OpenContainerButton
        {
            get => GetButton(OpenContainerButtonName);
            set => SetButton(OpenContainerButtonName, value);
        }

        /// <summary>
        /// Gets the Worker Name button of the inspector header that's currently being build.
        /// </summary>
        /// <value>The button or <c>null</c> if there is none yet.</value>
        [MaybeNull]
        public IButton WorkerNameButton
        {
            get => GetButton(WorkerNameButtonName);
            set => SetButton(WorkerNameButtonName, value);
        }

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

        /// <summary>
        /// Determines whether the given <paramref name="name"/> is valid for buttons.
        /// </summary>
        /// <remarks>
        /// Names must not be <c>null</c> or whitespace.
        /// </remarks>
        /// <param name="name">The name to validate.</param>
        /// <returns><c>true</c> if the <paramref name="name"/> is valid; otherwise, <c>false</c>.</returns>
        public static bool IsValidButtonName(string name)
            => !string.IsNullOrWhiteSpace(name);

        /// <summary>
        /// Gets the button with the given <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the button to get.</param>
        /// <returns>The button with the given <paramref name="name"/> if it exists; otherwise, <c>null</c>.</returns>
        /// <exception cref="ArgumentNullException">When the <paramref name="name"/> is <see cref="IsValidButtonName">invalid</see>.</exception>
        public IButton? GetButton(string name)
        {
            if (!IsValidButtonName(name))
                throw new ArgumentNullException(nameof(name), "Name must not be null or whitespace!");

            if (_buttons.TryGetValue(name, out var button))
                return button;

            return null;
        }

        /// <summary>
        /// Determines if there is a button with the given <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the button to check for.</param>
        /// <returns><c>true</c> if there is a button with the given <paramref name="name"/>; otherwise, <c>false</c>.</returns>
        public bool HasButton(string name)
            => GetButton(name) is not null;

        /// <summary>
        /// Sets a <paramref name="button"/> for the given <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name to set the <paramref name="button"/> for.</param>
        /// <param name="button">The button to set for the <paramref name="name"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// When the <paramref name="name"/> is <see cref="IsValidButtonName">invalid</see>,
        /// or the <paramref name="button"/> is <c>null</c>.
        /// </exception>
        public void SetButton(string name, IButton button)
        {
            if (!IsValidButtonName(name))
                throw new ArgumentNullException(nameof(name), "Name must not be null or whitespace!");

            if (button is null)
                throw new ArgumentNullException(nameof(button));

            _buttons[name] = button;
        }

        /// <summary>
        /// Tries to get the button with the given <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the button to get.</param>
        /// <param name="button">The button with the given <paramref name="name"/> if it exists; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if there is a button with the given <paramref name="name"/>; otherwise, <c>false</c>.</returns>
        public bool TryGetButton(string name, [NotNullWhen(true)] out IButton? button)
        {
            button = GetButton(name);
            return button is not null;
        }
    }
}