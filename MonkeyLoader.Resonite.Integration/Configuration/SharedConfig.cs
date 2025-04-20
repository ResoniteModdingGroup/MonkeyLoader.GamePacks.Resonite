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

        private static readonly HashSet<IConfigKeySessionShare> _configKeySessionShares = [];

        /// <summary>
        /// Gets the <see cref="World.SessionId"/> or the <see cref="World.Name"/>
        /// to use with the <see cref="WriteBackPrefix">WriteBackPrefix</see>.
        /// </summary>
        /// <param name="world">The world to get the identifier for.</param>
        /// <returns>A string identifying the world.</returns>
        public static string GetIdentifier(this World world)
            => world.SessionId ?? world.Name;

        /// <summary>
        /// Gets the SharedConfig <see cref="Slot"/> in a <see cref="World"/>'s <see cref="World.AssetsSlot">Assets</see> slot.
        /// </summary>
        /// <param name="world">The <see cref="World"/> to get the <see cref="Slot"/> for.</param>
        /// <returns>The SharedConfig slot for the given world.</returns>
        public static Slot GetSharedConfigSlot(this World world)
        {
            if (world.AssetsSlot.FindChild(Identifier) is not Slot sharedConfigSlot)
            {
                sharedConfigSlot = world.AssetsSlot.AddSlot(Identifier, false);
                sharedConfigSlot.AttachComponent<Comment>().Text.Value =
                    "This slot is used by a feature of the config system of MonkeyLoader. You can safely integrate with the config, make this slot persistent or delete it - though it will automatically be recreated if something needs it.";
            }

            return sharedConfigSlot;
        }

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
        /// <typeparam name="TKey">The type of the config item's value.</typeparam>
        /// <typeparam name="TShared">
        /// The type of the resource shared in Resonite <see cref="World"/>s.
        /// Must be a valid generic parameter for <see cref="ValueField{T}"/> components.
        /// </typeparam>
        /// <param name="convertToShared">Converts the config item's value to the shared resource's.</param>
        /// <param name="convertToKey">Converts the shared resource's value to the config item's.</param>
        /// <param name="definingKey">The defining key to wrap.</param>
        /// <param name="defaultValue">The default value for the shared config item for users that don't have it themselves.</param>
        /// <param name="allowWriteBack">Whether to allow writing back changes from the session to the config item.</param>
        /// <returns>The wrapped defining key.</returns>
        public static IDefiningConfigKey<TKey> MakeShared<TKey, TShared>(this IEntity<IDefiningConfigKey<TKey>> definingKey,
            Converter<TKey?, TShared?> convertToShared, Converter<TShared?, TKey?> convertToKey, TKey? defaultValue = default, bool allowWriteBack = false)
        {
            if (definingKey.Components.TryGet<IConfigKeySessionShare<TKey, TShared>>(out _))
                return definingKey.Self;

            definingKey.Add(new ConfigKeySessionShare<TKey, TShared>(convertToShared, convertToKey, defaultValue, allowWriteBack));

            return definingKey.Self;
        }

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
        public static IDefiningConfigKey<T> MakeShared<T>(this IEntity<IDefiningConfigKey<T>> definingKey,
            T? defaultValue = default, bool allowWriteBack = false)
        {
            if (definingKey.Components.TryGet<IConfigKeySessionShare<T>>(out _))
                return definingKey.Self;

            definingKey.Add(new ConfigKeySessionShare<T>(defaultValue, allowWriteBack));

            return definingKey.Self;
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

        internal static void Register(IConfigKeySessionShare sessionShare)
        {
            EngineInitHook.Logger.Debug(() => $"Registering {sessionShare.GetType().CompactDescription()} {(sessionShare.AllowWriteBack ? "with" : "without")} writeback for key [{sessionShare.ConfigKey.Id}]!");

            _configKeySessionShares.Add(sessionShare);

            if (Engine.Current?.WorldManager is null)
                return;

            foreach (var world in Engine.Current.WorldManager.Worlds)
                sessionShare.SetupOverride(world);
        }

        internal static void Unregister(IConfigKeySessionShare sessionShare)
        {
            EngineInitHook.Logger.Debug(() => $"Unregistering {sessionShare.GetType().CompactDescription()} {(sessionShare.AllowWriteBack ? "with" : "without")} writeback for key [{sessionShare.ConfigKey.Id}]!");

            _configKeySessionShares.Remove(sessionShare);

            foreach (var world in Engine.Current.WorldManager.Worlds)
                sessionShare.ShutdownOverride(world);
        }

        private static void InitializeSharedConfig(World world)
        {
            foreach (var sharedConfigKey in _configKeySessionShares)
                sharedConfigKey.SetupOverride(world);
        }

        private static void ShutdownSharedConfig(MonkeyLoader loader, IEnumerable<Mod> mods)
        {
            foreach (var sessionShare in mods.SelectMany(mod => mod.Config.ConfigurationItemDefinitions)
                .SelectMany(configKey => configKey.Components.GetAll<IComponent<IDefiningConfigKey>>())
                .OfType<IConfigKeySessionShare>())
            {
                Unregister(sessionShare);
            }
        }
    }
}