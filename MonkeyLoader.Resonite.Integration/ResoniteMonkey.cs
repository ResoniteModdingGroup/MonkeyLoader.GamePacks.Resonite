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
        bool IResoniteMonkey.OnEngineReady() => onEngineReady();

        void IResoniteMonkey.OnEngineShutdown() => onEngineShutdown();

        void IResoniteMonkey.OnEngineShutdownRequested(string reason) => onEngineShutdownRequested(reason);

        /// <summary>
        /// Override this method to be called when the <see cref="Engine"/> is <see cref="Engine.OnReady">ready</see>.
        /// </summary>
        /// <remarks>
        /// This is the primary method for patching used by Resonite Mods as basic facilities of the game
        /// are ready to use, while most other code hasn't been run.<br/>
        /// Override <see cref="onLoaded">onLoaded</see>() to patch before anything is initialized.
        /// </remarks>
        /// <returns>Whether the patching was successful.</returns>
        protected virtual bool onEngineReady() => true;

        /// <summary>
        /// Override this method to be called when the <see cref="Engine"/> is <see cref="Engine.OnShutdown">definitely shutting down</see>.
        /// </summary>
        protected virtual void onEngineShutdown()
        { }

        /// <summary>
        /// Override this method to be called when the <see cref="Engine"/> is <see cref="Engine.OnShutdownRequest">requested to shutdown</see>.
        /// </summary>
        /// <param name="reason">The reason for the shutdown request. Seems to always be <c>Quitting</c>.</param>
        protected virtual void onEngineShutdownRequested(string reason)
        { }

        /// <remarks>
        /// Override this method if you need to patch something involved in the initialization of the game.
        /// </remarks>
        /// <inheritdoc/>
        protected override bool onLoaded() => true;
    }

    internal interface IResoniteMonkey : IMonkey
    {
        /// <summary>
        /// Call when the <see cref="Engine"/> is <see cref="Engine.OnReady">ready</see>.
        /// </summary>
        /// <returns>Whether the patching was successful.</returns>
        bool OnEngineReady();

        /// <summary>
        /// Call when the <see cref="Engine"/> is <see cref="Engine.OnShutdown">definitely shutting down</see>.
        /// </summary>
        void OnEngineShutdown();

        /// <summary>
        /// Call when the <see cref="Engine"/> is <see cref="Engine.OnShutdownRequest">requested to shutdown</see>.
        /// </summary>
        /// <param name="reason">The reason for the shutdown request.</param>
        void OnEngineShutdownRequested(string reason);
    }
}