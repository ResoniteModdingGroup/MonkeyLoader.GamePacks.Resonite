using FrooxEngine.UIX;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLoader.Resonite.UI
{
    public static class UIXExtensions
    {
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