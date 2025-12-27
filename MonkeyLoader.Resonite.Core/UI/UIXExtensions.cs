using FrooxEngine.UIX;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace MonkeyLoader.Resonite.UI
{
    /// <summary>
    /// Contains miscellaneous extension methods for UIX types that couldn't be categorized further.
    /// </summary>
    [TypeForwardedFrom("MonkeyLoader.Resonite.Integration")]
    public static class UIXExtensions
    {
        /// <summary>
        /// Moves this <see cref="InteractionElement.ColorDriver">ColorDriver</see> to
        /// the <paramref name="target"/> <see cref="InteractionElement"/>.
        /// </summary>
        /// <remarks>
        /// This ColorDriver will remain unchanged, except for the
        /// <see cref="InteractionElement.ColorDriver.ColorDrive">ColorDrive</see>
        /// target being set to <see langword="null"/> afterwards.
        /// </remarks>
        /// <param name="sourceDriver">The ColorDriver being moved.</param>
        /// <param name="target">The interaction element that the ColorDriver should be moved to.</param>
        /// <returns>The newly created ColorDriver on the <paramref name="target"/> <see cref="InteractionElement"/>.</returns>
        public static InteractionElement.ColorDriver MoveTo(this InteractionElement.ColorDriver sourceDriver, InteractionElement target)
        {
            var driveTarget = sourceDriver.ColorDrive.Target;
            sourceDriver.ColorDrive.Target = null!;

            var destinationDriver = target.ColorDrivers.Add();
            destinationDriver.CopyValues(sourceDriver);
            destinationDriver.ColorDrive.Target = driveTarget;

            return destinationDriver;
        }
    }
}