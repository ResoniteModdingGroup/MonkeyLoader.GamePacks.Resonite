using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader
{
    /// <summary>
    /// Contains handy extension methods for <see cref="IEnumerable{T}"/>.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Formats an <see cref="AggregateException"/> with a message and a list of
        /// all <see cref="AggregateException.InnerExceptions">inner</see> <see cref="Exception"/>s.
        /// </summary>
        /// <param name="ex">The exception to format.</param>
        /// <param name="message">The message to prepend.</param>
        /// <returns>The formatted message and exceptions.</returns>
        public static string Format(this AggregateException ex, string message)
            => $"{message}{Environment.NewLine}{string.Join(Environment.NewLine, ex.InnerExceptions.Select(inEx => $"{inEx.Message}{Environment.NewLine}{inEx.StackTrace}"))}";

        /// <summary>
        /// Tries to cast every item from the <paramref name="source"/> to <typeparamref name="TTo"/>.
        /// </summary>
        /// <typeparam name="TFrom">The items in the source sequence.</typeparam>
        /// <typeparam name="TTo">The items in the result sequence.</typeparam>
        /// <param name="source">The items to try and cast.</param>
        /// <returns>All items from the source that were castable to <typeparamref name="TTo"/>.</returns>
        public static IEnumerable<TTo> SelectCastable<TFrom, TTo>(this IEnumerable<TFrom> source)
        {
            foreach (var item in source)
            {
                if (item is TTo toItem)
                    yield return toItem;
            }
        }

        /// <summary>
        /// Individually calls every method from a <see cref="Delegate"/>'s invocation list in a try-catch-block,
        /// collecting any <see cref="Exception"/>s into an <see cref="AggregateException"/>.
        /// </summary>
        /// <param name="del">The delegate to safely invoke.</param>
        /// <param name="args">The arguments for the invocation.</param>
        /// <exception cref="AggregateException">Thrown when any invoked methods threw. Contains all nested Exceptions.</exception>
        public static void TryInvokeAll(this Delegate del, params object[] args)
            => del.GetInvocationList().TryInvokeAll(args);

        /// <summary>
        /// Individually calls all <paramref name="delegates"/> in a try-catch-block,
        /// collecting any <see cref="Exception"/>s into an <see cref="AggregateException"/>.
        /// </summary>
        /// <param name="delegates">The delegates to safely invoke.</param>
        /// <param name="args">The arguments for the invocation.</param>
        /// <exception cref="AggregateException">Thrown when any invoked methods threw. Contains all nested Exceptions.</exception>
        public static void TryInvokeAll(this IEnumerable<Delegate> delegates, params object[] args)
        {
            var exceptions = new List<Exception>();

            foreach (var handler in delegates)
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

        /// <summary>
        /// Tries to transform each item in the <paramref name="source"/> sequence using the <paramref name="trySelector"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of items in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of items in the result sequence.</typeparam>
        /// <param name="source">The source sequence to transform.</param>
        /// <param name="trySelector">A selector following the try-pattern.</param>
        /// <returns>A result sequence containing only the successfully transformed items.</returns>
        public static IEnumerable<TResult> TrySelect<TSource, TResult>(this IEnumerable<TSource> source, TrySelector<TSource, TResult> trySelector)
        {
            foreach (var item in source)
            {
                if (trySelector(item, out var result))
                    yield return result;
            }
        }

        /// <summary>
        /// A selector following the try-pattern.
        /// </summary>
        /// <typeparam name="TSource">The type of the source item.</typeparam>
        /// <typeparam name="TResult">The type of the result item.</typeparam>
        /// <param name="source">The source item to transform.</param>
        /// <param name="result">The result item when successful, or <c>null</c> otherwise.</param>
        /// <returns>Whether the transformation was successful.</returns>
        public delegate bool TrySelector<TSource, TResult>(TSource source, [NotNullWhen(true)] out TResult? result);
    }
}