using FrooxEngine;
using MonkeyLoader.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.UI.ContextMenus
{
    /// <summary>
    /// Represents a dispatchable base class for all events that generate a <see cref="FrooxEngine.ContextMenu"/>.
    /// </summary>
    /// <remarks>
    /// Use <see cref="ContextMenuItemsGenerationEvent{T}"/> for derived classes instead.<br/>
    /// This class only exists as a subscribable catch-all event.
    /// </remarks>
    [DispatchableBaseEvent, SubscribableBaseEvent]
    public abstract class ContextMenuItemsGenerationEvent : AsyncEvent
    {
        private static readonly AnyMap _concreteEventConstructors = new();

        /// <summary>
        /// Gets the <see cref="FrooxEngine.ContextMenu"/> being summoned.
        /// </summary>
        public ContextMenu ContextMenu { get; }

        /// <summary>
        /// Gets whether the <see cref="ContextMenu">ContextMenu</see> is open.
        /// </summary>
        /// <value><see langword="true"/> if the <see cref="ContextMenu">ContextMenu</see> is open; otherwise, <see langword="false"/>.</value>
        public bool IsContextMenuOpen => ContextMenu.MenuState != ContextMenu.State.Closed;

        /// <summary>
        /// Gets the <see cref="IWorldElement"/> that the <see cref="ContextMenu">ContextMenu</see> is being summoned by.
        /// </summary>
        public IWorldElement Summoner => SummonerInternal;

        /// <summary>
        /// Gets the <see cref="User"/> that the <see cref="FrooxEngine.ContextMenu"/> is being summoned for.
        /// </summary>
        public User SummoningUser { get; }

        /// <summary>
        /// Internal implementation for <see cref="Summoner"/>.
        /// </summary>
        protected abstract IWorldElement SummonerInternal { get; }

        /// <summary>
        /// Creates a new <see cref="FrooxEngine.ContextMenu"/> items generation event with the given
        /// <paramref name="summoningUser"/> and its <see cref="ContextMenuExtensions.GetUserContextMenu">context menu</see>.
        /// </summary>
        /// <param name="summoningUser">The <see cref="User"/> that the <see cref="FrooxEngine.ContextMenu"/> is being summoned for.</param>
        /// <exception cref="ArgumentNullException">When the <paramref name="summoningUser"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">When the <paramref name="summoningUser"/>'s <see cref="ContextMenuExtensions.GetUserContextMenu">context menu</see> is <see langword="null"/>.</exception>
        protected ContextMenuItemsGenerationEvent(User summoningUser)
        {
            SummoningUser = summoningUser ?? throw new ArgumentNullException(nameof(summoningUser));
            ContextMenu = summoningUser.GetUserContextMenu() ?? throw new ArgumentException($"Context Menu was missing for User: {summoningUser}!", nameof(summoningUser));
        }

        /// <summary>
        /// Creates a new <see cref="FrooxEngine.ContextMenu"/> items generation event with the given
        /// <paramref name="contextMenu"/> and its <see cref="Slot.ActiveUser">active</see> <see cref="User"/>.
        /// </summary>
        /// <param name="contextMenu">The <see cref="FrooxEngine.ContextMenu"/> being summoned.</param>
        /// <exception cref="ArgumentNullException">When the <paramref name="contextMenu"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">When the <paramref name="contextMenu"/>'s <see cref="Slot.ActiveUser">active</see> <see cref="User"/> is <see langword="null"/>.</exception>
        protected ContextMenuItemsGenerationEvent(ContextMenu contextMenu)
        {
            ContextMenu = contextMenu ?? throw new ArgumentNullException(nameof(contextMenu));
            SummoningUser = contextMenu.Slot.ActiveUser ?? throw new ArgumentException($"Active User was missing for Context Menu: {contextMenu.ParentHierarchyToString()}", nameof(contextMenu));
        }

        /// <summary>
        /// Closes the <see cref="SummoningUser">SummoningUser</see>'s <see cref="FrooxEngine.ContextMenu"/>.
        /// </summary>
        public void CloseContextMenu()
            => ContextMenu.Close();

        /// <summary>
        /// Ensures that the <see cref="SummoningUser">SummoningUser</see>'s <see cref="FrooxEngine.ContextMenu"/>
        /// is open at the position determined by the <paramref name="pointer"/> using the given <paramref name="options"/>.
        /// </summary>
        /// <remarks>
        /// If <paramref name="options"/>.<see cref="ContextMenuOptions.keepPosition">keepPosition</see>
        /// is <c>true</c> the last position will be kept instead.
        /// </remarks>
        /// <param name="pointer">
        /// The slot that the <see cref="FrooxEngine.ContextMenu"/> will be centered at.<br/>
        /// This is typically the slot of the <see cref="TouchSource"/> that triggered the opening.
        /// </param>
        /// <param name="options">The additional options for opening the menu.</param>
        /// <returns>The opened <see cref="FrooxEngine.ContextMenu"/>, or <see langword="null"/> if it failed to open.</returns>
        public async Task<ContextMenu?> OpenContextMenuAsync(Slot pointer, ContextMenuOptions options = default)
            => await ContextMenu.OpenMenu(Summoner, pointer, options) ? ContextMenu : null;

        /// <summary>
        /// Opens the <see cref="SummoningUser">SummoningUser</see>'s <see cref="FrooxEngine.ContextMenu"/>
        /// at the position determined by the <paramref name="pointer"/> using the given <paramref name="options"/>
        /// if it's not open already.<br/>
        /// If it is open already, it will be closed instead.
        /// </summary>
        /// <returns>
        /// The <see cref="SummoningUser">SummoningUser</see>'s newly opened <see cref="FrooxEngine.ContextMenu"/>,
        /// or <c>null</c> if it was already open before.
        /// </returns>
        /// <inheritdoc cref="OpenContextMenuAsync"/>
        public async Task<ContextMenu?> ToggleContextMenuAsync(Slot pointer, ContextMenuOptions options = default)
        {
            if (!IsContextMenuOpen || ContextMenu.CurrentSummoner != Summoner)
                return await OpenContextMenuAsync(pointer, options);

            CloseContextMenu();
            return null;
        }

        internal static ContextMenuItemsGenerationEvent CreateFor(ContextMenu contextMenu)
        {
            // Find best instanciation method for the contextMenu summoner's type,
            // i.e. walk up its types until there's one that takes it
            // Have basic ISyncMember taking one as the base that falls back to constructing the plain generic event.
            // Store Dictionary<Type (of Summoner), Func<Summoner, ContextMenuItemsGenerationEvent>>
            // This could be an AnyMap storing the funcs - keep cache of the constructed Func types?

            var type = typeof(ContextMenuItemsGenerationEvent<>).MakeGenericType(contextMenu.CurrentSummoner.GetType());
            var instance = Activator.CreateInstance(type, contextMenu.Slot.ActiveUser, contextMenu.CurrentSummoner);
            return (ContextMenuItemsGenerationEvent)instance;
        }
    }

    /// <summary>
    /// Represents a generic (base) class for all events that generate a <see cref="ContextMenu"/>.
    /// </summary>
    /// <remarks>
    /// These events are always additive to the options added by the vanilla implementation.
    /// </remarks>
    /// <typeparam name="T">The type of the <see cref="Summoner">Summoner</see>.</typeparam>
    public class ContextMenuItemsGenerationEvent<T> : ContextMenuItemsGenerationEvent
        where T : class, IWorldElement
    {
        /// <summary>
        /// Gets the <typeparamref name="T"/> that the
        /// <see cref="ContextMenuItemsGenerationEvent.ContextMenu">ContextMenu</see>
        /// is being summoned by.
        /// </summary>
        public new T Summoner { get; }

        /// <inheritdoc/>
        protected override sealed IWorldElement SummonerInternal => Summoner;

        /// <inheritdoc/>
        public ContextMenuItemsGenerationEvent(User summoningUser) : base(summoningUser)
        {
            Summoner = ContextMenu.CurrentSummoner as T ?? throw new ArgumentException($"Summoner was null or not of type {typeof(T).CompactDescription()} for User: {summoningUser}!");
        }

        /// <inheritdoc/>
        public ContextMenuItemsGenerationEvent(ContextMenu contextMenu) : base(contextMenu)
        {
            Summoner = ContextMenu.CurrentSummoner as T ?? throw new ArgumentException($"Summoner was null or not of type {typeof(T).CompactDescription()} for Context Menu: {contextMenu.ParentHierarchyToString()}");
        }
    }
}