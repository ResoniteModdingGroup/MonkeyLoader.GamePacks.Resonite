using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Contains methods to update Resonite's loading progress indicator with custom phases.
    /// </summary>
    public interface ILoadProgressIndicator
    {
        /// <summary>
        /// Gets whether the progress indicator is available,
        /// determining the availability of these methods and properties.
        /// </summary>
        public bool Available { get; }

        /// <summary>
        /// Gets the index of the current fixed phase, if the progress indicator is <see cref="Available">available</see>.
        /// </summary>
        public int? FixedPhaseIndex { get; }

        /// <summary>
        /// Gets or sets the number of fixed phases, if the progress indicator is <see cref="Available">available</see>.
        /// </summary>
        public int? TotalFixedPhaseCount { get; }

        /// <summary>
        /// Increments the <see cref="TotalFixedPhaseCount">TotalFixedPhaseCount</see>
        /// by <paramref name="count"/> to make space for additional phases,
        /// if the progress indicator is <see cref="Available">available</see>.
        /// </summary>
        /// <remarks>
        /// Should be used as early as possible, to make sure the progress bar doesn't go backwards.
        /// </remarks>
        /// <returns><c>true</c> if the count was incremented successfully, otherwise <c>false</c>.</returns>
        public bool AddFixedPhases(int count);

        /// <summary>
        /// Sets the fixed phase name, if the progress indicator is <see cref="Available">available</see>.
        /// </summary>
        /// <param name="phase">The name of the phase.</param>
        public bool SetFixedPhase(string? phase);

        /// <summary>
        /// Sets the subphase, if the progress indicator is <see cref="Available">available</see>.
        /// </summary>
        /// <param name="subphase">The name of the subphase.</param>
        public bool SetSubphase(string? subphase);
    }
}