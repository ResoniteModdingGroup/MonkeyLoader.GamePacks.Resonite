using Elements.Core;
using FrooxEngine;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

namespace MonkeyLoader.Resonite.UI
{
    /// <summary>
    /// Contains extension methods to add pagination to and check for pagination on <see cref="ContextMenu"/>s.
    /// </summary>
    public static class ContextMenuPaginationExtensions
    {
        private static readonly ConditionalWeakTable<ContextMenu, PaginationInfo> _paginationInfosByContextMenu = [];

        /// <inheritdoc cref="AddPagination(ContextMenu, int, int, out ContextMenuItem, out ContextMenuItem)"/>
        public static void AddPagination(this ContextMenu contextMenu, int maxItems, int currentPage = 0)
            => contextMenu.AddPagination(maxItems, currentPage, out _, out _);

        /// <summary>
        /// Adds pagination with the given configuration to this context menu.<br/>
        /// If there already is pagination, it will be replaced.
        /// </summary>
        /// <remarks>
        /// The maximum number of non-pagination items to show per page does not include the paging buttons.<br/>
        /// This means that up to <c><paramref name="maxItems"/> + 2</c> non-pagination items will be displayed without pagination.
        /// </remarks>
        /// <param name="back">The newly created context menu item to move to the previous page.</param>
        /// <param name="forward">The newly created context menu item to move to the next page.</param>
        /// <returns/>
        /// <exception cref="ArgumentNullException">When this <paramref name="contextMenu"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">When <paramref name="maxItems"/> is not greater than zero.</exception>
        /// <exception cref="InvalidOperationException">When adding the pagination failed for other reasons.</exception>
        /// <inheritdoc cref="TryAddPagination(ContextMenu, int, int, out ContextMenuItem?, out ContextMenuItem?)"/>
        public static void AddPagination(this ContextMenu contextMenu, int maxItems, int currentPage,
            out ContextMenuItem back, out ContextMenuItem forward)
        {
            ArgumentNullException.ThrowIfNull(contextMenu);
            ArgumentOutOfRangeException.ThrowIfLessThan(maxItems, 0);

            if (!contextMenu.TryAddPagination(ref maxItems, ref currentPage, true, out back!, out forward!))
                throw new InvalidOperationException("Failed to add pagination!");
        }

        /// <summary>
        /// Tries to add pagination with the given configuration to this context menu.<br/>
        /// If there already is pagination, nothing happens.
        /// </summary>
        /// <inheritdoc cref="TryAddPagination(ContextMenu, int, int, out ContextMenuItem?, out ContextMenuItem?)"/>
        public static bool TryAddPagination(this ContextMenu contextMenu, int maxItems, int currentPage = 0)
            => contextMenu.TryAddPagination(ref maxItems, ref currentPage, false, out _, out _);

        /// <summary>
        /// Tries to add pagination with the given configuration to this context menu.<br/>
        /// If there already is pagination, the <see langword="out"/> parameters will
        /// be set to its <see cref="ContextMenuItem">menu items</see>.
        /// </summary>
        /// <param name="maxItems">The maximum number of non-pagination items to show per page. Must be greater than zero.</param>
        /// <param name="currentPage">The page that should be shown from the start. Can be any integer value, even negative.</param>
        /// <inheritdoc cref="TryAddPagination(ContextMenu, ref int, ref int, bool, out ContextMenuItem?, out ContextMenuItem?)"/>
        public static bool TryAddPagination(this ContextMenu contextMenu, int maxItems, int currentPage,
            [NotNullWhen(true)] out ContextMenuItem? back, [NotNullWhen(true)] out ContextMenuItem? forward)
            => contextMenu.TryAddPagination(ref maxItems, ref currentPage, false, out back, out forward);

