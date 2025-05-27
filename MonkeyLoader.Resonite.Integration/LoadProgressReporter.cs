using MonkeyLoader.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
            => RunForPrettySplashAsync(milliseconds, Task.Run(taskFunc), cancellationToken);

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
            if (Available && LoadingConfig.Instance.PrettySplashProgress)
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
    }
}