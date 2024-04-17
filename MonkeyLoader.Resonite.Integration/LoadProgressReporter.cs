using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Contains methods to update Resonite's loading progress indicator with custom phases.
    /// </summary>
    public static class LoadProgressReporter
    {
        /// <summary>
        /// Gets or sets the concrete <see cref="ILoadProgressIndicator"/>
        /// implementation used to report the load progress of mods and their monkeys.
        /// </summary>
        public static ILoadProgressIndicator? LoadProgressIndicator { get; set; }

        /// <summary>
        /// Gets whether the progress indicator is available,
        /// determining the availability of the methods and properties of this class.
        /// </summary>
        [MemberNotNullWhen(true, nameof(LoadProgressIndicator),
            nameof(FixedPhaseIndex), nameof(TotalFixedPhaseCount))]
        public static bool Available
        {
            get
            {
                if (LoadProgressIndicator == null || !LoadProgressIndicator.Available)
                {
                    // Clear reference to indicator when it compares as null
                    LoadProgressIndicator = null;
                    return false;
                }

#pragma warning disable CS8775 // Member must have a non-null value when exiting in some condition.
                return true;
#pragma warning restore CS8775 // Member must have a non-null value when exiting in some condition.
            }
        }

        /// <summary>
        /// Gets the index of the current fixed phase, if the progress indicator is <see cref="Available">available</see>.
        /// </summary>
        public static int? FixedPhaseIndex => LoadProgressIndicator?.FixedPhaseIndex;

        /// <summary>
        /// Gets the number of fixed phases, if the progress indicator is <see cref="Available">available</see>.
        /// </summary>
        public static int? TotalFixedPhaseCount => LoadProgressIndicator?.TotalFixedPhaseCount;

        /// <summary>
        /// Increments the <see cref="TotalFixedPhaseCount"/> to make space for an additional phase,
        /// if the progress indicator is <see cref="Available">available</see>..
        /// </summary>
        /// <remarks>
        /// Should be used as early as possible, to make sure the progress bar doesn't go backwards.
        /// </remarks>
        /// <returns><c>true</c> if the count was incremented successfully, otherwise <c>false</c>.</returns>
        public static bool AddFixedPhase()
        {
            if (!Available)
                return false;

            return LoadProgressIndicator.AddFixedPhases(1);
        }

        /// <summary>
        /// Increments the <see cref="TotalFixedPhaseCount"/> by <paramref name="count"/> to make space for additional phases,
        /// if the progress indicator is <see cref="Available">available</see>.
        /// </summary>
        /// <remarks>
        /// Should be used as early as possible, to make sure the progress bar doesn't go backwards.
        /// </remarks>
        /// <returns><c>true</c> if the count was incremented successfully, otherwise <c>false</c>.</returns>
        public static bool AddFixedPhases(int count)
        {
            if (!Available)
                return false;

            return LoadProgressIndicator.AddFixedPhases(count);
        }

        /// <summary>
        /// Increments the <see cref="FixedPhaseIndex"/> and sets the fixed phase to advance the progress bar,
        /// if the progress indicator is <see cref="Available">available</see>.
        /// </summary>
        /// <param name="phase">The name of the phase to advance to.</param>
        /// <returns><c>true</c> if the phase was advanced successfully, otherwise <c>false</c>.</returns>
        public static bool AdvanceFixedPhase(string phase)
        {
            if (!Available)
                return false;

            return LoadProgressIndicator.SetFixedPhase(phase);
        }

        /// <summary>
        /// Unsets the subphase, if the progress indicator is <see cref="Available">available</see>.
        /// </summary>
        /// <returns><c>true</c> if the subphase was changed successfully, otherwise <c>false</c>.</returns>
        public static bool ExitSubphase()
        {
            if (!Available)
                return false;

            return LoadProgressIndicator.SetSubphase(null);
        }

        /// <summary>
        /// Sets the subphase, if the progress indicator is <see cref="Available">available</see>.
        /// </summary>
        /// <param name="subphase">The name of the subphase.</param>
        /// <returns><c>true</c> if the subphase was changed successfully, otherwise <c>false</c>.</returns>
        public static bool SetSubphase(string subphase)
        {
            if (!Available)
                return false;

            return LoadProgressIndicator.SetSubphase(subphase);
        }
    }
}