using HarmonyLib;
using MonkeyLoader.Configuration;
using MonkeyLoader.Logging;
using MonkeyLoader.Meta;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace MonkeyLoader.Patching
{
    public interface IMonkey
    {
        /// <summary>
        /// Gets the name of the assembly this monkey is defined in.
        /// </summary>
        public AssemblyName AssemblyName { get; }

        /// <summary>
        /// Gets the <see cref="Configuration.Config"/> that this monkey can use to load <see cref="ConfigSection"/>s.
        /// </summary>
        public Config Config { get; }

        /// <summary>
        /// Gets whether this monkey failed while patching.
        /// </summary>
        public bool Failed { get; }

        /// <summary>
        /// Gets the <see cref="HarmonyLib.Harmony">Harmony</see> instance to be used by this patcher.
        /// </summary>
        public Harmony Harmony { get; }

        /// <summary>
        /// Gets the <see cref="MonkeyLogger"/> that this monkey can use to log messages to game-specific channels.
        /// </summary>
        public MonkeyLogger Logger { get; }

        /// <summary>
        /// Gets the mod that this monkey is a part of.
        /// </summary>
        public Mod Mod { get; }

        /// <summary>
        /// Gets the name of this monkey's <see cref="Type"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets whether this monkey's <see cref="Run">Run</see>() method has been called.
        /// </summary>
        public bool Ran { get; }

        /// <summary>
        /// Runs this monkey to apply its patching.<br/>
        /// Must only be called once.
        /// </summary>
        /// <returns>Whether it ran successfully.</returns>
        public bool Run();
    }

    /// <summary>
    /// Represents the base class for patchers that run after a game's assemblies have been loaded.<br/>
    /// All mod defined derivatives must derive from <see cref="MonkeyBase{TMonkey}"/> or from another class derived from it.
    /// </summary>
    /// <remarks>
    /// Game assemblies and their types can be directly referenced from these.
    /// </remarks>
    public abstract class Monkey<TMonkey> : MonkeyBase<TMonkey> where TMonkey : Monkey<TMonkey>, new()
    {
        /// <summary>
        /// Allows creating only a single <typeparamref name="TMonkey"/> instance.
        /// </summary>
        protected Monkey()
        { }

        /// <inheritdoc/>
        public override sealed bool Run()
        {
            throwIfRan();
            Ran = true;

            try
            {
                Failed = !onLoaded();
            }
            catch (Exception ex)
            {
                Failed = true;
                Logger.Error(() => ex.Format("Exception while applying patches!"));
            }

            return !Failed;
        }

        /// <summary>
        /// Called right after the game tooling packs and all the game's assemblies have been loaded.<br/>
        /// Use this to apply any patching and return <c>true</c> if it was successful.
        /// </summary>
        /// <returns>Whether the patching was successful.</returns>
        protected abstract bool onLoaded();
    }
}