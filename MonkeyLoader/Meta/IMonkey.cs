using HarmonyLib;
using MonkeyLoader.Configuration;
using MonkeyLoader.Logging;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;

namespace MonkeyLoader.Meta
{
    /// <summary>
    /// Interface for <see cref="EarlyMonkey{TMonkey}"/>s.
    /// </summary>
    public interface IEarlyMonkey : IMonkey
    {
        /// <summary>
        /// Gets the names of the assemblies and types therein which this pre-patcher targets.
        /// </summary>
        public IEnumerable<PrePatchTarget> PrePatchTargets { get; }
    }

    /// <summary>
    /// The interface for any monkey.
    /// </summary>
    public interface IMonkey : IRun, IShutdown, IComparable<IMonkey>
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
        /// Gets the impacts this (pre-)patcher has on certain features in the order of their size.
        /// </summary>
        public IEnumerable<IFeaturePatch> FeaturePatches { get; }

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
        /// Gets this monkey's name.
        /// </summary>
        public string Name { get; }
    }
}