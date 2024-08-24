using EnumerableToolkit;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLoader.Resonite.DataFeeds
{
    /// <summary>
    /// Holds the <see cref="DataFeedInjector{TDataFeed}"/>-Types which have been created.
    /// </summary>
    public static class DataFeedInjectorManager
    {
        private static readonly HashSet<Type> _dataFeedInjectorMonkeys = [];

        /// <summary>
        /// Gets the <see cref="DataFeedInjector{TDataFeed}"/>-Types which have been created.
        /// </summary>
        public static IEnumerable<Type> MonkeyTypes => _dataFeedInjectorMonkeys.AsSafeEnumerable();

        internal static bool AddInjector<TInjector>()
            => _dataFeedInjectorMonkeys.Add(typeof(TInjector));
    }
}