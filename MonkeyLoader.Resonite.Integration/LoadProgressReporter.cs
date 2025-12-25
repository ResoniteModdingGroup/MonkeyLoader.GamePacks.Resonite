using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Contains methods to update Resonite's loading progress indicator with custom phases.
    /// </summary>
    // This class should not directly reference the RendererInitProgressWrapper type in code to not make issues on headlesses
    [HarmonyPatch]
    [HarmonyPatchCategory(nameof(LoadProgressReporter))]
    public sealed class LoadProgressReporter : Monkey<LoadProgressReporter>
    {
        private const string RendererProgressWrapperTypeName = "FrooxEngine.RendererInitProgressWrapper, FrooxEngine";
        private static bool _advancedToReady;
        private static bool _overrideProceedToReady;
        private static int? _totalFixedPhaseCount;

        /// <summary>
        /// Gets whether the progress indicator is available,
        /// determining the availability of the methods and properties of this class.
        /// </summary>
        [MemberNotNullWhen(true, nameof(LoadProgressIndicator), nameof(FixedPhaseIndex), nameof(CanProgressToReady))]
        public static bool Available => IsActive && !_advancedToReady && LoadProgressIndicator is not null;

        /// <summary>
        /// Gets the index of the current fixed phase, if the progress indicator is <see cref="Available">available</see>.
        /// </summary>
        public static int? FixedPhaseIndex => LoadProgressIndicator?.FixedPhaseIndex;

        /// <summary>
        /// Gets whether the load progress reporter will be active for the current launch.
        /// </summary>
        /// <remarks>
        /// This isolates it against changes of the <see cref="LoadingConfig.HijackLoadProgressIndicator"/>
        /// setting during the launch.
        /// </remarks>
        public static bool IsActive { get; private set; }

        /// <summary>
        /// Gets whether the renderer's progressbar style init progress is available.<br/>
        /// If not, there is no "done" maximum progress to set.
        /// </summary>
        [MemberNotNullWhen(true, nameof(OriginalTotalFixedPhaseCount), nameof(TotalFixedPhaseCount))]
        public static bool IsRendererInitProgressWrapperAvailable => OriginalTotalFixedPhaseCount.HasValue;

        /// <summary>
        /// Gets the original total fixed phase count of the <see cref="RendererInitProgressWrapper"/>,
        /// if it is <see cref="IsRendererInitProgressWrapperAvailable">available</see>.
        /// </summary>
        public static int? OriginalTotalFixedPhaseCount { get; set; }

        /// <summary>
        /// Gets the number of fixed phases, if the <see cref="RendererInitProgressWrapper"/> is available.
        /// </summary>
        public static int? TotalFixedPhaseCount
        {
            get => IsActive ? _totalFixedPhaseCount : OriginalTotalFixedPhaseCount;
            private set => _totalFixedPhaseCount = value;
        }

        private static bool? CanProgressToReady => !Available ? null : FixedPhaseIndex >= TotalFixedPhaseCount || _overrideProceedToReady;
        private static float InternalTotalFixedPhaseCount => _totalFixedPhaseCount!.Value;

        /// <summary>
        /// Gets the <see cref="IEngineInitProgress"/> implementation used by the <see cref="Engine"/>.<br/>
        /// Used in this class to report the load progress of mods and their monkeys.
        /// </summary>
        private static IEngineInitProgress? LoadProgressIndicator => Engine.Current?.InitProgress;

        /// <summary>
        /// Increments the <see cref="TotalFixedPhaseCount"/> to make space for an additional phase,
        /// if the progress indicator is <see cref="Available">available</see>..
        /// </summary>
        /// <remarks>
        /// Should be used as early as possible, to make sure the progress bar doesn't go backwards.
        /// </remarks>
        /// <returns><c>true</c> if the count was incremented successfully, otherwise <c>false</c>.</returns>
        public static bool AddFixedPhase()
            => AddFixedPhases(1);

        /// <summary>
        /// Increments the <see cref="TotalFixedPhaseCount"/> by <paramref name="count"/>
        /// to make space for additional phases, if the count has a value.
        /// </summary>
        /// <remarks>
        /// Should be used as early as possible, to make sure the progress bar doesn't go backwards.
        /// </remarks>
        /// <returns><c>true</c> if the count was incremented successfully, otherwise <c>false</c>.</returns>
        public static bool AddFixedPhases(int count)
        {
            if (!IsActive || !IsRendererInitProgressWrapperAvailable)
                return false;

            TotalFixedPhaseCount += count;
            return true;
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
            {
                if (_advancedToReady)
                    Logger.Warn(() => $"Tried to set fixed phase [{phase}] after engine was ready!{Environment.NewLine}{Environment.StackTrace}");

                return false;
            }

            LoadProgressIndicator.SetFixedPhase(phase);
            return true;
        }

        /// <summary>
        /// Unsets the subphase, if the progress indicator is <see cref="Available">available</see>.
        /// </summary>
        /// <returns><c>true</c> if the subphase was changed successfully, otherwise <c>false</c>.</returns>
        [Obsolete("Do not exit subphases, just set the next one.")]
        public static bool ExitSubphase()
        {
            if (!Available)
            {
                if (_advancedToReady)
                    Logger.Warn(() => $"Tried to exit subphase after engine was ready!{Environment.NewLine}{Environment.StackTrace}");

                return false;
            }

            LoadProgressIndicator.SetSubphase(null!);
            return true;
        }

        /// <summary>
        /// <see cref="Task.Run(Action)">Runs</see> the given <paramref name="action"/>
        /// and waits for at least the specified time before completing,
        /// <i>if</i> the progress indicator is <see cref="Available">available</see> and
        /// <see cref="LoadingConfig.PrettySplashProgress">pretty splash progress</see> is enabled.
        /// </summary>
        /// <param name="milliseconds">How long to wait under the right conditions, in milliseconds.</param>
        /// <param name="action">The action to <see cref="Task.Run(Action)">run</see>.</param>
        /// <param name="cancellationToken">The cancellation token that will be checked prior to completing the returned task.</param>
        /// <returns>A task that represents the completion of the <paramref name="action"/> and optional <paramref name="milliseconds"/>-long wait.</returns>
        public static Task RunForPrettySplashAsync(int milliseconds, Action action, CancellationToken cancellationToken = default)
            => RunForPrettySplashAsync(milliseconds, Task.Run(action, cancellationToken), cancellationToken);

        /// <summary>
        /// <see cref="Task.Run{TResult}(Func{TResult}, CancellationToken)">Runs</see> the given <paramref name="func"/>
        /// and waits for at least the specified time before completing,
        /// <i>if</i> the progress indicator is <see cref="Available">available</see> and
        /// <see cref="LoadingConfig.PrettySplashProgress">pretty splash progress</see> is enabled.
        /// </summary>
        /// <param name="milliseconds">How long to wait under the right conditions, in milliseconds.</param>
        /// <param name="func">The function to <see cref="Task.Run{TResult}(Func{TResult}, CancellationToken)">run</see>.</param>
        /// <param name="cancellationToken">The cancellation token that will be checked prior to completing the returned task.</param>
        /// <returns>A task that represents the completion of the <paramref name="func"/> and optional <paramref name="milliseconds"/>-long wait.</returns>
        public static Task<T> RunForPrettySplashAsync<T>(int milliseconds, Func<T> func, CancellationToken cancellationToken = default)
            => RunForPrettySplashAsync(milliseconds, Task.Run(func, cancellationToken), cancellationToken);

        /// <summary>
        /// <see cref="Task.Run(Func{Task}, CancellationToken)">Runs</see> the given <paramref name="taskFunc"/>
        /// and waits for at least the specified time before completing,
        /// <i>if</i> the progress indicator is <see cref="Available">available</see> and
        /// <see cref="LoadingConfig.PrettySplashProgress">pretty splash progress</see> is enabled.
        /// </summary>
        /// <param name="milliseconds">How long to wait under the right conditions, in milliseconds.</param>
        /// <param name="taskFunc">The <see cref="Task"/>-function to <see cref="Task.Run(Func{Task}, CancellationToken)">run</see>.</param>
        /// <param name="cancellationToken">The cancellation token that will be checked prior to completing the returned task.</param>
        /// <returns>A task that represents the completion of the <paramref name="taskFunc"/> and optional <paramref name="milliseconds"/>-long wait.</returns>
        public static Task RunForPrettySplashAsync(int milliseconds, Func<Task> taskFunc, CancellationToken cancellationToken = default)
            => RunForPrettySplashAsync(milliseconds, Task.Run(taskFunc, cancellationToken), cancellationToken);

        /// <summary>
        /// <see cref="Task.Run{TResult}(Func{Task{TResult}}, CancellationToken)">Runs</see> the given <paramref name="taskFunc"/>
        /// and waits for at least the specified time before completing,
        /// <i>if</i> the progress indicator is <see cref="Available">available</see> and
        /// <see cref="LoadingConfig.PrettySplashProgress">pretty splash progress</see> is enabled.
        /// </summary>
        /// <param name="milliseconds">How long to wait under the right conditions, in milliseconds.</param>
        /// <param name="taskFunc">The <see cref="Task"/>-function to <see cref="Task.Run{TResult}(Func{Task{TResult}}, CancellationToken)">run</see>.</param>
        /// <param name="cancellationToken">The cancellation token that will be checked prior to completing the returned task.</param>
        /// <returns>A task that represents the completion of the <paramref name="taskFunc"/> and optional <paramref name="milliseconds"/>-long wait.</returns>
        public static Task<T> RunForPrettySplashAsync<T>(int milliseconds, Func<Task<T>> taskFunc, CancellationToken cancellationToken = default)
            => RunForPrettySplashAsync(milliseconds, Task.Run(taskFunc, cancellationToken), cancellationToken);

        /// <summary>
        /// Waits for the given <paramref name="task"/> to complete,
        /// but at least for the specified time before itself completing,
        /// <i>if</i> the progress indicator is <see cref="Available">available</see> and
        /// <see cref="LoadingConfig.PrettySplashProgress">pretty splash progress</see> is enabled.
        /// </summary>
        /// <param name="milliseconds">How long to wait under the right conditions, in milliseconds.</param>
        /// <param name="task">The to wait for the completion of.</param>
        /// <param name="cancellationToken">The cancellation token that will be checked prior to completing the returned task.</param>
        /// <returns>A task that represents the completion of the <paramref name="task"/> and optional <paramref name="milliseconds"/>-long wait.</returns>
        public static async Task RunForPrettySplashAsync(int milliseconds, Task task, CancellationToken cancellationToken = default)
        {
            if (Available && LoadingConfig.Instance.PrettySplashProgress && LoadingConfig.Instance.AlwaysShowLoadingPhases)
                await Task.WhenAll(Task.Delay(milliseconds, cancellationToken), task).ConfigureAwait(false);
            else
                await task.ConfigureAwait(false);
        }

        /// <inheritdoc cref="RunForPrettySplashAsync(int, Task, CancellationToken)"/>
        public static async Task<T> RunForPrettySplashAsync<T>(int milliseconds, Task<T> task, CancellationToken cancellationToken = default)
        {
            await RunForPrettySplashAsync(milliseconds, (Task)task, cancellationToken).ConfigureAwait(false);

            return task.Result;
        }

        /// <summary>
        /// Sets the subphase, if the progress indicator is <see cref="Available">available</see>.
        /// </summary>
        /// <param name="subphase">The name of the subphase.</param>
        /// <param name="alwaysShow">Should the subphase be always shown to the user.</param>
        /// <returns><see langword="true"/> if the subphase was changed successfully; otherwise, <see langword="false"/>.</returns>
        public static bool SetSubphase(string subphase, bool alwaysShow = false)
        {
            if (!Available)
            {
                if (_advancedToReady)
                    Logger.Warn(() => $"Tried to set subphase [{subphase}] after engine was ready!{Environment.NewLine}{Environment.StackTrace}");

                return false;
            }

            LoadProgressIndicator.SetSubphase(subphase, alwaysShow);
            return true;
        }

        /// <remarks>
        /// Calls <c><see cref="SetSubphase(string, bool)">SetSubphase</see>(<paramref name="subphase"/>, <see langword="false"/>)</c>.
        /// </remarks>
        /// <inheritdoc cref="SetSubphase(string, bool)"/>
        public static bool SetSubphase(string subphase)
            => SetSubphase(subphase, false);

        internal static bool EngineReady()
        {
            if (!Available)
                return false;

            if (!CanProgressToReady.Value)
                Logger.Warn(() => $"Proceeding to Engine Ready while fixed phase progress is too low. Current: {FixedPhaseIndex} / {TotalFixedPhaseCount}");

            _overrideProceedToReady = true;
            LoadProgressIndicator.EngineReady();

            return true;
        }

        /// <inheritdoc/>
        protected override bool OnLoaded()
        {
            OriginalTotalFixedPhaseCount = (int?)AccessTools.DeclaredField($"{RendererProgressWrapperTypeName}:TOTAL_FIXED_PHASE_COUNT")?.GetValue(null);
            TotalFixedPhaseCount = OriginalTotalFixedPhaseCount;

            var success = base.OnLoaded();

            IsActive = LoadingConfig.Instance.HijackLoadProgressIndicator && OriginalTotalFixedPhaseCount.HasValue;

            return success;
        }

        [HarmonyPrefix]
        [HarmonyPatch(RendererProgressWrapperTypeName, nameof(RendererInitProgressWrapper.EngineReady))]
        private static bool EngineReadyPrefix()
        {
            if (!Available || CanProgressToReady!.Value)
                _advancedToReady = true;

            return !Available || CanProgressToReady!.Value;
        }

        private static bool Prepare() => IsRendererInitProgressWrapperAvailable;

        [HarmonyPrefix]
        [HarmonyPatch(RendererProgressWrapperTypeName, nameof(RendererInitProgressWrapper.SendToRenderer))]
        private static void SendToRendererPrefix(string? ___fixedPhase, ref string? ___subPhase, ref bool ___forceShow)
        {
            if (!LoadingConfig.Instance.HijackLoadProgressIndicator)
                return;

            // To center the line when the subPhase has new lines in it
            var subPhaseNewLines = ___subPhase?.Split(Environment.NewLine).Length - 1 ?? 0;
            var prefixNewLines = Enumerable.Repeat(Environment.NewLine, subPhaseNewLines).Join(delimiter: "");

            var replacementSubphase = $"{prefixNewLines}{___fixedPhase}";

            // Add subphase only when it's actually present
            // Careful: those aren't spaces but em-boxes
            if (!string.IsNullOrWhiteSpace(___subPhase))
                replacementSubphase += $" {___subPhase}";

            ___subPhase = replacementSubphase;
            ___forceShow |= LoadingConfig.Instance.AlwaysShowLoadingPhases;
        }

        [HarmonyPatch]
        [HarmonyPatchCategory(nameof(LoadProgressReporter))]
        private static class FixedPhaseCountReplacementPatch
        {
            private static bool Prepare() => IsRendererInitProgressWrapperAvailable;

            private static IEnumerable<MethodBase> TargetMethods()
            {
                // Using nameof is fine as it gets compiled to loading a constant string
                yield return AccessTools.DeclaredMethod($"{RendererProgressWrapperTypeName}:{nameof(RendererInitProgressWrapper.EngineReady)}");
                yield return AccessTools.DeclaredMethod($"{RendererProgressWrapperTypeName}:{nameof(RendererInitProgressWrapper.SendToRenderer)}");
            }

            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
            {
                var isSendToRenderer = __originalMethod.Name == nameof(RendererInitProgressWrapper.SendToRenderer);
                var totalFixedPhaseCountGetter = AccessTools.DeclaredPropertyGetter(typeof(LoadProgressReporter), nameof(InternalTotalFixedPhaseCount));

                foreach (var instruction in instructions)
                {
                    if (!instruction.LoadsConstant(OriginalTotalFixedPhaseCount!.Value))
                    {
                        yield return instruction;
                        continue;
                    }

                    if (isSendToRenderer)
                        yield return new CodeInstruction(OpCodes.Conv_R4);

                    // Replace references to the constant TotalFixedPhaseCount with a call to our modified value
                    yield return new CodeInstruction(OpCodes.Call, totalFixedPhaseCountGetter);
                }
            }
        }
    }
}