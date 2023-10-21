using HarmonyLib;
using MonkeyLoader.Configuration;
using MonkeyLoader.Logging;
using MonkeyLoader.Meta;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

namespace MonkeyLoader.Patching
{
    /// <summary>
    /// Represents the base class for patchers that run after a game's assemblies have been loaded.<br/>
    /// All mod defined derivatives must derive from <see cref="Monkey{TMonkey}"/> or from another class derived from it.
    /// </summary>
    /// <remarks>
    /// Game assemblies and their types can be directly referenced from these.
    /// </remarks>
    public abstract class Monkey
    {
        private static readonly Type monkeyType = typeof(Monkey);

        /// <summary>
        /// Gets the <see cref="Configuration.Config"/> that this patcher can use to load <see cref="ConfigSection"/>s.
        /// </summary>
        public Config Config => Mod.Config;

        /// <summary>
        /// Gets the <see cref="HarmonyLib.Harmony">Harmony</see> instance to be used by this patcher.
        /// </summary>
        public Harmony Harmony => Mod.Harmony;

        /// <summary>
        /// Gets the <see cref="MonkeyLogger"/> that this patcher can use to log messages to game-specific channels.
        /// </summary>
        public MonkeyLogger Logger => Mod.Logger;

        /// <summary>
        /// Gets the mod that this patcher is a part of.
        /// </summary>
        public Mod Mod { get; internal set; }

        internal Monkey()
        { }

        internal static Monkey GetInstance(Type type)
        {
            // Could do more specific inheriting from Monkey<> check
            if (!monkeyType.IsAssignableFrom(type))
                throw new ArgumentException($"Given type [{type}] doesn't inherit from {monkeyType.FullName}!", nameof(type));

            return (Monkey)type.GetProperty("Instance", AccessTools.all | BindingFlags.FlattenHierarchy).GetValue(null);
        }

        /// <summary>
        /// Called right after the game tooling packs and all the game's assemblies have been loaded.
        /// </summary>
        protected internal virtual void OnLoaded()
        { }
    }

    /// <summary>
    /// Represents the base class for patchers that run after a game's assemblies have been loaded.<br/>
    /// </summary>
    /// <remarks>
    /// Game assemblies and their types can be directly referenced from these.<br/>
    /// Game tooling packs should expand this with useful overridable methods
    /// that are hooked to different points in the game's lifecycle.
    /// </remarks>
    /// <typeparam name="TMonkey">The type of the actual patcher.</typeparam>
    public abstract class Monkey<TMonkey> : Monkey where TMonkey : Monkey<TMonkey>, new()
    {
        /// <summary>
        /// Gets the <see cref="Configuration.Config"/> that this patcher can use to load <see cref="ConfigSection"/>s.
        /// </summary>
        public new static Config Config => Instance.Config;

        /// <summary>
        /// Gets the <see cref="HarmonyLib.Harmony">Harmony</see> instance to be used by this patcher.
        /// </summary>
        public new static Harmony Harmony => Instance.Harmony;

        /// <summary>
        /// Gets the instance of this patcher.
        /// </summary>
        public static Monkey Instance { get; } = new TMonkey();

        /// <summary>
        /// Gets the <see cref="MonkeyLogger"/> that this patcher can use to log messages to game-specific channels.
        /// </summary>
        public new static MonkeyLogger Logger => Instance.Logger;

        /// <summary>
        /// Gets the mod that this patcher is a part of.
        /// </summary>
        public new static Mod Mod => Instance.Mod;

        static Monkey()
        {
            File.AppendAllText("test.log", $"[{DateTime.Now:HH:mm:dd:ss:ffff}] Static constructor of {typeof(TMonkey).FullName} - Instance is: {(Instance is null ? "null" : "existing")}{Environment.NewLine}");
        }

        /// <summary>
        /// Allows creating only a single <see cref="Monkey{TMonkey}"/> instance.
        /// </summary>
        protected Monkey() : base()
        {
            if (Instance is not null)
                throw new InvalidOperationException("Can't create more than one patcher instance!");
        }
    }
}