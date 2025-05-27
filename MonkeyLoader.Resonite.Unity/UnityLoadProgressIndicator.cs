using HarmonyLib;
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
    /// Contains methods to update Resonite's Unity loading progress indicator with custom phases.
    /// </summary>
    [HarmonyPatch(typeof(EngineLoadProgress))]
    [HarmonyPatchCategory(nameof(UnityLoadProgressIndicator))]
    internal sealed class UnityLoadProgressIndicator : UnityMonkey<UnityLoadProgressIndicator>, ILoadProgressIndicator
    {
        private static EngineLoadProgress? _loadProgress;

        private static string? _phase;

        /// <inheritdoc/>
        [MemberNotNullWhen(true, nameof(_loadProgress),
            nameof(FixedPhaseIndex), nameof(TotalFixedPhaseCount))]
        public bool Available
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

        /// <inheritdoc/>
        public int? FixedPhaseIndex => _loadProgress?.FixedPhaseIndex;

        /// <inheritdoc/>
        public int? TotalFixedPhaseCount => _loadProgress?.TotalFixedPhaseCount;

        /// <summary>
        /// Increments the <see cref="EngineLoadProgress.TotalFixedPhaseCount"/> by <paramref name="count"/> to make space for additional phases,
        /// if the progress indicator is <see cref="Available">available</see>.
        /// </summary>
        /// <remarks>
        /// Should be used as early as possible, to make sure the progress bar doesn't go backwards.
        /// </remarks>
        /// <returns><c>true</c> if the count was incremented successfully, otherwise <c>false</c>.</returns>
        public bool AddFixedPhases(int count)
        {
            if (!Available)
                return false;

            _loadProgress.TotalFixedPhaseCount += count;
            Logger.Trace(() => $"Incremented EngineLoadProgress.TotalFixedPhaseCount by {count}.");

            return true;
        }

        /// <summary>
        /// Sets the subphase, if the progress indicator is <see cref="Available">available</see>.
        /// </summary>
        /// <param name="subphase">The name of the subphase.</param>
        /// <returns><c>true</c> if the subphase was changed successfully, otherwise <c>false</c>.</returns>
        public bool SetSubphase(string? subphase)
        {
            if (!Available)
                return false;

            _loadProgress.SetSubphase(subphase);

            return true;
        }

        /// <inheritdoc/>
        protected override bool OnFirstSceneReady(Scene scene)
        {
            if (!LoadingConfig.Instance.HijackLoadProgressIndicator)
                return true;

            _loadProgress = scene.GetRootGameObjects()
                .Select(g => g.GetComponentInChildren<EngineLoadProgress>())
                .FirstOrDefault(elp => elp != null);

            Logger.Info(() => $"Hooked EngineLoadProgress indicator: {Available}");

            if (Available)
                LoadProgressReporter.LoadProgressIndicator = this;

            return base.OnFirstSceneReady(scene);
        }

        /// <inheritdoc/>
        public bool SetFixedPhase(string? phase)
        {
            if (!Available)
                return false;

            _loadProgress.SetFixedPhase(phase);

            return true;
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
            => __instance._showSubphase = subphase is null ? _phase : $"{_phase}   {subphase}";
    }
}