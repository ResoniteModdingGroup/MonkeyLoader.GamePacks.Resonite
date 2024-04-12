using Elements.Assets;
using MonkeyLoader.Events;

namespace MonkeyLoader.Resonite.Locale
{
    /// <summary>
    /// Represents the event data for a locale loading event.
    /// </summary>
    public sealed class LocaleLoadingEvent : IAsyncEvent<LocaleResource>
    {
        /// <summary>
        /// Gets whether this is the last locale code
        /// being queried for the current locale loading process.
        /// </summary>
        public bool Last { get; }

        /// <summary>
        /// Gets the locale code that data should be loaded for.<br/>
        /// No processing needs to be done on this, fallbacks create their own events.
        /// </summary>
        public string LocaleCode { get; }

        /// <summary>
        /// Gets the <see cref="LocaleResource"/> instance that
        /// new locale authors and keys should be added to.
        /// </summary>
        public LocaleResource Target { get; }

        internal LocaleLoadingEvent(LocaleResource localeData, string localeCode, bool last)
        {
            Target = localeData;
            LocaleCode = localeCode;
            Last = last;
        }
    }
}