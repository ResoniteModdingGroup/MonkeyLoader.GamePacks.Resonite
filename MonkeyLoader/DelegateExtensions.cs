using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader
{
    internal static class DelegateExtensions
    {
        /// <summary>
        /// Individually calls every method from a <see cref="Delegate"/>'s invocation list in a try-catch-block,
        /// collecting any <see cref="Exception"/>s into an <see cref="AggregateException"/>.
        /// </summary>
        /// <param name="del">The delegate to safely invoke.</param>
        /// <param name="args">The arguments for the invocation.</param>
        /// <exception cref="AggregateException">Thrown when any invoked methods threw. Contains all nested Exceptions.</exception>
        // Adapted from the NeosModLoader project.
        internal static void SafeInvoke(this Delegate del, params object[] args)
        {
            var exceptions = new List<Exception>();

            foreach (var handler in del.GetInvocationList())
            {
                try
                {
                    handler.Method.Invoke(handler.Target, args);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}