        /// <summary>
        /// Tries to add pagination with the given configuration to this context menu.<br/>
        /// If <paramref name="forceNew"/> is not <see langword="true"/> and there already is pagination,
        /// the <see langword="ref"/> and <see langword="out"/> parameters will be set to its
        /// configuration and <see cref="ContextMenuItem">menu items</see>.
        /// </summary>
        /// <remarks><para>
        /// This method will not throw exceptions, even if the <paramref name="contextMenu"/> is <see langword="null"/>,
        /// or <paramref name="maxItems"/> is set to an invalid number.<br/>
        /// Any <see langword="ref"/> and <see langword="out"/> parameters only have
        /// valid or updated values if the return value is <see langword="true"/>.
        /// </para><para>
        /// The maximum number of non-pagination items to show per page does not include the paging buttons.<br/>
        /// This means that up to <c><paramref name="maxItems"/> + 2</c> non-pagination items will be displayed without pagination.
        /// </para></remarks>
        /// <param name="contextMenu">The context menu to add pagination to.</param>
        /// <param name="maxItems">
        /// The maximum number of non-pagination items to show per page. Must be greater than zero.<br/>
        /// Will be set to the already configured value if pagination was
        /// already set up and <paramref name="forceNew"/> is <see langword="false"/>.
        /// </param>
        /// <param name="currentPage">
        /// The page that should be shown from the start. Can be any integer value, even negative.<br/>
        /// Will be set to the clamped value after set up or the current value if pagination was
        /// already set up and <paramref name="forceNew"/> is <see langword="false"/>.<br/>
        /// This can be <c>-1</c> if there's only up to <c><paramref name="maxItems"/> + 2</c> non-pagination items, so pagination isn't necessary.
        /// </param>
        /// <param name="forceNew">Whether to re-add pagination even if it is already present.</param>
        /// <param name="back">
        /// The newly created or already present context menu item to move to the
        /// previous page if the return value is <see langword="true"/>; otherwise, <see langword="null"/>.
        /// </param>
        /// <param name="forward">
        /// The newly created or already present context menu item to move to the
        /// next page if the return value is <see langword="true"/>; otherwise, <see langword="null"/>.
        /// </param>
        /// <returns><see langword="true"/> if this <see cref="ContextMenu"/> is now paginated; otherwise, <see langword="false"/>.</returns>
        public static bool TryAddPagination(this ContextMenu contextMenu, ref int maxItems, ref int currentPage, bool forceNew,
            [NotNullWhen(true)] out ContextMenuItem? back, [NotNullWhen(true)] out ContextMenuItem? forward)
        {
            if (contextMenu.HasPagination(out var paginationInfo))
            {
                if (!forceNew)
                {
                    maxItems = paginationInfo.MaxItems;
                    currentPage = paginationInfo.CurrentPage;

                    back = paginationInfo.Back;
                    forward = paginationInfo.Forward;

                    return true;
                }

                paginationInfo.Destroy();
            }

            back = null;
            forward = null;

            if (contextMenu is null || maxItems <= 0)
                return false;

            if (contextMenu.LocalUser != contextMenu.Owner.Target || contextMenu._ui is null)
                return false;

            paginationInfo = new(contextMenu, maxItems, currentPage);

            back = paginationInfo.Back;
            forward = paginationInfo.Forward;
            currentPage = paginationInfo.CurrentPage;

            return true;
        }

        /// <summary>
        /// Checks if this context menu already has pagination.
        /// </summary>
        /// <inheritdoc cref="HasPagination(ContextMenu, out int, out int, out ContextMenuItem?, out ContextMenuItem?)"/>
        public static bool HasPagination(this ContextMenu contextMenu)
            => contextMenu.HasPagination(out _);

        /// <summary>
        /// Checks if this context menu already has pagination and returns its configuration if so.
        /// </summary>
        /// <inheritdoc cref="HasPagination(ContextMenu, out int, out int, out ContextMenuItem?, out ContextMenuItem?)"/>
        public static bool HasPagination(this ContextMenu contextMenu, out int maxItems, out int currentPage)
            => contextMenu.HasPagination(out maxItems, out currentPage, out _, out _);

        /// <summary>
        /// Checks if this context menu already has pagination and returns its
        /// configuration and <see cref="ContextMenuItem">menu items</see> if so.
        /// </summary>
        /// <param name="contextMenu">The context menu to check for pagination.</param>
        /// <param name="maxItems">The maximum number of non-pagination items shown per page if there is pagination; otherwise, <see cref="int.MinValue"/>.</param>
        /// <param name="currentPage">
        /// The currently shown page if there is pagination; otherwise, <see cref="int.MinValue"/>.<br/>
        /// This can be <c>-1</c> if there's only up to <c><paramref name="maxItems"/> + 2</c> non-pagination items, so pagination isn't necessary.
        /// </param>
        /// <param name="back">The context menu item to move to the previous page if there is pagination; otherwise, <see langword="null"/>.</param>
        /// <param name="forward">The context menu item to move to the next page if there is pagination; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if there is pagination on this context menu; otherwise, <see langword="false"/>.</returns>
        public static bool HasPagination(this ContextMenu contextMenu, out int maxItems, out int currentPage,
            [NotNullWhen(true)] out ContextMenuItem? back, [NotNullWhen(true)] out ContextMenuItem? forward)
        {
            if (!contextMenu.HasPagination(out var paginationInfo))
            {
                maxItems = int.MinValue;
                currentPage = int.MinValue;
                back = null;
                forward = null;

                return false;
            }

            maxItems = paginationInfo.MaxItems;
            currentPage = paginationInfo.CurrentPage;
            back = paginationInfo.Back;
            forward = paginationInfo.Forward;

            return true;
        }

