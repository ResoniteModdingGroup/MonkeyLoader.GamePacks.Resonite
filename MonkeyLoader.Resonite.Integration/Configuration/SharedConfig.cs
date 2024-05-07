using FrooxEngine;
using MonkeyLoader.Components;
using MonkeyLoader.Configuration;
using MonkeyLoader.Meta;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.Configuration
{
    /// <summary>
    /// Contains constants and helper methods to work with <see cref="ConfigKeySessionShare{T}"/>.
    /// </summary>
    public static class SharedConfig
    {
        /// <summary>
        /// The name of the <see cref="GetSharedConfigSlot(World)">shared config slot</see>
        /// in a <see cref="World"/>'s <see cref="World.AssetsSlot">Assets</see> <see cref="Slot"/>.
        /// </summary>
        public const string Identifier = "MonkeyLoader.SharedConfig";

        /// <summary>
        /// The prefix for the <see cref="IDefiningConfigKey{T}.SetValue(T, string?)">SetValue</see>
        /// <c>eventLabel</c> used when the cause is a change in a <see cref="World"/>'s shared value.
        /// </summary>
        /// <remarks>
        /// The actually passed label has the following format:
        /// <c>$"{<see cref="WriteBackPrefix"/>}.{<see cref="ValueField{T}">field</see>.<see cref="World"/>.<see cref="GetIdentifier">GetIdentifier</see>()}"</c>
        /// </remarks>
        public const string WriteBackPrefix = "SharedConfig.WriteBack";

        private static readonly HashSet<IConfigKeySessionShare> _sharedConfigKeys = new();

        /// <summary>
        /// Gets the <see cref="World.SessionId"/> or the <see cref="World.Name"/>
        /// to use with the <see cref="WriteBackPrefix">WriteBackPrefix</see>.
        /// </summary>
        /// <param name="world"></param>
        /// <returns></returns>
        public static string GetIdentifier(this World world)
            => world.SessionId ?? world.Name;

        /// <summary>
        /// Gets the SharedConfig <see cref="Slot"/> in a <see cref="World"/>'s <see cref="World.AssetsSlot">Assets</see> slot.
        /// </summary>
        /// <param name="world">The <see cref="World"/> to get the <see cref="Slot"/> for.</param>
        /// <returns>The SharedConfig slot for the given world.</returns>
        public static Slot GetSharedConfigSlot(this World world)
            => world.AssetsSlot.FindChildOrAdd(Identifier);

        /// <summary>
        /// Gets the given config owner's <see cref="Slot"/>
        /// under the <see cref="GetSharedConfigSlot(World)">SharedConfig slot</see>.
        /// </summary>
        /// <param name="world">The <see cref="World"/> to get the <see cref="Slot"/> for.</param>
        /// <param name="configOwner">The config owner to get the <see cref="Slot"/> for.</param>
        /// <returns>The config owner's SharedConfig slot for the given world.</returns>
        public static Slot GetSharedConfigSlot(this World world, IConfigOwner configOwner)
            => world.GetSharedConfigSlot().FindChildOrAdd(configOwner.Id);

        /// <summary>
        /// Wraps the given <see cref="IDefiningConfigKey{T}"/> in a <see cref="ConfigKeySessionShare{T}"/>,
        /// to make its local value available as a shared resource in Resonite sessions,
        /// and optionally allow writing back changes from the session to the config item.
        /// </summary>
        /// <typeparam name="T">The type of the config item's value.</typeparam>
        /// <param name="definingKey">The defining key to wrap.</param>
        /// <param name="defaultValue">The default value for the shared config item for users that don't have it themselves.</param>
        /// <param name="allowWriteBack">Whether to allow writing back changes from the session to the config item.</param>
        /// <returns>The wrapped defining key.</returns>
        public static IDefiningConfigKey<T> MakeShared<T>(this IDefiningConfigKey<T> definingKey,
            T? defaultValue = default, bool allowWriteBack = false)
        {
            var entity = (IEntity<IDefiningConfigKey<T>>)definingKey;

            if (entity.Components.TryGet<IConfigKeySessionShare<T>>(out _))
                return definingKey;

            entity.Add(new ConfigKeySessionShare<T>(defaultValue, allowWriteBack));

            return definingKey;
        }

        /// <summary>
        /// Determines whether the <see cref="IConfigKeyChangedEventArgs"/>
        /// were caused by a change in a <see cref="World"/>'s shared value,
        /// and extracts the <see cref="GetIdentifier">World Identifier</see>
        /// from the <see cref="IConfigKeyChangedEventArgs.Label">Label</see> if so.
        /// </summary>
        /// <param name="configKeyChangedEventArgs">The event args of the change.</param>
        /// <param name="worldIdentifier">The extracted World Identifier or <c>null</c>.</param>
        /// <returns><c>true</c> if the event was caused by a change in a <see cref="World"/>'s shared value; otherwise, <c>false</c>.</returns>
        public static bool TryGetWorldIdentifier(this IConfigKeyChangedEventArgs configKeyChangedEventArgs, [NotNullWhen(true)] out string? worldIdentifier)
        {
            if (configKeyChangedEventArgs.HasLabel && configKeyChangedEventArgs.Label.StartsWith(WriteBackPrefix))
            {
                worldIdentifier = configKeyChangedEventArgs.Label[(WriteBackPrefix.Length + 1)..];
                return true;
            }

            worldIdentifier = null;
            return false;
        }

        internal static bool Initialize()
        {
            Engine.Current.WorldManager.WorldAdded += world => world.WorldRunning += InitializeSharedConfig;
            EngineInitHook.Mod.Loader.ModsShutdown += ShutdownSharedConfig;

            foreach (var world in Engine.Current.WorldManager.Worlds)
                InitializeSharedConfig(world);

            return true;
        }

        internal static void Register(IConfigKeySessionShare sharedKeyWrapper)
        {
            _sharedConfigKeys.Add(sharedKeyWrapper);

            if (Engine.Current?.WorldManager is null)
                return;

            foreach (var world in Engine.Current.WorldManager.Worlds)
                sharedKeyWrapper.SetupOverride(world);
        }

        internal static void Unregister(IConfigKeySessionShare sharedConfigKey)
        {
            _sharedConfigKeys.Remove(sharedConfigKey);

            foreach (var world in Engine.Current.WorldManager.Worlds)
                sharedConfigKey.ShutdownOverride(world);
        }

        private static void InitializeSharedConfig(World world)
        {
            foreach (var sharedConfigKey in _sharedConfigKeys)
                sharedConfigKey.SetupOverride(world);
        }

        private static void ShutdownSharedConfig(MonkeyLoader loader, IEnumerable<Mod> mods)
        {
            foreach (var sharedConfigKey in mods.SelectMany(mod => mod.Config.ConfigurationItemDefinitions)
                .SelectCastable<IDefiningConfigKey, IConfigKeySessionShare>())
            {
                Unregister(sharedConfigKey);
            }
        }
    }
}