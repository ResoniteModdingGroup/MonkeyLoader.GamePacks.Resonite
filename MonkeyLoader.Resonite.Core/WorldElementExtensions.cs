using FrooxEngine;
using System.Diagnostics.CodeAnalysis;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Contains extensions for <see cref="IWorldElement">world elements</see>.
    /// </summary>
    public static class WorldElementExtensions
    {
        // These are only necessary because FrooxEngine doesn't have proper nullability annotations
#pragma warning disable CS8621 // Nullability of reference types in return type doesn't match the target delegate (possibly because of nullability attributes).
#pragma warning disable CS8631 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match constraint type.
#pragma warning disable CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.

        /// <summary>
        /// Filters out any <see cref="Delegate"/>s with non-static methods targeting
        /// <see langword="null"/>, <see cref="IWorldElement.IsLocalElement">local</see>,
        /// or <see cref="IWorldElement.IsRemoved">removed</see> <see cref="IWorldElement">world element</see>.
        /// </summary>
        /// <param name="delegate">The <see cref="Delegate"/> to filter.</param>
        /// <returns>
        /// The <see cref="Delegate"/> if it's a static method or targeting a
        /// <see cref="IWorldElement.IsLocalElement">global</see>
        /// and not <see cref="IWorldElement.IsRemoved">removed</see>
        /// <see cref="IWorldElement">world element</see>; otherwise, <see langword="null"/>.
        /// </returns>
        public static Delegate? FilterGlobalWorldDelegate(this Delegate? @delegate)
        {
            if (@delegate is null)
                return null;

            if (@delegate.Method.IsStatic)
                return @delegate;

            if ((@delegate.Target as IWorldElement)!.FilterGlobalWorldElement() is not null)
                return @delegate;

            return null;
        }

        /// <summary>
        /// <see cref="FilterGlobalWorldDelegate(Delegate?)">Filters</see> this sequence of <see cref="Delegate"/>s for any
        /// non-static methods targeting <see langword="null"/>, <see cref="IWorldElement.IsLocalElement">local</see>,
        /// or <see cref="IWorldElement.IsRemoved">removed</see> <see cref="IWorldElement">world elements</see>.
        /// </summary>
        /// <param name="delegates">The sequence of <see cref="Delegate"/>s to filter.</param>
        /// <returns>
        /// The sequence of <see cref="Delegate"/>s without any non-static methods targeting
        /// <see langword="null"/>, <see cref="IWorldElement.IsLocalElement">local</see>,
        /// or <see cref="IWorldElement.IsRemoved">removed</see> <see cref="IWorldElement">world elements</see>.
        /// </returns>
        public static IEnumerable<Delegate> FilterGlobalWorldDelegates(this IEnumerable<Delegate?> delegates)
            => delegates.Select(FilterGlobalWorldDelegate).OfType<Delegate>();

        /// <summary>
        /// Filters out any <see cref="Delegate"/>s with non-static methods targeting
        /// <see langword="null"/> or <see cref="IWorldElement.IsRemoved">removed</see>
        /// <see cref="IWorldElement">world element</see>.
        /// </summary>
        /// <param name="delegate">The <see cref="Delegate"/> to filter.</param>
        /// <returns>
        /// The <see cref="Delegate"/> if it's a static method or targeting
        /// a not <see cref="IWorldElement.IsRemoved">removed</see>
        /// <see cref="IWorldElement">world element</see>; otherwise, <see langword="null"/>.
        /// </returns>
        public static Delegate? FilterWorldDelegate(this Delegate? @delegate)
        {
            if (@delegate is null)
                return null;

            if (@delegate.Method.IsStatic)
                return @delegate;

            if ((@delegate.Target as IWorldElement)!.FilterWorldElement() is not null)
                return @delegate;

            return null;
        }

        /// <summary>
        /// <see cref="FilterWorldDelegate(Delegate?)">Filters</see> this sequence of <see cref="Delegate"/>s
        /// for any non-static methods targeting <see langword="null"/> or
        /// <see cref="IWorldElement.IsRemoved">removed</see> <see cref="IWorldElement">world elements</see>.
        /// </summary>
        /// <param name="delegates">The sequence of <see cref="Delegate"/>s to filter.</param>
        /// <returns>
        /// The sequence of <see cref="Delegate"/>s without any non-static methods targeting
        /// <see langword="null"/> or <see cref="IWorldElement.IsRemoved">removed</see>
        /// <see cref="IWorldElement">world elements</see>.
        /// </returns>
        public static IEnumerable<Delegate> FilterWorldDelegates(this IEnumerable<Delegate?> delegates)
            => delegates.Select(FilterWorldDelegate).OfType<Delegate>();

        /// <summary>
        /// <see cref="FrooxEngine.WorldElementExtensions.FilterGlobalWorldElement{T}(T)">Filters</see>
        /// this sequence for any <see langword="null"/>, <see cref="IWorldElement.IsLocalElement">local</see>,
        /// or <see cref="IWorldElement.IsRemoved">removed</see> elements.
        /// </summary>
        /// <typeparam name="T">The type of the world elements.</typeparam>
        /// <param name="elements">The sequence of world elements to filter.</param>
        /// <returns>
        /// The sequence of world elements without any <see langword="null"/>,
        /// <see cref="IWorldElement.IsLocalElement">local</see>,
        /// or <see cref="IWorldElement.IsRemoved">removed</see> ones.
        /// </returns>
        public static IEnumerable<T> FilterGlobalWorldElements<T>(this IEnumerable<T?> elements)
            where T : class, IWorldElement
            => elements.Select(FrooxEngine.WorldElementExtensions.FilterGlobalWorldElement).OfType<T>();

        /// <summary>
        /// <see cref="FrooxEngine.WorldElementExtensions.FilterWorldElement{T}(T)">Filters</see>
        /// this sequence for any <see langword="null"/> or <see cref="IWorldElement.IsRemoved">removed</see> elements.
        /// </summary>
        /// <typeparam name="T">The type of the world elements.</typeparam>
        /// <param name="elements">The sequence of world elements to filter.</param>
        /// <returns>The sequence of world elements without any <see langword="null"/> or <see cref="IWorldElement.IsRemoved">removed</see> ones.</returns>
        public static IEnumerable<T> FilterWorldElements<T>(this IEnumerable<T?> elements)
            where T : class, IWorldElement
            => elements.Select(FrooxEngine.WorldElementExtensions.FilterWorldElement).OfType<T>();

#pragma warning restore CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.
#pragma warning restore CS8631 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match constraint type.
#pragma warning restore CS8621 // Nullability of reference types in return type doesn't match the target delegate (possibly because of nullability attributes).

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