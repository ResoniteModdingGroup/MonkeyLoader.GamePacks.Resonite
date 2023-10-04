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
    public abstract class ResoniteMonkey : Monkey
    {
        /// <summary>
        /// Called right after <see cref="Engine"/>.<see cref="Engine.Initialize">Initialize</see> is done.
        /// </summary>
        protected internal virtual void OnEngineInitialized()
        { }

        /// <summary>
        /// Called when the <see cref="Engine"/> is shutting down.
        /// </summary>
        protected internal virtual void OnEngineShutdown()
        { }
    }
}