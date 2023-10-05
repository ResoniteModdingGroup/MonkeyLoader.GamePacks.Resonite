using FrooxEngine;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Represents the base class for patchers that run after Resonite's assemblies have been loaded.
    /// </summary>
    /// <remarks>
    /// Game assemblies and their types can be directly referenced from these.<br/>
    /// Contains useful overridable methods that are hooked to different points in the game's lifecycle.
    /// </remarks>
    public abstract class ResoniteMonkey<TMonkey> : Monkey<TMonkey>, IResoniteMonkey
        where TMonkey : ResoniteMonkey<TMonkey>, new()
    {
        void IResoniteMonkey.OnEngineReady() => OnEngineReady();

        void IResoniteMonkey.OnEngineShutdown(string reason) => OnEngineShutdown(reason);

        void IResoniteMonkey.OnEngineShutdownRequested() => OnEngineShutdownRequested();

        /// <summary>
        /// Called when the <see cref="Engine"/> is <see cref="Engine.OnReady">ready</see>.
        /// </summary>
        protected internal virtual void OnEngineReady()
        { }

        /// <summary>
        /// Called when the <see cref="Engine"/> is <see cref="Engine.OnShutdown">definitely shutting down</see>.
        /// </summary>
        protected internal virtual void OnEngineShutdown(string reason)
        { }

        /// <summary>
        /// Called when the <see cref="Engine"/> is <see cref="Engine.OnShutdownRequest">requested to shutdown</see>.
        /// </summary>
        protected internal virtual void OnEngineShutdownRequested()
        { }
    }

    internal interface IResoniteMonkey
    {
        /// <summary>
        /// Called when the <see cref="Engine"/> is <see cref="Engine.OnReady">ready</see>.
        /// </summary>
        void OnEngineReady();

        /// <summary>
        /// Called when the <see cref="Engine"/> is <see cref="Engine.OnShutdown">definitely shutting down</see>.
        /// </summary>
        void OnEngineShutdown(string reason);

        /// <summary>
        /// Called when the <see cref="Engine"/> is <see cref="Engine.OnShutdownRequest">requested to shutdown</see>.
        /// </summary>
        void OnEngineShutdownRequested();
    }
}