using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Meta
{
    /// <summary>
    /// Interface for everything that can be run.
    /// </summary>
    public interface IRun
    {
        /// <summary>
        /// Gets whether this object's <see cref="Run">Run</see>() method failed when it was called.
        /// </summary>
        public bool Failed { get; }

        /// <summary>
        /// Gets whether this object's <see cref="Run">Run</see>() method has been called.
        /// </summary>
        public bool Ran { get; }

        /// <summary>
        /// Runs this object to activate its effects.<br/>
        /// Must only be called once.
        /// </summary>
        /// <returns>Whether it ran successfully.</returns>
        /// <exception cref="InvalidOperationException">If it gets called more than once.</exception>
        public bool Run();
    }
}