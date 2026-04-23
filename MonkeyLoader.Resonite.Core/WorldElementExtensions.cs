using FrooxEngine;
using System.Diagnostics.CodeAnalysis;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Contains extensions for <see cref="IWorldElement">world elements</see>.
    /// </summary>
    public static class WorldElementExtensions
    {
        /// <summary>
        /// Gets the allocating <see cref="User"/> of this <see cref="IWorldElement">world element</see>.
        /// </summary>
        /// <remarks>
        /// Implementation is the same as the Allocating User ProtoFlux node.
        /// </remarks>
        /// <param name="element">The world element to get the allocating <see cref="User"/> of.</param>
        /// <returns>This <see cref="IWorldElement">world element</see>'s allocating <see cref="User"/> if valid; otherwise, <see langword="null"/>.</returns>
        public static User? GetAllocatingUser(this IWorldElement? element)
        {
            if (element!.FilterWorldElement() is null)
                return null;

            element!.ReferenceID.ExtractIDs(out var position, out var allocationId);
            var user = element.World.GetUserByAllocationID(allocationId);

            if (user is null || position < user.AllocationIDStart)
                return null;

            return user;
        }

        /// <summary>
        /// Tries to get the allocating <see cref="User"/> of this <see cref="IWorldElement">world element</see>.
        /// </summary>
        /// <remarks>
        /// Implementation is the same as the Allocating User ProtoFlux node.
        /// </remarks>
        /// <param name="element">The world element to get the allocating <see cref="User"/> of.</param>
        /// <param name="user">This <see cref="IWorldElement">world element</see>'s allocating <see cref="User"/> if valid; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if this <see cref="IWorldElement">world element</see> has a valid allocating <see cref="User"/>; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetAllocatingUser(this IWorldElement? element, [NotNullWhen(true)] out User? user)
        {
            user = element.GetAllocatingUser();
            return user is not null;
        }
    }
}