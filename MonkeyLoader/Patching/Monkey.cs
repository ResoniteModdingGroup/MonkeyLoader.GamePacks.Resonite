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
                if (!onLoaded())
                {
                    Failed = true;
                    Logger.Warn(() => "OnLoaded failed!");
                }
            }
            catch (Exception ex)
            {
                Failed = true;
                Logger.Error(() => ex.Format("OnLoaded threw an Exception:"));
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