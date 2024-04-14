using Elements.Assets;
using MonkeyLoader.Events;
using MonkeyLoader.Meta;

namespace MonkeyLoader.Resonite.Locale
{
    /// <summary>
    /// Represents the event data for a locale loading event.
    /// </summary>
    /// <remarks>
    /// <para>
    /// By default, the <see cref="LocaleData">locale data</see> provided by
    /// <see cref="Mod">mods</see> in their <see cref="Mod.FileSystem">FileSystem</see>
    /// is injected into the loading process of the <see cref="LocaleResource"/> asset
    /// by the predefined <see cref="FileSystemLocaleLoader"/>.
    /// </para>
    /// <para>
    /// Data files are automatically loaded from any <see cref="Mod">mods</see> that provide them.<br/>
    /// To be considered for loading, <see cref="LocaleData">locale data</see> files have
    /// to be in <i>any</i> folder called <c>Locale</c> <i>anywhere</i> in a mod's <see cref="Mod.ContentPaths">content files</see>.
    /// Multiple <c>Locale</c> folders with files for the same locale are no problem.
    /// </para>
    /// <para>
    /// <see cref="LocaleData">Locale data</see> files need to have their language code
    /// as their filename, for example: <c>de-at.json</c> for austrian german, or <c>de.json</c> for any german.<br/>
    /// The locale definitions are loaded with the exact locale code (de-at) coming first,
    /// falling back to the base locale code (de), and finally the universal fallback english (en).<br/>
    /// All <see cref="Elements.Core.LocaleHelper.AsLocaleKey(string, string, object)">locale keys</see>
    /// will use the first definition encountered for them:
    /// A key that's the same for <c>de-at</c> and <c>de</c> would not have to be present in <c>de-at</c>,
    /// while a different one in <c>de-at</c> would take priority over the one in <c>de</c>.
    /// </para>
    /// <para>
    /// The json files need to be in the following format (for <c>en.json</c> here),
    /// although the <see cref="Mod.Id">ModId</see>-prefix is just convention:
    /// <code>
    /// {
    ///   "localeCode": "en",
    ///   "authors": [ "Mod", "Locale", "Author", "Names" ],
    ///   "messages": {
    ///     "ModId.KeyA": "A first message.",
    ///     "ModId.KeyB": "Better locale support!"
    ///   }
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    public sealed class LocaleLoadingEvent : IAsyncEvent
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
        /// Gets the <see cref="Elements.Assets.LocaleResource"/>
        /// instance that new locale authors and keys should be added to.
        /// </summary>
        public LocaleResource LocaleResource { get; }

        internal LocaleLoadingEvent(LocaleResource localeResource, string localeCode, bool last)
        {
            LocaleResource = localeResource;
            LocaleCode = localeCode;
            Last = last;
        }
    }
}