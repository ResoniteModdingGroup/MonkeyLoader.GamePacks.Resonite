using FrooxEngine;
using System.Linq;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Contains an extension method to add <see cref="DestroyOnUserLeave"/> components targeting
    /// the <see cref="World.LocalUser">local user</see> to mod additions that rely on the local user being there to work.
    /// </summary>
    public static class DestroyOnUserLeaveExtensions
    {
        /// <summary>
        /// Ensures that the given <see cref="Slot"/> will be destroyed when the local user leaves,
        /// and won't be saved either (marking it non-persistant).
        /// </summary>
        /// <param name="slot">The slot that should be non-persistant and destroyed when the local user leaves.</param>
        /// <returns>The <see cref="DestroyOnUserLeave"/> component handling the destruction.</returns>
        public static DestroyOnUserLeave DestroyWhenLocalUserLeaves(this Slot slot)
        {
            slot.PersistentSelf = false;

            if (slot.GetComponents<DestroyOnUserLeave>().FirstOrDefault(destroy => destroy.TargetUser.Target == slot.LocalUser) is DestroyOnUserLeave destroy)
                return destroy;

            return slot.DestroyWhenUserLeaves(slot.LocalUser);
        }
    }
}