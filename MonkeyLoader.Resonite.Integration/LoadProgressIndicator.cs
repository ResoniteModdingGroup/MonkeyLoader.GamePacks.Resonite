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
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Contains methods to update Resonite's loading progress indicator with custom phases.
    /// </summary>
    [HarmonyPatch(typeof(EngineLoadProgress))]
    [HarmonyPatchCategory(nameof(LoadProgressIndicator))]
    public class LoadProgressIndicator : UnityMonkey<LoadProgressIndicator>
    {
        private static EngineLoadProgress? _loadProgress;

        //private static bool failed;

        private static string? _phase;

        /// <summary>
        /// Gets whether the progress indicator is available.
        /// </summary>
        [MemberNotNullWhen(true, nameof(_loadProgress),
            nameof(FixedPhaseIndex), nameof(TotalFixedPhaseCount))]
        public static bool Available
        {
            get
            {
                if (_loadProgress == null)
                {
                    _loadProgress = null;
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Gets the index of the current fixed phase, if the progress indicator is <see cref="Available">available</see>.
        /// </summary>
        public static int? FixedPhaseIndex => Available ? _loadProgress.FixedPhaseIndex : null;

        /// <summary>
        /// Gets the number of fixed phases, if the progress indicator is <see cref="Available">available</see>.
        /// </summary>
        public static int? TotalFixedPhaseCount => Available ? _loadProgress.TotalFixedPhaseCount : null;

        /// <inheritdoc/>
        public override string Name { get; } = "Load Progress Indicator";

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

        //private static FieldInfo? ShowSubphase
        //{
        //    get
        //    {
        //        if (_showSubphase is null)
        //        {
        //            try
        //            {
        //                _showSubphase = typeof(EngineLoadProgress).GetField("_showSubphase", BindingFlags.NonPublic | BindingFlags.Instance);
        //            }
        //            catch (Exception ex)
        //            {
        //                if (!failed)
        //                {
        //                    Logger.WarnInternal("_showSubphase not found: " + ex.ToString());
        //                }
        //                failed = true;
        //            }
        //        }
        //        return _showSubphase;
        //    }
        //}
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

        //private static bool isHeadless => Type.GetType("FrooxEngine.Headless.HeadlessCommands, Resonite") != null;
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
        /// Returns the full hierarchy name of the game object.
        /// </summary>
        /// <param name="gameObject">The game object.</param>
        public static IEnumerable<Func<object>> DebugHierarchy(GameObject gameObject)
        {
            do
            {
                var transform = gameObject.transform;
                yield return () => $"{gameObject.name} (T: {transform.localPosition}; S: {transform.localScale}; R: {transform.rotation.eulerAngles})";

                gameObject = gameObject.transform.parent.gameObject;
            }
            while (gameObject != null);
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

            lock (_loadProgress)
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

            lock (__instance)
                __instance.SetSubphase(phase);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(EngineLoadProgress.SetSubphase))]
        private static void SetSubphasePostfix(EngineLoadProgress __instance, string subphase)
        {
            lock (__instance)
                __instance.SetSubphase($"{_phase}   {subphase}");
        }

        // Returned true means success, false means something went wrong.
        //public static bool SetCustom(string text)
        //{
        //    if (ModLoaderConfiguration.Get().HideVisuals) { return true; }
        //    if (!isHeadless)
        //    {
        //        if (ShowSubphase != null)
        //        {
        //            ShowSubphase.SetValue(Engine.Current.InitProgress, text);
        //        }
        //        return true;
        //    }
        //    return false;
        //}
    }
}