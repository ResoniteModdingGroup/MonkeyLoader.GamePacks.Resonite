using FrooxEngine;
using MonkeyLoader.Events;
using MonkeyLoader.Resonite.UI.ContextMenus;

namespace MonkeyLoader.Resonite.UI.Inspectors
{
    /// <summary>
    /// Represents a dispatchable base class for all <see cref="FieldDriveReceiver{T}"/> <see cref="ContextMenu"/> items generation events.
    /// </summary>
    [DispatchableBaseEvent, SubscribableBaseEvent]
    public abstract class FieldDriveReceiverMenuItemsGenerationEvent : ContextMenuItemsGenerationEvent, ITargetSyncMemberEvent
    {
        /// <inheritdoc/>
        public Slot? Slot => SlotCore;

        /// <summary>
        /// Gets the <see cref="IField"/> that the summoning <see cref="FieldDriveReceiver{T}"/> targets.
        /// </summary>
        public IField Target => TargetCore;

        ISyncMember ITargetSyncMemberEvent.Target => Target;

        /// <inheritdoc/>
        public User? User => UserCore;

        /// <summary>
        /// Internal implementation of <see cref="Slot"/>.
        /// </summary>
        protected abstract Slot? SlotCore { get; }

        /// <summary>
        /// Internal implementation of <see cref="Target"/>.
        /// </summary>
        protected abstract IField TargetCore { get; }

        /// <summary>
        /// Internal implementation of <see cref="User"/>.
        /// </summary>
        protected abstract User? UserCore { get; }

        /// <inheritdoc/>
        protected FieldDriveReceiverMenuItemsGenerationEvent(ContextMenu contextMenu)
            : base(contextMenu)
        { }
    }

    /// <summary>
    /// Represents the event data for the concrete <see cref="FieldDriveReceiver{T}"/> <see cref="ContextMenu"/> items generation events.
    /// </summary>
    public sealed class FieldDriveReceiverMenuItemsGenerationEvent<T> : FieldDriveReceiverMenuItemsGenerationEvent
    {
        /// <summary>
        /// Gets the <see cref="FieldDriveReceiver{T}"/> that the <see cref="ContextMenu">ContextMenu</see> is being summoned by.
        /// </summary>
        public new FieldDriveReceiver<T> Summoner { get; }

        /// <summary>
        /// Gets the <see cref="IField{T}"/> that the summoning <see cref="FieldDriveReceiver{T}"/> targets.
        /// </summary>
        public new IField<T> Target { get; }

        /// <inheritdoc/>
        protected override sealed Slot? SlotCore { get; }

        /// <inheritdoc/>
        protected override sealed IWorldElement SummonerCore => Summoner;

        /// <inheritdoc/>
        protected override sealed IField TargetCore => Target;

        /// <inheritdoc/>
        protected override sealed User? UserCore { get; }

        /// <inheritdoc/>
        public FieldDriveReceiverMenuItemsGenerationEvent(ContextMenu contextMenu)
            : base(contextMenu)
        {
            Summoner = ContextMenu.CurrentSummoner as FieldDriveReceiver<T>
                ?? throw new ArgumentException($"Summoner was null or not of type {typeof(FieldDriveReceiver<T>).CompactDescription()} for Context Menu: {contextMenu.ParentHierarchyToString()}");

            Target = Summoner.Field.Target;

            SlotCore = Target.FindNearestParent<Slot>();
            UserCore = Slot?.ActiveUser ?? Target.FindNearestParent<User>();
        }
    }
}