        private static bool HasPagination(this ContextMenu contextMenu, [NotNullWhen(true)] out PaginationInfo? paginationInfo)
        {
            if (!_paginationInfosByContextMenu.TryGetValue(contextMenu, out paginationInfo))
                return false;

            if (paginationInfo.IsInvalid)
                paginationInfo = null;

            return paginationInfo is not null;
        }

        private sealed class PaginationInfo
        {
            public ContextMenuItem Back { get; }
            public ContextMenu ContextMenu { get; }
            public int CurrentPage { get; private set; }
            public ContextMenuItem Forward { get; }
            public int MaxItems { get; }

            public bool IsInvalid => ContextMenu.FilterWorldElement() is null
                || Back.FilterWorldElement() is null || Forward.FilterWorldElement() is null;

            public PaginationInfo(ContextMenu contextMenu, int maxItems, int currentPage)
            {
                ContextMenu = contextMenu;
                MaxItems = maxItems;
                CurrentPage = currentPage;

                Back = contextMenu.AddItem("Back", OfficialAssets.Common.Icons.Left_Chevron, RadiantUI_Constants.Neutrals.LIGHT);
                Back.Slot.OrderOffset = long.MinValue;
                Back.Button.LocalPressed += PreviousPage;
                Back.Destroyed += Destroy;

                Forward = contextMenu.AddItem("Forward", OfficialAssets.Common.Icons.Right_Chevron, RadiantUI_Constants.Neutrals.LIGHT);
                Forward.Slot.OrderOffset = long.MaxValue;
                Forward.Button.LocalPressed += NextPage;

                var itemsRoot = ContextMenu._itemsRoot.Target;
                itemsRoot.ChildAdded += SlotChildrenChanged;
                itemsRoot.ChildRemoved += SlotChildrenChanged;
                itemsRoot.ChildrenOrderInvalidated += SlotChildrenChanged;

                UpdatePagination();

                _paginationInfosByContextMenu.AddOrUpdate(contextMenu, this);
            }

            public void Destroy(IDestroyable? destroyable = null)
            {
                var itemsRoot = ContextMenu._itemsRoot.Target;

                itemsRoot.ChildAdded -= SlotChildrenChanged;
                itemsRoot.ChildRemoved -= SlotChildrenChanged;
                itemsRoot.ChildrenOrderInvalidated -= SlotChildrenChanged;

                _paginationInfosByContextMenu.Remove(ContextMenu);
            }

            private void NextPage(IButton button, ButtonEventData args)
            {
                ++CurrentPage;
                UpdatePagination();
            }

            private void PreviousPage(IButton button, ButtonEventData args)
            {
                --CurrentPage;
                UpdatePagination();
            }

            private void SlotChildrenChanged(Slot slot, Slot child)
                => UpdatePagination();

            private void SlotChildrenChanged(Slot slot)
                => UpdatePagination();

            private void UpdatePagination()
            {
                if (IsInvalid)
                    return;

                // Do not count the paging buttons
                var itemsRoot = ContextMenu._itemsRoot.Target;
                var items = itemsRoot.ChildrenCount - 2;

                if (items <= MaxItems)
                {
                    CurrentPage = -1;
                    Back.Slot.ActiveSelf = false;
                    Forward.Slot.ActiveSelf = false;
                    return;
                }

                // Shift max page down by one item to prevent empty page at the end
                var maxPage = (items - 1) / MaxItems;
                CurrentPage = MathX.Max(0, MathX.Min(maxPage, CurrentPage));

                Back.Slot.ActiveSelf = true;
                Back.Button.Enabled = CurrentPage > 0;

                Forward.Slot.ActiveSelf = true;
                Forward.Button.Enabled = CurrentPage < maxPage;

                var pageStart = CurrentPage * MaxItems;
                var pageEnd = (CurrentPage + 1) * MaxItems;

                for (var i = 0; i < items; ++i)
                    itemsRoot[i + 1].ActiveSelf = i >= pageStart && i < pageEnd;
            }
        }
    }
}

#pragma warning restore CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)