using FrooxEngine;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Contains extension methods for <see cref="IField{T}">fields</see>
    /// and other <see cref="IWorldElement">world elements</see>
    /// </summary>
    [TypeForwardedFrom("MonkeyLoader.Resonite.Integration")]
    public static class FieldExtensions
    {
        /// <summary>
        /// Creates a label describing the <paramref name="target"/> reference as a <see cref="RefEditor"/> would.
        /// </summary>
        /// <param name="target">The reference to label.</param>
        /// <returns>A label for the <paramref name="target"/> reference if it is not <c>null</c>; otherwise, <c>&lt;i&gt;null&lt;/i&gt;</c>.</returns>
        public static string GetReferenceLabel(this IWorldElement? target)
        {
            if (target is null)
                return "<i>null</i>";

            if (target is Slot targetSlot)
                return $"{targetSlot.Name} ({target.ReferenceID})";

            var component = target.FindNearestParent<Component>();
            var slot = component?.Slot ?? target.FindNearestParent<Slot>();

            var arg = (component is not null && component != target) ? ("on " + component.Name + " on " + slot.Name) : ((slot is null) ? "" : ("on " + slot.Name));
            return (target is not SyncElement syncElement) ? $"{target.Name ?? target.GetType().Name} {arg} ({target.ReferenceID})" : $"{syncElement.NameWithPath} {arg} ({target.ReferenceID})";
        }
    }
}