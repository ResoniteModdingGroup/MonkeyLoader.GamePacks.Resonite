using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Patching;
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
    [HarmonyPatchCategory(nameof(LoadProgressIndicator))]
    [HarmonyPatch(typeof(EngineLoadProgress), nameof(EngineLoadProgress.Awake))]
    public class LoadProgressIndicator : Monkey<LoadProgressIndicator>
    {
        private static readonly Stack<string> _phases = new();

        private static EngineLoadProgress? _loadProgress;

        private static bool failed;

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
        /// Gets the index of the current fixed phase if the progress indicator is <see cref="Available">available</see>.
        /// </summary>
        public static int? FixedPhaseIndex => Available ? _loadProgress.FixedPhaseIndex : null;

        /// <summary>
        /// Gets the number of fixed phases if the progress indicator is <see cref="Available">available</see>.
        /// </summary>
        public static int? TotalFixedPhaseCount => Available ? _loadProgress.TotalFixedPhaseCount : null;

        /// <inheritdoc/>
        public override string Name { get; } = "Load Progress Indicator";

        //private static bool isHeadless => Type.GetType("FrooxEngine.Headless.HeadlessCommands, Resonite") != null;

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
        /// Increments the <see cref="EngineLoadProgress.TotalFixedPhaseCount"/> to make space for an additional phase.
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
            return true;
        }

        /// <summary>
        /// Increments the <see cref="EngineLoadProgress.FixedPhaseIndex"/> to advance the progress bar.
        /// </summary>
        /// <returns><c>true</c> if the index was incremented successfully, otherwise <c>false</c>.</returns>
        public static bool AdvanceFixedPhase()
        {
            if (!Available)
                return false;

            ++_loadProgress.FixedPhaseIndex;
            return true;
        }

        /// <summary>
        /// Enters a nested custom phase, saving the previous one.
        /// </summary>
        /// <returns><c>true</c> if the phase was changed successfully, otherwise <c>false</c>.</returns>
        public static bool EnterCustomPhase(string phase)
        {
            if (!Available)
                return false;

            _phases.Push(_loadProgress._showSubphase);
            _loadProgress._showSubphase = phase;

            return true;
        }

        /// <summary>
        /// Exits the current custom phase, returning to the previous one.
        /// </summary>
        /// <returns><c>true</c> if the phase was changed successfully, otherwise <c>false</c>.</returns>
        public static bool ExitCustomPhase()
        {
            if (!Available || _phases.Count == 0)
                return false;

            _loadProgress._showSubphase = _phases.Pop();

            return true;
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

        /// <inheritdoc/>
        protected override IEnumerable<IFeaturePatch> GetFeaturePatches()
            => Enumerable.Empty<IFeaturePatch>();

        private static void AwakePrefix(EngineLoadProgress __instance)
            => _loadProgress = __instance;
    }
}