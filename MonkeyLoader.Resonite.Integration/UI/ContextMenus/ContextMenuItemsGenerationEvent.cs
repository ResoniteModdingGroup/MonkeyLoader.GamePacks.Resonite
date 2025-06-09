using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
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
        private static readonly Type[] _contextMenuConstructorParameters = [typeof(ContextMenu)];

        private static readonly Dictionary<Type, Func<ContextMenu, ContextMenuItemsGenerationEvent>> _contextMenuConstructorsBySummonerType = [];
        private static readonly Dictionary<Type, Func<ContextMenu, ContextMenuItemsGenerationEvent>> _defaultContextMenuConstructorsBySummonerType = [];

        private static readonly Type _objectType = typeof(object);
        private static readonly Type _rawEventType = typeof(ContextMenuItemsGenerationEvent<>);

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
        // Make sure this stays private protected
        private protected abstract IWorldElement SummonerInternal { get; }

        /// <summary>
        /// Creates a new <see cref="FrooxEngine.ContextMenu"/> items generation event with the given
        /// <paramref name="contextMenu"/> and its <see cref="Slot.ActiveUser">active</see> <see cref="User"/>.
        /// </summary>
        /// <inheritdoc cref="CreateFor(ContextMenu)"/>
        // Make sure this stays private protected
        private protected ContextMenuItemsGenerationEvent(ContextMenu contextMenu)
        {
            ContextMenu = contextMenu ?? throw new ArgumentNullException(nameof(contextMenu));
            SummoningUser = contextMenu.Slot.ActiveUser ?? throw new ArgumentException($"Active User was missing for Context Menu: {contextMenu.ParentHierarchyToString()}", nameof(contextMenu));
        }

        /// <summary>
        /// Adds the given <paramref name="constructorFunc"/> as a concrete derived event
        /// constructor for <see cref="ContextMenu.CurrentSummoner">summoners</see>
        /// of type <typeparamref name="TSummoner"/> or a more derived type.
        /// </summary>
        /// <typeparam name="TSummoner">The type of the <see cref="ContextMenu.CurrentSummoner">summoners</see> to use the <paramref name="constructorFunc"/> for.</typeparam>
        /// <inheritdoc cref="AddConcreteEvent(Type, Func{ContextMenu, ContextMenuItemsGenerationEvent}, bool)"/>
        public static bool AddConcreteEvent<TSummoner>(Func<ContextMenu, ContextMenuItemsGenerationEvent> constructorFunc, bool replace = false)
            => AddConcreteEvent(typeof(TSummoner), constructorFunc, replace);

        /// <summary>
        /// Adds the given <paramref name="constructorFunc"/> as a concrete derived event
        /// constructor for <see cref="ContextMenu.CurrentSummoner">summoners</see>
        /// of the <paramref name="summonerType"/> or a more derived type.
        /// </summary>
        /// <param name="summonerType">The type of the <see cref="ContextMenu.CurrentSummoner">summoners</see> to use the <paramref name="constructorFunc"/> for.</param>
        /// <param name="constructorFunc">A function that constructs the concrete <see cref="ContextMenuItemsGenerationEvent{T}"/>-derived instance with the given <see cref="FrooxEngine.ContextMenu"/>.</param>
        /// <param name="replace"><see langword="true"/> if the given <paramref name="constructorFunc"/> should replace one that's already present; otherwise, <see langword="false"/>.</param>
        /// <returns><see langword="true"/> if the given <paramref name="constructorFunc"/> is now the used one; otherwise, <see langword="false"/>.</returns>
        public static bool AddConcreteEvent(Type summonerType, Func<ContextMenu, ContextMenuItemsGenerationEvent> constructorFunc, bool replace = false)
        {
            if (_contextMenuConstructorsBySummonerType.ContainsKey(summonerType) && !replace)
                return false;

            _contextMenuConstructorsBySummonerType[summonerType] = constructorFunc;

            return true;
        }

        /// <inheritdoc cref="HasConcreteEvent{TSummoner}(out Func{ContextMenu, ContextMenuItemsGenerationEvent}?)"/>
        public static bool HasConcreteEvent<TSummoner>()
            => HasConcreteEvent(typeof(TSummoner));

        /// <summary>
        /// Determines whether there is a concrete derived event constructor for the type <typeparamref name="TSummoner"/>.
        /// </summary>
        /// <typeparam name="TSummoner">The type of the <see cref="ContextMenu.CurrentSummoner">summoners</see> to check.</typeparam>
        /// <inheritdoc cref="HasConcreteEvent(Type, out Func{ContextMenu, ContextMenuItemsGenerationEvent}?)"/>
        public static bool HasConcreteEvent<TSummoner>([NotNullWhen(true)] out Func<ContextMenu, ContextMenuItemsGenerationEvent>? constructorFunc)
            => HasConcreteEvent(typeof(TSummoner), out constructorFunc);

        /// <inheritdoc cref="HasConcreteEvent(Type, out Func{ContextMenu, ContextMenuItemsGenerationEvent}?)"/>
        public static bool HasConcreteEvent(Type summonerType)
            => _contextMenuConstructorsBySummonerType.ContainsKey(summonerType);

        /// <summary>
        /// Determines whether there is a concrete derived event constructor for the given <paramref name="summonerType"/>.
        /// </summary>
        /// <param name="summonerType">The type of the <see cref="ContextMenu.CurrentSummoner">summoners</see> to check.</param>
        /// <param name="constructorFunc">The function that constructs the concrete <see cref="ContextMenuItemsGenerationEvent{T}"/>-derived instance with the given <see cref="FrooxEngine.ContextMenu"/> if there is one; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if a constructor function for the <paramref name="summonerType"/> was found; otherwise, <see langword="false"/>.</returns>
        public static bool HasConcreteEvent(Type summonerType, [NotNullWhen(true)] out Func<ContextMenu, ContextMenuItemsGenerationEvent>? constructorFunc)
            => _contextMenuConstructorsBySummonerType.TryGetValue(summonerType, out constructorFunc);

        /// <summary>
        /// Removes the concrete derived event constructor for the type <typeparamref name="TSummoner"/>.
        /// </summary>
        /// <typeparam name="TSummoner">The type of the <see cref="ContextMenu.CurrentSummoner">summoners</see> to remove the constructor function for.</typeparam>
        /// <inheritdoc cref="RemoveConcreteEvent(Type)"/>
        public static bool RemoveConcreteEvent<TSummoner>()
            => RemoveConcreteEvent(typeof(TSummoner));

        /// <summary>
        /// Removes the concrete derived event constructor for the given <paramref name="summonerType"/>.
        /// </summary>
        /// <param name="summonerType">The type of the <see cref="ContextMenu.CurrentSummoner">summoners</see> to remove the constructor function for.</param>
        /// <returns><see langword="true"/> if a constructor function for the <paramref name="summonerType"/> was found and removed; otherwise, <see langword="false"/>.</returns>
        public static bool RemoveConcreteEvent(Type summonerType)
            => _contextMenuConstructorsBySummonerType.Remove(summonerType);

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
        {
            ContextMenuInjector.IsHandlerOpeningContextMenu = true;
            var success = await ContextMenu.OpenMenu(Summoner, pointer, options);
            ContextMenuInjector.IsHandlerOpeningContextMenu = false;

            return success ? ContextMenu : null;
        }

        /// <summary>
        /// Opens the <see cref="SummoningUser">SummoningUser</see>'s <see cref="FrooxEngine.ContextMenu"/>
        /// at the position determined by the <paramref name="pointer"/> using the given <paramref name="options"/>
        /// if it's not open already.<br/>
        /// If it is open already, it will be closed instead.
        /// </summary>
        /// <returns>
        /// The <see cref="SummoningUser">SummoningUser</see>'s newly opened <see cref="FrooxEngine.ContextMenu"/>,
        /// or <c>null</c> if opening failed or it was already open before.
        /// </returns>
        /// <inheritdoc cref="OpenContextMenuAsync"/>
        public async Task<ContextMenu?> ToggleContextMenuAsync(Slot pointer, ContextMenuOptions options = default)
        {
            if (!IsContextMenuOpen || ContextMenu.CurrentSummoner != Summoner)
                return await OpenContextMenuAsync(pointer, options);

            CloseContextMenu();
            return null;
        }

        /// <summary>
        /// Creates a new <see cref="FrooxEngine.ContextMenu"/> items generation event with the given
        /// <paramref name="summoningUser"/> and its <see cref="ContextMenuExtensions.GetUserContextMenu">context menu</see>.
        /// </summary>
        /// <remarks>
        /// This method dynamically uses the best registered concrete derived event type for the
        /// <see cref="ContextMenu.CurrentSummoner"/>, falling back to the concrete <see cref="ContextMenuItemsGenerationEvent{T}"/>.
        /// </remarks>
        /// <param name="summoningUser">The <see cref="User"/> that the <see cref="FrooxEngine.ContextMenu"/> is being summoned for.</param>
        /// <exception cref="ArgumentNullException">When the <paramref name="summoningUser"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">When the <paramref name="summoningUser"/>'s <see cref="ContextMenuExtensions.GetUserContextMenu">context menu</see> is <see langword="null"/>.</exception>
        internal static ContextMenuItemsGenerationEvent CreateFor(User summoningUser)
        {
            if (summoningUser is null)
                throw new ArgumentNullException(nameof(summoningUser));

            var contextMenu = summoningUser.GetUserContextMenu()
                ?? throw new ArgumentException($"Context Menu was missing for User: {summoningUser}!", nameof(summoningUser));

            return CreateFor(contextMenu);
        }

        /// <summary>
        /// Creates a new <see cref="FrooxEngine.ContextMenu"/> items generation event for the given
        /// <paramref name="contextMenu"/> and its <see cref="Slot.ActiveUser">active</see> <see cref="User"/>.
        /// </summary>
        /// <remarks>
        /// This method dynamically uses the best registered concrete derived event type
        /// for the <c><paramref name="contextMenu"/>.<see cref="ContextMenu.CurrentSummoner">CurrentSummoner</see></c>,
        /// falling back to the concrete <see cref="ContextMenuItemsGenerationEvent{T}"/>.
        /// </remarks>
        /// <param name="contextMenu">The <see cref="FrooxEngine.ContextMenu"/> being summoned.</param>
        /// <exception cref="ArgumentNullException">When the <paramref name="contextMenu"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">When the <paramref name="contextMenu"/>'s <see cref="Slot.ActiveUser">active</see> <see cref="User"/> is <see langword="null"/>.</exception>
        internal static ContextMenuItemsGenerationEvent CreateFor(ContextMenu contextMenu)
        {
            if (contextMenu is null)
                throw new ArgumentNullException(nameof(contextMenu));

            if (contextMenu.CurrentSummoner is null)
                throw new ArgumentException($"Summoner was missing for Context Menu: {contextMenu.ParentHierarchyToString()}");

            var summonerType = contextMenu.CurrentSummoner.GetType();
            var contextMenuConstructor = GetDefaultContextMenuConstructor(summonerType);

            while (summonerType != _objectType)
            {
                if (_contextMenuConstructorsBySummonerType.TryGetValue(summonerType, out var customContextMenuConstructor))
                {
                    contextMenuConstructor = customContextMenuConstructor;
                    break;
                }

                summonerType = summonerType.BaseType;
            }

            return contextMenuConstructor(contextMenu);
        }

        private static Func<ContextMenu, ContextMenuItemsGenerationEvent> GetDefaultContextMenuConstructor(Type summonerType)
        {
            if (!_defaultContextMenuConstructorsBySummonerType.TryGetValue(summonerType, out var contextMenuConstructor))
            {
                var eventType = _rawEventType.MakeGenericType(summonerType);
                var constructorMethod = AccessTools.DeclaredConstructor(eventType, _contextMenuConstructorParameters);
                contextMenuConstructor = contextMenu => (ContextMenuItemsGenerationEvent)constructorMethod.Invoke([contextMenu]);
            }

            return contextMenuConstructor;
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
        private protected override sealed IWorldElement SummonerInternal => Summoner;

        /// <inheritdoc/>
        public ContextMenuItemsGenerationEvent(ContextMenu contextMenu) : base(contextMenu)
        {
            Summoner = ContextMenu.CurrentSummoner as T
                ?? throw new ArgumentException($"Summoner was null or not of type {typeof(T).CompactDescription()} for Context Menu: {contextMenu.ParentHierarchyToString()}");
        }
    }
}