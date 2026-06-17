using FrooxEngine;
using System.Runtime.CompilerServices;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Contains extension methods for <see cref="IField{T}">fields</see>
    /// and other <see cref="IWorldElement">world elements</see>
    /// </summary>
    [TypeForwardedFrom("MonkeyLoader.Resonite.FieldExtensions, MonkeyLoader.Resonite.Integration")]
    public static class FieldExtensions
    {
        /// <summary>
        /// Creates a label describing the <paramref name="target"/> reference almost as a <see cref="RefEditor"/> would.
        /// </summary>
        /// <remarks>
        /// For <see cref="Slot"/>s, their <see cref="Component"/>s, and the fields of those the behavior is the same.<br/>
        /// For <see cref="User"/>s, this format is instead:
        /// <c>$"&lt;noparse&gt;{<see cref="User">targetUser</see>.<see cref="User.UserName">UserName</see>}
        /// ({<paramref name="target"/>.<see cref="IWorldElement.ReferenceID">ReferenceID</see>})&lt;/noparse&gt;"</c>
        /// </remarks>
        /// <param name="target">The reference to label.</param>
        /// <returns>A label for the <paramref name="target"/> reference if it is not <c>null</c>; otherwise, <c>"&lt;i&gt;null&lt;/i&gt;"</c>.</returns>
        public static string GetReferenceLabel(this IWorldElement? target)
        {
            if (target is null)
                return "<i>null</i>";

            if (target is Slot targetSlot)
                return $"{targetSlot.Name} ({target.ReferenceID})";

            if (target is User targetUser)
                return $"<noparse>{targetUser.UserName} ({target.ReferenceID})</noparse>";

            var component = target.FindNearestParent<Component>();
            var slot = component?.Slot ?? target.FindNearestParent<Slot>();

            var arg = (component is not null && component != target) ? ("on " + component.Name + " on " + slot.Name) : ((slot is null) ? "" : ("on " + slot.Name));
            return (target is not SyncElement syncElement) ? $"{target.Name ?? target.GetType().Name} {arg} ({target.ReferenceID})" : $"{syncElement.NameWithPath} {arg} ({target.ReferenceID})";
        }
    }
}