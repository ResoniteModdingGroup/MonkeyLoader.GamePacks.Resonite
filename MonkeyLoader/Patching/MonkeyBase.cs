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
        private static readonly Type _monkeyType = typeof(MonkeyBase);
        private Mod _mod;

        /// <inheritdoc/>
        public AssemblyName AssemblyName { get; }

        /// <inheritdoc/>
        public Config Config => Mod.Config;

        /// <summary>
        /// Gets whether this monkey's <see cref="Run">Run</see>() method failed when it was called.
        /// </summary>
        public bool Failed { get; protected set; }

        /// <inheritdoc/>
        public Harmony Harmony => Mod.Harmony;

        /// <inheritdoc/>
        public MonkeyLogger Logger { get; private set; }

        /// <inheritdoc/>
        public Mod Mod
        {
            get => _mod;

            [MemberNotNull(nameof(_mod), nameof(Logger))]
            internal set
            {
                if (value == _mod)
                    return;

                _mod = value;
                Logger = new MonkeyLogger(_mod.Logger, Name);
            }
        }

        /// <inheritdoc/>
        public abstract string Name { get; }

        /// <summary>
        /// Gets whether this monkey's <see cref="Run">Run</see>() method has been called.
        /// </summary>
        public bool Ran { get; private protected set; } = false;

        /// <summary>
        /// Gets whether this monkey's <see cref="Shutdown">Shutdown</see>() failed when it was called.
        /// </summary>
        public bool ShutdownFailed { get; private set; } = false;

        /// <summary>
        /// Gets whether this monkey's <see cref="Shutdown">Shutdown</see>() method has been called.
        /// </summary>
        public bool ShutdownRan { get; private set; } = false;

        internal MonkeyBase()
        {
            var type = GetType();
            AssemblyName = new(type.Assembly.GetName().Name);
        }

        /// <summary>
        /// Runs this monkey to let it patch.<br/>
        /// Must only be called once.
        /// </summary>
        /// <inheritdoc/>
        public abstract bool Run();

        /// <summary>
        /// Lets this monkey cleanup and shutdown.<br/>
        /// Must only be called once.
        /// </summary>
        /// <inheritdoc/>
        public bool Shutdown()
        {
            if (ShutdownRan)
                throw new InvalidOperationException("A monkey's Shutdown() method must only be called once!");

            ShutdownRan = true;

            try
            {
                if (!OnShutdown())
                {
                    ShutdownFailed = true;
                    Logger.Warn(() => "OnShutdown failed!");
                }
            }
            catch (Exception ex)
            {
                ShutdownFailed = true;
                Logger.Error(() => ex.Format("OnShutdown threw an Exception:"));
            }

            return !ShutdownFailed;
        }

        internal static MonkeyBase GetInstance(Type type)
        {
            // Could do more specific inheriting from Monkey<> check
            if (!_monkeyType.IsAssignableFrom(type))
                throw new ArgumentException($"Given type [{type}] doesn't inherit from {_monkeyType.FullName}!", nameof(type));

            return Traverse.Create(type).Property<MonkeyBase>("Instance").Value;
        }

        /// <summary>
        /// Lets this monkey cleanup and shutdown.
        /// </summary>
        /// <inheritdoc/>
        protected virtual bool OnShutdown() => true;

        private protected void ThrowIfRan()
        {
            if (Ran)
                throw new InvalidOperationException("A monkey's Run() method must only be called once!");
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