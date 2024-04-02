using FrooxEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.Configuration
{
    public static class SharedConfig
    {
        public const string Identifier = "MonkeyLoader.SharedConfig";
        public const string WriteBackPrefix = "SharedConfig.WriteBack";

        private static readonly HashSet<ISharedDefiningConfigKeyWrapper> _sharedConfigKeys = new();

        public static Slot GetSharedConfigSlot(this World world)
            => world.AssetsSlot.FindChildOrAdd(Identifier);

        internal static bool Initialize()
        {
            if (Engine.Current.WorldManager is null)
                return false;

            Engine.Current.WorldManager.WorldAdded += world => world.WorldRunning += InitializeSharedConfig;

            foreach (var world in Engine.Current.WorldManager.Worlds)
                InitializeSharedConfig(world);

            return true;
        }

        internal static void Register(ISharedDefiningConfigKeyWrapper sharedDefiningConfigKeyWrapper)
            => _sharedConfigKeys.Add(sharedDefiningConfigKeyWrapper);

        private static void InitializeSharedConfig(World world)
        {
            foreach (var sharedConfigKey in _sharedConfigKeys)
                sharedConfigKey.SetupOverride(world);
        }
    }
}