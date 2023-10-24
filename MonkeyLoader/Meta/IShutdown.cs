using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Meta
{
    /// <summary>
    /// Interface for everything that can be shut down.
    /// </summary>
    public interface IShutdown
    {
        /// <summary>
        /// Gets whether this object's <see cref="Shutdown">Shutdown</see>() failed when it was called.
        /// </summary>
        public bool ShutdownFailed { get; }

        /// <summary>
        /// Gets whether this object's <see cref="Shutdown">Shutdown</see>() method has been called.
        /// </summary>
        public bool ShutdownRan { get; }

        /// <summary>
        /// Lets this object cleanup and shutdown.<br/>
        /// Must only be called once.
        /// </summary>
        /// <returns>Whether it ran successfully.</returns>
        /// <exception cref="InvalidOperationException">If it gets called more than once.</exception>
        public bool Shutdown();
    }
}