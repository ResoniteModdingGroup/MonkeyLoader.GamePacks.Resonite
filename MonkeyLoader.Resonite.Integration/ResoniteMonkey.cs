using FrooxEngine;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// The interface for Resonite monkeys.
    /// </summary>
    public interface IResoniteMonkey : IMonkey
    {
        /// <summary>
        /// Gets whether this <see cref="ResoniteMonkey{TMonkey}">ResoniteMonkey</see> failed when
        /// <see cref="ResoniteMonkey{TMonkey}.OnEngineReady">OnEngineReady</see>() was called.
        /// </summary>
        public bool EngineReadyFailed { get; }

        /// <summary>
        /// Gets whether this <see cref="ResoniteMonkey{TMonkey}">ResoniteMonkey's</see>
        /// <see cref="ResoniteMonkey{TMonkey}.OnEngineReady">OnEngineReady</see>() method has been called.
        /// </summary>
        public bool EngineReadyRan { get; }
    }

    /// <summary>
    /// Represents the base class for patchers that run after Resonite's assemblies have been loaded.
    /// </summary>
    /// <remarks>
    /// Game assemblies and their types can be directly referenced from these.<br/>
    /// Contains useful overridable methods that are hooked to different points in the game's lifecycle.
    /// </remarks>
    public abstract class ResoniteMonkey<TMonkey> : Monkey<TMonkey>, IResoniteMonkeyInternal
        where TMonkey : ResoniteMonkey<TMonkey>, new()
    {
        /// <inheritdoc/>
        public bool EngineReadyFailed { get; private set; }

        /// <inheritdoc/>
        public bool EngineReadyRan { get; private set; }

        bool IResoniteMonkeyInternal.EngineReady()
        {
            if (Failed)
            {
                Logger.Warn(() => "Monkey already failed, skipping OnEngineReady!");
                return false;
            }

            if (EngineReadyRan)
                throw new InvalidOperationException("A Resonite monkey's OnEngineReady() method must only be called once!");

            EngineReadyRan = true;

            try
            {
                if (!OnEngineReady())
                {
                    Failed = true;
                    Logger.Warn(() => "OnEngineReady failed!");
                }
            }
            catch (Exception ex)
            {
                Failed = true;
                Logger.Error(() => ex.Format("OnEngineReady threw an Exception:"));
            }

            return !Failed;
        }

        void IResoniteMonkeyInternal.EngineShutdownRequested(string reason)
        {
            try
            {
                OnEngineShutdownRequested(reason);
            }
            catch (Exception ex)
            {
                Logger.Error(() => ex.Format("OnEngineShutdownRequested threw an Exception:"));
            }
        }

        /// <summary>
        /// Override this method to be called when the <see cref="Engine"/> is <see cref="Engine.OnReady">ready</see>.
        /// </summary>
        /// <remarks>
        /// This is the primary method for patching used by Resonite Mods as basic facilities of the game
        /// are ready to use, while most other code hasn't been run.<br/>
        /// Override <see cref="OnLoaded">onLoaded</see>() to patch before anything is initialized.
        /// </remarks>
        /// <returns>Whether the patching was successful.</returns>
        protected virtual bool OnEngineReady() => true;

        /// <summary>
        /// Override this method to be called when the <see cref="Engine"/> is <see cref="Engine.OnShutdownRequest">requested to shutdown</see>.
        /// </summary>
        /// <param name="reason">The reason for the shutdown request. Seems to always be <c>Quitting</c>.</param>
        protected virtual void OnEngineShutdownRequested(string reason)
        { }

        /// <remarks>
        /// Override this method if you need to patch something involved in the initialization of the game.
        /// </remarks>
        /// <inheritdoc/>
        protected override bool OnLoaded() => true;
    }

    internal interface IResoniteMonkeyInternal : IResoniteMonkey
    {
        /// <summary>
        /// Call when the <see cref="Engine"/> is <see cref="Engine.OnReady">ready</see>.
        /// </summary>
        /// <returns>Whether the patching was successful.</returns>
        public bool EngineReady();

        /// <summary>
        /// Call when the <see cref="Engine"/> is <see cref="Engine.OnShutdownRequest">requested to shutdown</see>.
        /// </summary>
        /// <param name="reason">The reason for the shutdown request.</param>
        public void EngineShutdownRequested(string reason);
    }
}