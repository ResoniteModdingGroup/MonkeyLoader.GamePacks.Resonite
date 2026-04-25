using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Events;
using MonkeyLoader.Resonite.UI.ContextMenus;

namespace MonkeyLoader.Resonite.UI.Inspectors
{
    /// <summary>
    /// Represents a dispatchable base class for all <see cref="FieldDriveReceiver{T}"/> and
    /// <see cref="ReferenceDriveReceiver{T}"/> <see cref="ContextMenu"/> items generation events.
    /// </summary>
    [DispatchableBaseEvent, SubscribableBaseEvent]
    public abstract class DriveReceiverMenuItemsGenerationEvent : ContextMenuItemsGenerationEvent, ITargetSyncMemberEvent
    {
        private static readonly Type[] _contextMenuConstructorParameters = [typeof(ContextMenu)];

        private static readonly Dictionary<Type, Func<ContextMenu, DriveReceiverMenuItemsGenerationEvent>> _contextMenuConstructorsBySummonerType = [];

        private static readonly Type _fieldEventType = typeof(FieldDriveReceiverMenuItemsGenerationEvent<>);
        private static readonly Type _fieldReceiverType = typeof(FieldDriveReceiver<>);
        private static readonly Type _objectType = typeof(object);
        private static readonly Type _referenceEventType = typeof(ReferenceDriveReceiverMenuItemsGenerationEvent<>);
        private static readonly Type _referenceReceiverType = typeof(ReferenceDriveReceiver<>);

        /// <inheritdoc/>
        public Slot? Slot => SlotCore;

        /// <summary>
        /// Gets the <see cref="ISyncMember"/> that the summoning
        /// <see cref="FieldDriveReceiver{T}"/> or <see cref="ReferenceDriveReceiver{T}"/> targets.<br/>
        /// This will be an <see cref="IField{T}"/> for <see cref="FieldDriveReceiver{T}"/>s,
        /// and an <see cref="ISyncRef{T}"/> for <see cref="ReferenceDriveReceiver{T}"/>s.
        /// </summary>
        public ISyncMember Target => TargetCore;

        /// <inheritdoc/>
        public User? User => UserCore;

        /// <summary>
        /// Internal implementation of <see cref="Slot"/>.
        /// </summary>
        protected abstract Slot? SlotCore { get; }

        /// <summary>
        /// Internal implementation of <see cref="Target"/>.
        /// </summary>
        protected abstract ISyncMember TargetCore { get; }

        /// <summary>
        /// Internal implementation of <see cref="User"/>.
        /// </summary>
        protected abstract User? UserCore { get; }

        /// <inheritdoc/>
        protected DriveReceiverMenuItemsGenerationEvent(ContextMenu contextMenu)
            : base(contextMenu)
        { }

        /// <summary>
        /// Creates a new <see cref="ContextMenu"/> items generation event for the given
        /// <paramref name="contextMenu"/> and its <see cref="Slot.ActiveUser">active</see> <see cref="User"/>,
        /// when the <see cref="ContextMenu.CurrentSummoner">summoner</see> is a <see cref="FieldDriveReceiver{T}"/>
        /// or <see cref="ReferenceDriveReceiver{T}"/>.
        /// </summary>
        /// <inheritdoc cref="DriveReceiverMenuItemsGenerationEvent(ContextMenu)"/>
        internal static DriveReceiverMenuItemsGenerationEvent CreateForDriveReceiver(ContextMenu contextMenu)
        {
            ArgumentNullException.ThrowIfNull(contextMenu);

            if (contextMenu.CurrentSummoner is null)
                throw new ArgumentException($"Summoner was missing for Context Menu: {contextMenu.ParentHierarchyToString()}");

            var summonerType = contextMenu.CurrentSummoner.GetType();

            var currentType = summonerType;
            Func<ContextMenu, DriveReceiverMenuItemsGenerationEvent>? contextMenuConstructor = null;

            while (currentType != _objectType)
            {
                if (_contextMenuConstructorsBySummonerType.TryGetValue(currentType, out var customContextMenuConstructor))
                {
                    contextMenuConstructor = customContextMenuConstructor;
                    break;
                }

                if (currentType.IsGenericType && currentType.GenericTypeArguments.Length is 1)
                {
                    var currentGenericType = currentType.GetGenericTypeDefinition();
                    Type parameterType = currentType.GenericTypeArguments[0];

                    if (currentGenericType == _referenceReceiverType)
                    {
                        contextMenuConstructor = GetDriveReceiverContextMenuConstructor(_referenceEventType, parameterType);
                        break;
                    }

                    if (currentGenericType == _fieldReceiverType)
                    {
                        contextMenuConstructor = GetDriveReceiverContextMenuConstructor(_fieldEventType, parameterType);
                        break;
                    }
                }

                // Will never be null since we stop when currentType is object
                currentType = currentType.BaseType!;
            }

            if (contextMenuConstructor is null)
                throw new ArgumentException($"Summoner was not of type {typeof(FieldDriveReceiver<>).CompactDescription()} or {typeof(ReferenceDriveReceiver<>).CompactDescription()} or derived from them for Context Menu: {contextMenu.ParentHierarchyToString()}");

            return contextMenuConstructor(contextMenu);
        }

