using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace MonkeyLoader.Patching
{
    /// <summary>
    /// Represents the base class for patchers that run after a game's assemblies have been loaded.<br/>
    /// All mod defined derivatives must derive from <see cref="Monkey{TMonkey}"/>,
    /// <see cref="ConfiguredMonkey{TMonkey, TConfigSection}"/>, or another class derived from it.
    /// </summary>
    /// <remarks>
    /// Game assemblies and their types can be directly referenced from these.
    /// </remarks>
    /// <inheritdoc/>
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
            ThrowIfRan();
            Ran = true;

            Trace(() => "Running Monkey's OnLoaded!");

            try
            {
                if (!OnLoaded())
                {
                    Failed = true;
                    Warn(() => "OnLoaded failed!");
                }
            }
            catch (Exception ex)
            {
                Failed = true;
                Error(() => ex.Format("OnLoaded threw an Exception:"));
            }

            return !Failed;
        }

        /// <summary>
        /// Called right after the game tooling packs and all the game's assemblies have been loaded.<br/>
        /// Use this to apply any patching and return <c>true</c> if it was successful.
        /// </summary>
        /// <remarks>
        /// <i>By default:</i> Applies the <see cref="Harmony"/> patches of the
        /// <see cref="Harmony.PatchCategory(string)">category</see> with this patcher's type's name.<br/>
        /// Easy to apply by using <c>[<see cref="HarmonyPatchCategory"/>(nameof(MyPatcher))]</c> attribute.
        /// </remarks>
        /// <returns>Whether the patching was successful.</returns>
        protected virtual bool OnLoaded()
        {
            var type = GetType();
            Harmony.PatchCategory(type.Assembly, type.Name);

            return true;
        }
    }
}