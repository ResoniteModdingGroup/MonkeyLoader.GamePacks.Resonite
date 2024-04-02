using FrooxEngine;
using MonkeyLoader.Resonite.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite
{
    public static class SharedComponentHelper
    {
        public static T GetSharedComponentOrCreate<T>(this Slot slot, string sharedKey, Action<T> onCreate,
            int version = 0, bool replaceExisting = false, bool updateExisting = false) where T : Component, new()
        {
            var component = slot.World.KeyOwner(sharedKey);

            if (component is not null)
            {
                if (component is T typedComponent)
                {
                    if (version > 0 && slot.World.KeyVersion(sharedKey) < version)
                    {
                        updateExisting = true;
                        slot.World.RequestKey(typedComponent, sharedKey, version, false);
                    }

                    if (updateExisting)
                        onCreate(typedComponent);

                    return typedComponent;
                }

                if (!replaceExisting)
                    throw new Exception("An incompatible component is stored under the key " + sharedKey + $"\nExisting: {component}");
            }

            var attachedComponent = slot.AttachComponent<T>();
            onCreate(attachedComponent);

            slot.World.RequestKey(attachedComponent, sharedKey, version, false);

            return attachedComponent;
        }
    }
}