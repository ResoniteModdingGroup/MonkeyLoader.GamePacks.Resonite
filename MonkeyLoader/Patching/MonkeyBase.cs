#define DISABLE_CODEMAID
using HarmonyLib;
using MonkeyLoader.Configuration;
using MonkeyLoader.Logging;
using MonkeyLoader.Meta;
using System;
using System.Diagnostics.CodeAnalysis;

namespace MonkeyLoader.Patching
{
    /// <summary>
    /// Abstract base for regular <see cref="Monkey{TMonkey}"/>s and <see cref="EarlyMonkey{TMonkey}"/>s.
    /// </summary>
    public abstract class MonkeyBase : IMonkey
    {
        private static readonly Type monkeyType = typeof(MonkeyBase);
        private Mod mod;

        /// <inheritdoc/>
        public AssemblyName AssemblyName { get; }

        /// <inheritdoc/>
        public Config Config => Mod.Config;

        /// <inheritdoc/>
        public Harmony Harmony => Mod.Harmony;

        /// <inheritdoc/>
        public MonkeyLogger Logger { get; private set; }

        /// <inheritdoc/>
        public bool Failed { get; protected set; }

        /// <inheritdoc/>
        public Mod Mod
        {
            get => mod;

            [MemberNotNull(nameof(mod), nameof(Logger))]
            internal set
            {
                if (value == mod)
                    return;

                mod = value;
                Logger = new MonkeyLogger(mod.Logger, Name);
            }
        }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public bool Ran { get; protected set; }

        internal MonkeyBase()
        {
            var type = GetType();
            Name = type.Name;
            AssemblyName = new(type.Assembly.GetName().Name);
        }

        /// <inheritdoc/>
        public abstract bool Run();

        internal static MonkeyBase GetInstance(Type type)
        {
            // Could do more specific inheriting from Monkey<> check
            if (!monkeyType.IsAssignableFrom(type))
                throw new ArgumentException($"Given type [{type}] doesn't inherit from {monkeyType.FullName}!", nameof(type));

            return Traverse.Create(type).Property<MonkeyBase>("Instance").Value;
        }

        private protected void throwIfRan()
        {
            if (Ran)
                throw new InvalidOperationException($"A monkey's Run() method must only be called once!");
        }
    }

    /// <inheritdoc/>
    /// <typeparam name="TMonkey">The type of the actual patcher.</typeparam>
    public abstract class MonkeyBase<TMonkey> : MonkeyBase where TMonkey : MonkeyBase<TMonkey>, new()
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
        public static MonkeyBase Instance { get; } = new TMonkey();

        /// <summary>
        /// Gets the <see cref="MonkeyLogger"/> that this patcher can use to log messages to game-specific channels.
        /// </summary>
        public new static MonkeyLogger Logger => Instance.Logger;

        /// <summary>
        /// Gets the mod that this patcher is a part of.
        /// </summary>
        public new static Mod Mod => Instance.Mod;

        /// <summary>
        /// Allows creating only a single <typeparamref name="TMonkey"/> instance.
        /// </summary>
        internal MonkeyBase() : base()
        {
            if (Instance is not null)
                throw new InvalidOperationException("Can't create more than one patcher instance!");
        }
    }
}