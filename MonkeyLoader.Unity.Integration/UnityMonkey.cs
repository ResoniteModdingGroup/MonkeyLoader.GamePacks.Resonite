using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MonkeyLoader.Unity
{
    /// <summary>
    /// The interface for Unity monkeys.
    /// </summary>
    public interface IUnityMonkey : IMonkey
    {
        /// <summary>
        /// Gets whether this <see cref="UnityMonkey{TMonkey}">UnityMonkey</see> failed when
        /// <see cref="UnityMonkey{TMonkey}.OnFirstSceneReady">OnFirstSceneReady</see>() was called.
        /// </summary>
        public bool FirstSceneReadyFailed { get; }

        /// <summary>
        /// Gets whether this <see cref="UnityMonkey{TMonkey}">ResoniteMonkey's</see>
        /// <see cref="UnityMonkey{TMonkey}.OnFirstSceneReady">OnFirstSceneReady</see>() method has been called.
        /// </summary>
        public bool FirstSceneReadyRan { get; }
    }

    /// <summary>
    /// Represents the base class for patchers that run after all Unity and game assemblies have been loaded.
    /// </summary>
    /// <remarks>
    /// Game assemblies and their types can be directly referenced from these.<br/>
    /// Allows hooking into the game loading after Unity has loaded the first <see cref="Scene"/>
    /// to ensure that <see cref="MonoBehaviour"/> and other engine methods keep working properly.
    /// </remarks>
    /// <inheritdoc/>
    public abstract class UnityMonkey<TMonkey> : Monkey<TMonkey>, IUnityMonkeyInternal
        where TMonkey : UnityMonkey<TMonkey>, new()
    {
        /// <inheritdoc/>
        public bool FirstSceneReadyFailed { get; private set; }

        /// <inheritdoc/>
        public bool FirstSceneReadyRan { get; private set; }

        /// <summary>
        /// Allows creating only a single <typeparamref name="TMonkey"/> instance.
        /// </summary>
        protected UnityMonkey()
        { }

        bool IUnityMonkeyInternal.FirstSceneReady(Scene scene)
        {
            if (Failed)
            {
                Warn(() => "Monkey already failed Run, skipping OnFirstSceneReady!");
                return false;
            }

            if (FirstSceneReadyRan)
                throw new InvalidOperationException("A Unity monkey's OnFirstSceneReady() method must only be called once!");

            FirstSceneReadyRan = true;
            Debug(() => "Running OnFirstSceneReady");

            try
            {
                if (!OnFirstSceneReady(scene))
                {
                    FirstSceneReadyFailed = true;
                    Warn(() => "OnFirstSceneReady failed!");
                }
            }
            catch (Exception ex)
            {
                FirstSceneReadyFailed = true;
                Error(() => ex.Format("OnFirstSceneReady threw an Exception:"));
            }

            return !FirstSceneReadyFailed;
        }

        /// <summary>
        /// Override this method to be called when the first <see cref="Scene"/> has been <see cref="SceneManager.sceneLoaded">loaded</see>.<br/>
        /// This is the primary method for patching used by Unity Mods as basic facilities of the game
        /// are ready to use, while most other code hasn't been run yet and breaking Unity engine methods is avoided.<br/>
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
        /// <param name="scene">The first <see cref="Scene"/> that was loaded.</param>
        /// <returns>Whether the patching was successful.</returns>
        protected virtual bool OnFirstSceneReady(Scene scene) => base.OnLoaded();

        /// <remarks>
        /// Override this method if you need to patch something involved in the initialization of the game.
        /// This must not touch Unity engine methods, or things will break.<br/>
        /// For UnityMonkeys, the default behavior of<see cref="Monkey{TMonkey}.OnLoaded">OnLoaded</see>()
        /// is moved to <see cref="OnFirstSceneReady">OnFirstSceneReady</see>().
        /// <para/>
        /// Strongly consider also overriding <see cref="OnFirstSceneReady">OnFirstSceneReady</see>() if you override this method.<br/>
        /// Otherwise your patches will be applied twice, if you're using <c>[<see cref="HarmonyPatchCategory"/>(nameof(MyPatcher))]</c> attributes.
        /// <para/>
        /// <i>By default:</i> Doesn't do anything except return <c>true</c>.
        /// </remarks>
        /// <inheritdoc/>
        protected override bool OnLoaded() => true;
    }

    internal interface IUnityMonkeyInternal : IUnityMonkey
    {
        /// <summary>
        /// Call when the first <see cref="Scene"/> has been <see cref="SceneManager.sceneLoaded">loaded</see>.
        /// </summary>
        /// <param name="scene">The first <see cref="Scene"/> that was loaded.</param>
        /// <returns>Whether the patching was successful.</returns>
        public bool FirstSceneReady(Scene scene);
    }
}