        private static Func<ContextMenu, DriveReceiverMenuItemsGenerationEvent> GetDriveReceiverContextMenuConstructor(Type rawEventType, Type parameterType)
        {
            var eventType = rawEventType.MakeGenericType(parameterType);
            var constructorMethod = AccessTools.DeclaredConstructor(eventType, _contextMenuConstructorParameters);
            return contextMenu => (DriveReceiverMenuItemsGenerationEvent)constructorMethod.Invoke([contextMenu]);
        }
    }

    /// <summary>
    /// Represents a dispatchable base class for all<see cref="FieldDriveReceiver{T}"/> <see cref="ContextMenu"/> items generation events.
    /// </summary>
    [DispatchableBaseEvent, SubscribableBaseEvent]
    public abstract class FieldDriveReceiverMenuItemsGenerationEvent : DriveReceiverMenuItemsGenerationEvent
    {
        /// <summary>
        /// Gets the <see cref="IField"/> that the summoning <see cref="FieldDriveReceiver{T}"/> targets.
        /// </summary>
        public new IField Target => TargetFieldCore;

        /// <summary>
        /// Internal implementation of <see cref="Target"/>.
        /// </summary>
        protected abstract IField TargetFieldCore { get; }

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
        protected override sealed ISyncMember TargetCore => Target;

        /// <inheritdoc/>
        protected override sealed IField TargetFieldCore => Target;

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

    /// <summary>
    /// Represents a dispatchable base class for all <see cref="ReferenceDriveReceiver{T}"/> <see cref="ContextMenu"/> items generation events.
    /// </summary>
    [DispatchableBaseEvent, SubscribableBaseEvent]
    public abstract class ReferenceDriveReceiverMenuItemsGenerationEvent : DriveReceiverMenuItemsGenerationEvent
    {
        /// <summary>
        /// Gets the <see cref="ISyncRef"/> that the summoning <see cref="ReferenceDriveReceiver{T}"/> targets.
        /// </summary>
        public new ISyncRef Target => TargetSyncRefCore;

        /// <summary>
        /// Internal implementation of <see cref="Target"/>.
        /// </summary>
        protected abstract ISyncRef TargetSyncRefCore { get; }

        /// <inheritdoc/>
        protected ReferenceDriveReceiverMenuItemsGenerationEvent(ContextMenu contextMenu)
            : base(contextMenu)
        { }
    }

    /// <summary>
    /// Represents the event data for the concrete <see cref="ReferenceDriveReceiver{T}"/> <see cref="ContextMenu"/> items generation events.
    /// </summary>
    public sealed class ReferenceDriveReceiverMenuItemsGenerationEvent<T> : ReferenceDriveReceiverMenuItemsGenerationEvent
        where T : class, IWorldElement
    {
        /// <summary>
        /// Gets the <see cref="ReferenceDriveReceiver{T}"/> that the <see cref="ContextMenu">ContextMenu</see> is being summoned by.
        /// </summary>
        public new ReferenceDriveReceiver<T> Summoner { get; }

        /// <summary>
        /// Gets the <see cref="ISyncRef{T}"/> that the summoning <see cref="ReferenceDriveReceiver{T}"/> targets.
        /// </summary>
        public new ISyncRef<T> Target { get; }

        /// <inheritdoc/>
        protected override sealed Slot? SlotCore { get; }

        /// <inheritdoc/>
        protected override sealed IWorldElement SummonerCore => Summoner;

        /// <inheritdoc/>
        protected override sealed ISyncMember TargetCore => Target;

        /// <inheritdoc/>
        protected override sealed ISyncRef TargetSyncRefCore => Target;

        /// <inheritdoc/>
        protected override sealed User? UserCore { get; }

        /// <inheritdoc/>
        public ReferenceDriveReceiverMenuItemsGenerationEvent(ContextMenu contextMenu)
            : base(contextMenu)
        {
            Summoner = ContextMenu.CurrentSummoner as ReferenceDriveReceiver<T>
                ?? throw new ArgumentException($"Summoner was null or not of type {typeof(ReferenceDriveReceiver<T>).CompactDescription()} for Context Menu: {contextMenu.ParentHierarchyToString()}");

            Target = Summoner.Reference.Target;

            SlotCore = Target.FindNearestParent<Slot>();
            UserCore = Slot?.ActiveUser ?? Target.FindNearestParent<User>();
        }
    }
}