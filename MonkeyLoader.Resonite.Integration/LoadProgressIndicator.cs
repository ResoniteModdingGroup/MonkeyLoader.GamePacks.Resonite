using HarmonyLib;
using MonkeyLoader.Patching;
using MonkeyLoader.Unity;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Contains methods to update Resonite's loading progress indicator with custom phases.
    /// </summary>
    [HarmonyPatch(typeof(EngineLoadProgress))]
    [HarmonyPatchCategory(nameof(LoadProgressIndicator))]
    public sealed class LoadProgressIndicator : UnityMonkey<LoadProgressIndicator>
    {
        private static EngineLoadProgress? _loadProgress;

        private static string? _phase;

        /// <summary>
        /// Gets whether the progress indicator is available,
        /// determining the availability of the methods and properties of this class.
        /// </summary>
        [MemberNotNullWhen(true, nameof(_loadProgress),
            nameof(FixedPhaseIndex), nameof(TotalFixedPhaseCount))]
        public static bool Available
        {
            get
            {
                if (_loadProgress == null)
                {
                    // Clear reference to UnityObject when it compares as null
                    _loadProgress = null;
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
        public static int? FixedPhaseIndex => _loadProgress?.FixedPhaseIndex;

        /// <summary>
        /// Gets the number of fixed phases, if the progress indicator is <see cref="Available">available</see>.
        /// </summary>
        public static int? TotalFixedPhaseCount => _loadProgress?.TotalFixedPhaseCount;

        /// <summary>
        /// Increments the <see cref="EngineLoadProgress.TotalFixedPhaseCount"/> to make space for an additional phase,
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

            ++_loadProgress.TotalFixedPhaseCount;
            Trace(() => "Incremented EngineLoadProgress.TotalFixedPhaseCount by 1.");

            return true;
        }

        /// <summary>
        /// Increments the <see cref="EngineLoadProgress.TotalFixedPhaseCount"/> by <paramref name="count"/> to make space for additional phases,
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

            _loadProgress.TotalFixedPhaseCount += count;
            Trace(() => $"Incremented EngineLoadProgress.TotalFixedPhaseCount by {count}.");

            return true;
        }

        /// <summary>
        /// Increments the <see cref="EngineLoadProgress.FixedPhaseIndex"/> and sets the fixed phase to advance the progress bar,
        /// if the progress indicator is <see cref="Available">available</see>.
        /// </summary>
        /// <param name="phase">The name of the phase to advance to.</param>
        /// <returns><c>true</c> if the phase was advanced successfully, otherwise <c>false</c>.</returns>
        public static bool AdvanceFixedPhase(string phase)
        {
            if (!Available)
                return false;

            _loadProgress.SetFixedPhase(phase);
            Trace(() => $"Advanced EngineLoadProgress phase to: {phase}");

            return true;
        }

        /// <summary>
        /// Unsets the subphase, if the progress indicator is <see cref="Available">available</see>.
        /// </summary>
        /// <returns><c>true</c> if the subphase was changed successfully, otherwise <c>false</c>.</returns>
        public static bool ExitSubphase()
        {
            if (!Available)
                return false;

            _loadProgress.SetSubphase(null);
            Trace(() => "Reset the EngineLoadProgress subphase.");

            return true;
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

            _loadProgress.SetSubphase(subphase);

            Trace(() => $"Set EngineLoadProgress subphase to: {subphase}");

            return true;
        }

        /// <inheritdoc/>
        protected override IEnumerable<IFeaturePatch> GetFeaturePatches()
            => Enumerable.Empty<IFeaturePatch>();

        /// <inheritdoc/>
        protected override bool OnFirstSceneReady(Scene scene)
        {
            _loadProgress = scene.GetRootGameObjects()
                .Select(g => g.GetComponentInChildren<EngineLoadProgress>())
                .FirstOrDefault(elp => elp != null);

            Info(() => $"Hooked EngineLoadProgress indicator: {Available}");

            return base.OnFirstSceneReady(scene);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(EngineLoadProgress.SetFixedPhase))]
        private static void SetFixedPhasedPostfix(EngineLoadProgress __instance, string phase)
        {
            _phase = phase;
            __instance._showSubphase = phase;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(EngineLoadProgress.SetSubphase))]
        private static void SetSubphasePostfix(EngineLoadProgress __instance, string subphase)
            => __instance._showSubphase = $"{_phase}   {subphase}";
    }
}