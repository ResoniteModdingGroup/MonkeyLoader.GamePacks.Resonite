using FrooxEngine;
using System.Linq;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Contains extension methods to add <see cref="DestroyOnUserLeave"/> components targeting
    /// the <see cref="World.LocalUser">local user</see> to mod additions that rely on the local user being there to work.
    /// </summary>
    public static class DestroyOnUserLeaveExtensions
    {
        /// <summary>
        /// Ensures that the given component's <see cref="IComponent.Slot">slot</see>
        /// will be destroyed when the local user leaves.
        /// </summary>
        /// <param name="component">The component who's slot should be destroyed.</param>
        /// <returns>The <see cref="DestroyOnUserLeave"/> component handling the destruction.</returns>
        public static DestroyOnUserLeave DestroyWhenLocalUserLeaves(this IComponent component)
            => component.Slot.DestroyWhenLocalUserLeaves();

        /// <summary>
        /// Ensures that the given <see cref="Slot"/> will be destroyed when the local user leaves.
        /// </summary>
        /// <param name="slot">The slot that should be destroyed.</param>
        /// <returns>The <see cref="DestroyOnUserLeave"/> component handling the destruction.</returns>
        public static DestroyOnUserLeave DestroyWhenLocalUserLeaves(this Slot slot)
        {
            if (slot.GetComponents<DestroyOnUserLeave>().FirstOrDefault(destroy => destroy.TargetUser.Target == slot.LocalUser) is DestroyOnUserLeave destroy)
                return destroy;

            return slot.DestroyWhenUserLeaves(slot.LocalUser);
        }
    }
}