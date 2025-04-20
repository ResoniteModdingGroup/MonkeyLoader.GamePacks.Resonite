using FrooxEngine;
using MonkeyLoader.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.Events
{
    /// <summary>
    /// Represents a dispatchable base class for all events that generate a <see cref="FrooxEngine.ContextMenu"/>.
    /// </summary>
    /// <remarks>
    /// Use <see cref="ContextMenuItemsGenerationEvent{T}"/> for derived classes instead.<br/>
    /// This class only exists as a subscribable catch-all event.
    /// </remarks>
    [DispatchableBaseEvent]
    public abstract class ContextMenuItemsGenerationEvent : AsyncEvent
    {
        /// <summary>
        /// Gets the <see cref="SummoningUser">SummoningUser</see>'s <see cref="FrooxEngine.ContextMenu"/>.
        /// </summary>
        public ContextMenu ContextMenu { get; }

        /// <summary>
        /// Gets whether the <see cref="SummoningUser">SummoningUser</see>'s <see cref="FrooxEngine.ContextMenu"/> is open.
        /// </summary>
        /// <value><c>true</c> if the <see cref="FrooxEngine.ContextMenu"/> is open; otherwise, <c>false</c>.</value>
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
        private protected abstract IWorldElement SummonerInternal { get; }

        /// <summary>
        /// Creates a new <see cref="FrooxEngine.ContextMenu"/> items generation event with the given <paramref name="summoningUser"/>.
        /// </summary>
        /// <param name="summoningUser">The <see cref="User"/> that the <see cref="FrooxEngine.ContextMenu"/> is being summoned for.</param>
        /// <exception cref="ArgumentNullException">
        /// If the <paramref name="summoningUser"/> or its <see cref="ContextMenuExtensions.GetUserContextMenu">context menu</see> is <c>null</c>.
        /// </exception>
        private protected ContextMenuItemsGenerationEvent(User summoningUser)
        {
            SummoningUser = summoningUser ?? throw new ArgumentNullException(nameof(summoningUser));
            ContextMenu = summoningUser.GetUserContextMenu() ?? throw new ArgumentNullException(nameof(ContextMenu), $"Context Menu was null for user {summoningUser}!");
        }

        /// <summary>
        /// Closes the <see cref="SummoningUser">SummoningUser</see>'s <see cref="FrooxEngine.ContextMenu"/>.
        /// </summary>
        public void CloseContextMenu()
            => SummoningUser.CloseContextMenu(Summoner);

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
        /// <returns>The <see cref="SummoningUser">SummoningUser</see>'s opened <see cref="FrooxEngine.ContextMenu"/>.</returns>
        public async Task<ContextMenu> OpenContextMenuAsync(Slot pointer, ContextMenuOptions options = default)
            => await SummoningUser.OpenContextMenu(Summoner, pointer, options);

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
            => await SummoningUser.ToggleContextMenu(Summoner, pointer, options);
    }

    /// <summary>
    /// Represents a base class for all events that generate a <see cref="ContextMenu"/>.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="Summoner">Summoner</see>.</typeparam>
    public abstract class ContextMenuItemsGenerationEvent<T> : ContextMenuItemsGenerationEvent
        where T : class, IWorldElement
    {
        /// <summary>
        /// Gets the <typeparamref name="T"/> that the
        /// <see cref="ContextMenuItemsGenerationEvent.ContextMenu">ContextMenu</see>
        /// is being summoned by.
        /// </summary>
        public new T Summoner { get; }

        /// <inheritdoc/>
        private protected override sealed IWorldElement SummonerInternal => Summoner;

        /// <summary>
        /// Creates a new <see cref="ContextMenu"/> items generation event with the given <paramref name="summoningUser"/> and <paramref name="summoner"/>.
        /// </summary>
        /// <param name="summoningUser">The <see cref="User"/> that the <see cref="FrooxEngine.ContextMenu"/> is being summoned for.</param>
        /// <param name="summoner">
        /// The <typeparamref name="T"/> that the
        /// <see cref="ContextMenuItemsGenerationEvent.ContextMenu">ContextMenu</see>
        /// is being summoned by.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If the <paramref name="summoner"/>, or the <paramref name="summoningUser"/> or
        /// its <see cref="ContextMenuExtensions.GetUserContextMenu">context menu</see> is <c>null</c>.
        /// </exception>
        protected ContextMenuItemsGenerationEvent(User summoningUser, T summoner) : base(summoningUser)
        {
            Summoner = summoner ?? throw new ArgumentNullException(nameof(summoner));
        }
    }
}