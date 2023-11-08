using HarmonyLib;
using MonkeyLoader.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Patching
{
    /// <summary>
    /// Represents a base class for patchers that run after a game's assemblies have been loaded,
    /// while using a <c><typeparamref name="TConfigSection"/> <see cref="ConfigSection">ConfigSection</see></c>.<br/>
    /// All mod defined derivatives must derive from <see cref="Monkey{TMonkey}"/>,
    /// <see cref="ConfiguredMonkey{TMonkey, TConfigSection}"/>, or another class derived from it.
    /// </summary>
    /// <inheritdoc/>
    /// <typeparam name="TMonkey">The type of the actual patcher.</typeparam>
    /// <typeparam name="TConfigSection">The type of the config section to load.</typeparam>
    public abstract class ConfiguredMonkey<TMonkey, TConfigSection> : Monkey<TMonkey>
        where TMonkey : ConfiguredMonkey<TMonkey, TConfigSection>, new()
        where TConfigSection : ConfigSection, new()
    {
        /// <summary>
        /// Gets the loaded config section for this patcher after it has been <see cref="MonkeyBase.Run">run</see>.
        /// </summary>
        protected static TConfigSection ConfigSection { get; private set; } = null!;

        /// <summary>
        /// Allows creating only a single <typeparamref name="TMonkey"/> instance.
        /// </summary>
        protected ConfiguredMonkey()
        { }

        /// <remarks>
        /// <i>By default:</i> Loads this patcher's <c><typeparamref name="TConfigSection"/> <see cref="ConfigSection">ConfigSection</see></c>.<br/>
        /// Then applies the <see cref="Harmony"/> patches of the
        /// <see cref="Harmony.PatchCategory(string)">category</see> with this patcher's type's name.<br/>
        /// Easy to apply by using <c>[<see cref="HarmonyPatchCategory"/>(nameof(MyPatcher))]</c> attribute.
        /// </remarks>
        /// <inheritdoc/>
        protected override bool OnLoaded()
        {
            ConfigSection = Config.LoadSection<TConfigSection>();

            return base.OnLoaded();
        }
    }
}