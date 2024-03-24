using FrooxEngine;
using HarmonyLib;
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
        /// <see cref="ResoniteMonkey{TMonkey}.OnEngineInit">OnEngineReady</see>() was called.
        /// </summary>
        public bool EngineInitFailed { get; }

        /// <summary>
        /// Gets whether this <see cref="ResoniteMonkey{TMonkey}">ResoniteMonkey's</see>
        /// <see cref="ResoniteMonkey{TMonkey}.OnEngineInit">OnEngineInit</see>() method has been called.
        /// </summary>
        public bool EngineInitRan { get; }

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
    /// Represents the base class for patchers that run after Resonite's assemblies have been loaded and that hook into the game's lifecycle.
    /// </summary>
    /// <remarks>
    /// Game assemblies and their types can be directly referenced from these.<br/>
    /// Contains useful overridable methods that are hooked to different points in the game's lifecycle.
    /// </remarks>
    /// <inheritdoc/>
    public abstract class ResoniteMonkey<TMonkey> : Monkey<TMonkey>, IResoniteMonkeyInternal
        where TMonkey : ResoniteMonkey<TMonkey>, new()
    {
        /// <inheritdoc/>
        public bool EngineInitFailed { get; private set; }

        /// <inheritdoc/>
        public bool EngineInitRan { get; private set; }

        /// <inheritdoc/>
        public bool EngineReadyFailed { get; private set; }

        /// <inheritdoc/>
        public bool EngineReadyRan { get; private set; }

        /// <summary>
        /// Allows creating only a single <typeparamref name="TMonkey"/> instance.
        /// </summary>
        protected ResoniteMonkey()
        { }

        bool IResoniteMonkeyInternal.EngineInit()
        {
            if (Failed)
            {
                Logger.Warn(() => "Monkey already failed Run, skipping OnEngineInit!");
                return false;
            }

            if (EngineInitRan)
                throw new InvalidOperationException("A Resonite monkey's OnEngineInit() method must only be called once!");

            EngineInitRan = true;
            Logger.Debug(() => "Running OnEngineInit");

            try
            {
                if (!OnEngineInit())
                {
                    EngineInitFailed = true;
                    Logger.Warn(() => "OnEngineInit failed!");
                }
            }
            catch (Exception ex)
            {
                EngineInitFailed = true;
                Logger.Error(() => ex.Format("OnEngineInit threw an Exception:"));
            }

            return !EngineInitFailed;
        }

        bool IResoniteMonkeyInternal.EngineReady()
        {
            if (EngineInitFailed)
            {
                Logger.Warn(() => "Monkey already failed OnEngineInit, skipping OnEngineReady!");
                return false;
            }

            if (EngineReadyRan)
                throw new InvalidOperationException("A Resonite monkey's OnEngineReady() method must only be called once!");

            EngineReadyRan = true;
            Logger.Debug(() => "Running OnEngineReady");

            try
            {
                if (!OnEngineReady())
                {
                    EngineReadyFailed = true;
                    Logger.Warn(() => "OnEngineReady failed!");
                }
            }
            catch (Exception ex)
            {
                EngineReadyFailed = true;
                Logger.Error(() => ex.Format("OnEngineReady threw an Exception:"));
            }

            return !EngineReadyFailed;
        }

        void IResoniteMonkeyInternal.EngineShutdownRequested(string reason)
        {
            try
            {
                Logger.Debug(() => "Running OnEngineShutdownRequested");
                OnEngineShutdownRequested(reason);
            }
            catch (Exception ex)
            {
                Logger.Error(() => ex.Format("OnEngineShutdownRequested threw an Exception:"));
            }
        }

        /// <summary>
        /// Override this method to be called when the <see cref="Engine"/>'s <see cref="Engine.Initialize">Initialize</see>() method has just been called.<br/>
        /// This method can be used if elements of the initialization need to be modified,
        /// such as changing <see cref="LoadProgressIndicator.TotalFixedPhaseCount"/>.<br/>
        /// Return <c>true</c> if patching was successful.
        /// </summary>
        /// <remarks>
        /// Override <see cref="OnLoaded">OnLoaded</see>() to patch before <i>anything</i> is initialized,
        /// or <see cref="OnEngineReady">OnEngineReady</see>() to patch when basic facilities of the game
        /// are ready to use, while most other code hasn't been run yet.
        /// <para/>
        /// <i>By default:</i> Doesn't do anything except return <c>true</c>.
        /// </remarks>
        /// <returns>Whether the patching was successful.</returns>
        protected virtual bool OnEngineInit() => true;

        /// <summary>
        /// Override this method to be called when the <see cref="Engine"/> is <see cref="Engine.OnReady">ready</see>.<br/>
        /// This is the primary method for patching used by Resonite Mods as basic facilities of the game
        /// are ready to use, while most other code hasn't been run yet.<br/>
        /// Return <c>true</c> if patching was successful.
        /// </summary>
        /// <remarks>
        /// Override <see cref="OnLoaded">OnLoaded</see>() to patch before anything is initialized,
        /// but strongly consider also overriding this method if you do that.<br/>
        /// Otherwise your patches will be applied twice, if you're using <c>[<see cref="HarmonyPatchCategory"/>(nameof(MyPatcher))]</c> attributes.
        /// <para/>
        /// <i>By default:</i> Applies the <see cref="Harmony"/> patches of the
        /// <see cref="Harmony.PatchCategory(string)">category</see> with this patcher's type's name.<br/>
        /// Easy to apply by using <c>[<see cref="HarmonyPatchCategory"/>(nameof(MyPatcher))]</c> attribute.
        /// </remarks>
        /// <returns>Whether the patching was successful.</returns>
        protected virtual bool OnEngineReady() => base.OnLoaded();

        /// <summary>
        /// Override this method to be called when the <see cref="Engine"/> is <see cref="Engine.OnShutdownRequest">requested to shutdown</see>.
        /// </summary>
        /// <param name="reason">The reason for the shutdown request. Seems to always be <c>Quitting</c>.</param>
        protected virtual void OnEngineShutdownRequested(string reason)
        { }

        /// <remarks>
        /// Override this method if you need to patch something involved in the initialization of the game.<br/>
        /// For ResoniteMonkeys, the default behavior of<see cref="Monkey{TMonkey}.OnLoaded">OnLoaded</see>()
        /// is moved to <see cref="OnEngineReady">OnEngineReady</see>().
        /// <para/>
        /// Strongly consider also overriding <see cref="OnEngineReady">OnEngineReady</see>() if you override this method.<br/>
        /// Otherwise your patches will be applied twice, if you're using <c>[<see cref="HarmonyPatchCategory"/>(nameof(MyPatcher))]</c> attributes.
        /// <para/>
        /// <i>By default:</i> Doesn't do anything except return <c>true</c>.
        /// </remarks>
        /// <inheritdoc/>
        protected override bool OnLoaded() => true;
    }

    internal interface IResoniteMonkeyInternal : IResoniteMonkey
    {
        /// <summary>
        /// Call when the <see cref="Engine"/>'s <see cref="Engine.Initialize">Initialize</see>()
        /// method has just been called.
        /// </summary>
        /// <returns>Whether the patching was successful.</returns>
        public bool EngineInit();

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