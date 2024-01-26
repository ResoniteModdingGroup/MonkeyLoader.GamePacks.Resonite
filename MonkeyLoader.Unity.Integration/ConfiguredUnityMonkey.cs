using HarmonyLib;
using MonkeyLoader.Configuration;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Unity
{
    /// <summary>
    /// Represents the base class for patchers that run after all Unity and game assemblies have been loaded,
    /// while using a <c><typeparamref name="TConfigSection"/> <see cref="ConfigSection">ConfigSection</see></c>.
    /// </summary>
    /// <inheritdoc/>
    /// <typeparam name="TMonkey">The type of the actual patcher.</typeparam>
    /// <typeparam name="TConfigSection">The type of the config section to load.</typeparam>
    public abstract class ConfiguredUnityMonkey<TMonkey, TConfigSection> : UnityMonkey<TMonkey>
        where TMonkey : ConfiguredUnityMonkey<TMonkey, TConfigSection>, new()
        where TConfigSection : ConfigSection, new()
    {
        /// <summary>
        /// Gets the loaded config section for this patcher after it has been <see cref="MonkeyBase.Run">run</see>.
        /// </summary>
        protected static TConfigSection ConfigSection { get; private set; } = null!;

        /// <summary>
        /// Allows creating only a single <typeparamref name="TMonkey"/> instance.
        /// </summary>
        protected ConfiguredUnityMonkey()
        { }

        /// <remarks>
        /// Override this method if you need to patch something involved in the initialization of the game.<br/>
        /// For UnityMonkeys, the default behavior of<see cref="Monkey{TMonkey}.OnLoaded">OnLoaded</see>()
        /// is moved to <see cref="UnityMonkey{TMonkey}.OnFirstSceneReady">OnFirstSceneReady</see>().
        /// <para/>
        /// Strongly consider also overriding <see cref="UnityMonkey{TMonkey}.OnFirstSceneReady">OnFirstSceneReady</see>() if you override this method.<br/>
        /// Otherwise your patches will be applied twice, if you're using <c>[<see cref="HarmonyPatchCategory"/>(nameof(MyPatcher))]</c> attributes.
        /// <para/>
        /// <i>By default:</i> Loads this patcher's <c><typeparamref name="TConfigSection"/>
        /// <see cref="ConfigSection">ConfigSection</see></c> and returns <c>true</c>.
        /// </remarks>
        /// <inheritdoc/>
        protected override bool OnLoaded()
        {
            ConfigSection = Config.LoadSection<TConfigSection>();

            return base.OnLoaded();
        }
    }
}