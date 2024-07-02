using MonkeyLoader.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.Events
{
    /// <summary>
    /// Abstract base class for all sorts of cancelable events that focus on adding a (sorted) list of unique items.
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    public abstract class CancelableSortedItemsEvent<T> : CancelableSyncEvent
    {
        /// <summary>
        /// The items that have been added to the event.
        /// </summary>
        protected readonly Dictionary<T, int> sortableItems;

        /// <summary>
        /// Gets the unique items as sorted by their sort order values.
        /// </summary>
        public virtual IEnumerable<T> Items => sortableItems.OrderBy(entry => entry.Value).Select(entry => entry.Key);

        /// <summary>
        /// Creates a new instance with no items.
        /// </summary>
        protected CancelableSortedItemsEvent()
        {
            sortableItems = new();
        }

        /// <summary>
        /// Creates a new instance with the given items,
        /// setting their sort order values to <c>0</c>.
        /// </summary>
        /// <param name="items">The items to start with.</param>
        protected CancelableSortedItemsEvent(IEnumerable<T> items) : this(items, item => 0)
        { }

        /// <summary>
        /// Creates a new instance with the given items, using the
        /// <paramref name="getSortOrder"/> function to determine their sort order values.
        /// </summary>
        /// <param name="items">The items to start with.</param>
        /// <param name="getSortOrder">The function to map items to a sort order value.</param>
        protected CancelableSortedItemsEvent(IEnumerable<T> items, Func<T, int> getSortOrder)
        {
            sortableItems = items.ToDictionary(item => item, getSortOrder);
        }

        /// <summary>
        /// Adds the given item with the given sort order.<br/>
        /// If it's already present, the sort order is only changed to the given one,
        /// when <paramref name="updateOrder"/> is <c>true</c>.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="sortOrder">The sort order value for the item.</param>
        /// <param name="updateOrder">Whether to update the sort order value if the item is already present.</param>
        /// <returns><c>true</c> if the item is now present as defined; otherwise, <c>false</c>.</returns>
        public bool AddItem(T item, int sortOrder = 0, bool updateOrder = false)
        {
            if (sortableItems.ContainsKey(item))
            {
                if (updateOrder)
                    sortableItems[item] = sortOrder;

                return updateOrder;
            }

            sortableItems.Add(item, sortOrder);
            return true;
        }

        /// <summary>
        /// Checks whether the given item is already present.
        /// </summary>
        /// <param name="item">The item to check for.</param>
        /// <param name="sortOrder">The sort order value of the item, if it is present.</param>
        /// <returns><c>true</c> if the item is present; otherwise, <c>false</c>.</returns>
        public bool HasItem(T item, [NotNullWhen(true)] out int? sortOrder)
        {
            if (sortableItems.TryGetValue(item, out var order))
            {
                sortOrder = order;
                return true;
            }

            sortOrder = null;
            return false;
        }

        /// <summary>
        /// Removes the given item.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns><c>true</c> if the item was found and removed; otherwise <c>false</c>.</returns>
        public bool RemoveItem(T item) => sortableItems.Remove(item);
    }
}