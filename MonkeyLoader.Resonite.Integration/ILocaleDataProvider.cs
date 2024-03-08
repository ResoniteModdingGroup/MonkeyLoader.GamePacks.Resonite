using Elements.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Defines the interface for locale data providers used by the <see cref="LocaleDataInjector"/>.
    /// </summary>
    public interface ILocaleDataProvider
    {
        /// <summary>
        /// Gets the priority of this locale data provider.
        /// </summary>
        /// <value>
        /// An integer used to sort the locale data providers used by the <see cref="LocaleDataInjector"/>.
        /// </value>
        public int Priority { get; }

        /// <summary>
        /// Gets this provider's locale data for the given locale.<br/>
        /// Should not be <c>null</c> or fail if
        /// <see cref="SupportsLocale">SupportsLocale</see>(<paramref name="locale"/>) returned <c>true</c>.
        /// </summary>
        /// <param name="locale">The locale code to provide the data for.</param>
        /// <returns>This provider's locale data for the given locale.</returns>
        public LocaleData GetLocaleData(string locale);

        /// <summary>
        /// Determines whether this provider's <see cref="GetLocaleData">GetLocaleData</see>
        /// method should be queried for the given <paramref name="locale"/> code.
        /// </summary>
        /// <param name="locale">The locale code to determine support for.</param>
        /// <returns><c>true</c> if this provider should be queried; otherwise <c>false</c>.</returns>
        public bool SupportsLocale(string locale);
    }
}