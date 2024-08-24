using Elements.Core;
using FrooxEngine;
using MonkeyLoader;
using MonkeyLoader.Meta;
using MonkeyLoader.Resonite.Locale;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LocaleResourceData = Elements.Assets.LocaleResource;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Contains helper methods to create <see cref="LocaleString"/>s prefixed with a <see cref="Mod.Id"/>.
    /// </summary>
    public static class LocaleExtensions
    {
        /// <summary>
        /// Gets the locale code for the fallback locale.
        /// </summary>
        public const string FallbackLocaleCode = "en";

        /// <summary>
        /// The name of the argument added to Mod's locale strings to indicate that they belong to a identifiable.
        /// </summary>
        public const string ModLocaleStringIndicatorArgumentName = "MonkeyLoader.Mod.LocaleString";

        /// <summary>
        /// Gets the latest loaded <see cref="Elements.Assets.LocaleResource"/> data for the current locale.
        /// </summary>
        public static LocaleResourceData CurrentLocale => Userspace.Current.GetCoreLocale().Asset.Data;

        /// <summary>
        /// Gets the latest loaded <see cref="Elements.Assets.LocaleResource"/> data for the fallback locale.
        /// </summary>
        public static LocaleResourceData FallbackLocale { get; private set; } = new();

        /// <summary>
        /// Gets the formatted, localized message of this <see cref="LocaleString"/>
        /// according to the <see cref="CurrentLocale">current locale</see>.
        /// </summary>
        /// <param name="localeString">The locale string to get the localized message of.</param>
        /// <param name="returnNullIfNotFound">Whether to return <c>null</c> if the current locale does not have a message for the locale key.</param>
        /// <returns>
        /// <see cref="LocaleString.content"/> if it's not a locale key; otherwise, the formatted, localized message,
        /// or <c>null</c> if <paramref name="returnNullIfNotFound"/> is <c>true</c> and the key does not have a message.
        /// </returns>
        public static string? FormatWithCurrent(this LocaleString localeString, bool returnNullIfNotFound = false)
            => localeString.FormatWithLocale(CurrentLocale, returnNullIfNotFound);

        /// <summary>
        /// Gets the formatted, localized message of this <see cref="LocaleString"/>
        /// according to the <see cref="FallbackLocale">fallback locale</see>.
        /// </summary>
        /// <param name="localeString">The locale string to get the localized message of.</param>
        /// <param name="returnNullIfNotFound">Whether to return <c>null</c> if the current locale does not have a message for the locale key.</param>
        /// <returns>
        /// <see cref="LocaleString.content"/> if it's not a locale key; otherwise, the formatted, localized message,
        /// or <c>null</c> if <paramref name="returnNullIfNotFound"/> is <c>true</c> and the key does not have a message.
        /// </returns>
        public static string? FormatWithFallback(this LocaleString localeString, bool returnNullIfNotFound = false)
            => localeString.FormatWithLocale(FallbackLocale, returnNullIfNotFound);

        /// <summary>
        /// Gets the formatted, localized message of this <see cref="LocaleString"/>
        /// according to the given <paramref name="locale"/>.
        /// </summary>
        /// <param name="localeString">The locale string to get the localized message of.</param>
        /// <param name="locale">The locale to localize the message in.</param>
        /// <param name="returnNullIfNotFound">Whether to return <c>null</c> if the current locale does not have a message for the locale key.</param>
        /// <returns>
        /// <see cref="LocaleString.content"/> if it's not a locale key; otherwise, the formatted, localized message,
        /// or <c>null</c> if <paramref name="returnNullIfNotFound"/> is <c>true</c> and the key does not have a message.
        /// </returns>
        public static string? FormatWithLocale(this LocaleString localeString, LocaleResourceData locale, bool returnNullIfNotFound = false)
        {
            if (!localeString.isLocaleKey)
                return localeString.content;

            try
            {
                var localized = locale?.Format(localeString.content, localeString.EvaluateArguments());
                if (localized is null && returnNullIfNotFound)
                    return null;

                var format = string.IsNullOrEmpty(localeString.format) ? "{0}" : localeString.format;

                return string.Format(format, localized ?? localeString.content);
            }
            catch (Exception ex)
            {
                UniLog.Error(ex.Format("Exception formatting message " + localeString.content + ", Locale Code: " + locale?.GetKeyLocaleCode(localeString.content)));
                return "ERROR!!!";
            }
        }

        /// <summary>
        /// Gets the formatted, localized message of this <see cref="LocaleString"/>
        /// according to the given <paramref name="locale"/>.
        /// </summary>
        /// <param name="localeString">The locale string to get the localized message of.</param>
        /// <param name="locale">The locale to localize the message in.</param>
        /// <param name="returnNullIfNotFound">Whether to return <c>null</c> if the current locale does not have a message for the locale key.</param>
        /// <returns>
        /// <see cref="LocaleString.content"/> if it's not a locale key; otherwise, the formatted, localized message,
        /// or <c>null</c> if <paramref name="returnNullIfNotFound"/> is <c>true</c> and the key does not have a message.
        /// </returns>
        public static string? FormatWithLocale(this LocaleString localeString, LocaleResource locale, bool returnNullIfNotFound = false)
            => localeString.FormatWithLocale(locale.Data, returnNullIfNotFound);

        /// <summary>
        /// Creates a locale key prefixed with <paramref name="identifiable"/>'s <see cref="IIdentifiable.FullId">FullId</see>, in the format:
        /// <c>$"{<paramref name="identifiable"/>.<see cref="IIdentifiable.FullId">FullId</see>}.{<paramref name="key"/>}"</c>.
        /// </summary>
        /// <param name="identifiable">The <see cref="IIdentifiable"/> whose <see cref="IIdentifiable.FullId">FullId</see> to use as a prefix.</param>
        /// <param name="key">The locale key to prefix.</param>
        /// <returns>The prefixed locale key.</returns>
        public static string GetLocaleKey(this IIdentifiable identifiable, string key)
            => $"{identifiable.FullId}.{key}";

        /// <summary>
        /// Uses <c><paramref name="identifiable"/>.<see cref="GetLocaleKey">GetLocaleKey</see>(<paramref name="key"/>)</c>
        /// as the key to create a <see cref="LocaleString"/> using the other arguments.
        /// </summary>
        /// <param name="identifiable">The <see cref="IIdentifiable"/> whose <see cref="IIdentifiable.FullId">FullId</see> to use as a prefix.</param>
        /// <param name="key">The locale key to prefix.</param>
        /// <param name="argName">The name of the single argument used to format the locale message.</param>
        /// <param name="argField">The value of the single argument used to format the locale message.</param>
        /// <returns>The <see cref="IsModLocaleString">Mod</see>-<see cref="LocaleString"/> created from the key with the arguments.</returns>
        public static LocaleString GetLocaleString(this IIdentifiable identifiable, string key, string argName, object argField)
            => identifiable.GetLocaleKey(key).AsLocaleKey((argName, argField), (ModLocaleStringIndicatorArgumentName, string.Empty));

        /// <summary>
        /// Uses <c><paramref name="identifiable"/>.<see cref="GetLocaleKey">GetLocaleKey</see>(<paramref name="key"/>)</c>
        /// as the key to create a <see cref="LocaleString"/> using the other arguments.
        /// </summary>
        /// <param name="identifiable">The <see cref="IIdentifiable"/> whose <see cref="IIdentifiable.FullId">FullId</see> to use as a prefix.</param>
        /// <param name="key">The locale key to prefix.</param>
        /// <param name="format">An additional format string to insert the locale message into.</param>
        /// <param name="argName">The name of the single argument used to format the locale message.</param>
        /// <param name="argField">The value of the single argument used to format the locale message.</param>
        /// <returns>The <see cref="IsModLocaleString">Mod</see>-<see cref="LocaleString"/> created from the key with the arguments.</returns>
        public static LocaleString GetLocaleString(this IIdentifiable identifiable, string key, string format, string argName, object argField)
            => identifiable.GetLocaleKey(key).AsLocaleKey(format, (argName, argField), (ModLocaleStringIndicatorArgumentName, string.Empty));

        /// <summary>
        /// Uses <c><paramref name="identifiable"/>.<see cref="GetLocaleKey">GetLocaleKey</see>(<paramref name="key"/>)</c>
        /// as the key to create a <see cref="LocaleString"/> using the other arguments.
        /// </summary>
        /// <param name="identifiable">The <see cref="IIdentifiable"/> whose <see cref="IIdentifiable.FullId">FullId</see> to use as a prefix.</param>
        /// <param name="key">The locale key to prefix.</param>
        /// <param name="arguments">The arguments used to format the locale message.</param>
        /// <returns>The <see cref="IsModLocaleString">Mod</see>-<see cref="LocaleString"/> created from the key with the arguments.</returns>
        public static LocaleString GetLocaleString(this IIdentifiable identifiable, string key, params (string, object)[] arguments)
            => identifiable.GetLocaleKey(key).AsLocaleKey(arguments.AddModIndicator());

        /// <summary>
        /// Uses <c><paramref name="identifiable"/>.<see cref="GetLocaleKey">GetLocaleKey</see>(<paramref name="key"/>)</c>
        /// as the key to create a <see cref="LocaleString"/> using the other arguments.
        /// </summary>
        /// <param name="identifiable">The <see cref="IIdentifiable"/> whose <see cref="IIdentifiable.FullId">FullId</see> to use as a prefix.</param>
        /// <param name="key">The locale key to prefix.</param>
        /// <param name="format">An additional format string to insert the locale message into.</param>
        /// <param name="arguments">The arguments used to format the locale message.</param>
        /// <returns>The <see cref="IsModLocaleString">Mod</see>-<see cref="LocaleString"/> created from the key with the arguments.</returns>
        public static LocaleString GetLocaleString(this IIdentifiable identifiable, string key, string format, params (string, object)[] arguments)
            => identifiable.GetLocaleKey(key).AsLocaleKey(format, arguments.AddModIndicator());

        /// <summary>
        /// Uses <c><paramref name="identifiable"/>.<see cref="GetLocaleKey">GetLocaleKey</see>(<paramref name="key"/>)</c>
        /// as the key to create a <see cref="LocaleString"/> using the other arguments.
        /// </summary>
        /// <param name="identifiable">The <see cref="IIdentifiable"/> whose <see cref="IIdentifiable.FullId">FullId</see> to use as a prefix.</param>
        /// <param name="key">The locale key to prefix.</param>
        /// <param name="continuous">Whether to assign the locale message once or continuously.</param>
        /// <param name="arguments">The arguments used to format the locale message.</param>
        /// <returns>The <see cref="IsModLocaleString">Mod</see>-<see cref="LocaleString"/> created from the key with the arguments.</returns>
        public static LocaleString GetLocaleString(this IIdentifiable identifiable, string key, bool continuous, Dictionary<string, object>? arguments = null)
            => identifiable.GetLocaleKey(key).AsLocaleKey(continuous, arguments.AddModIndicator());

        /// <summary>
        /// Uses <c><paramref name="identifiable"/>.<see cref="GetLocaleKey">GetLocaleKey</see>(<paramref name="key"/>)</c>
        /// as the key to create a <see cref="LocaleString"/> using the other arguments.
        /// </summary>
        /// <param name="identifiable">The <see cref="IIdentifiable"/> whose <see cref="IIdentifiable.FullId">FullId</see> to use as a prefix.</param>
        /// <param name="key">The locale key to prefix.</param>
        /// <param name="format">An additional format string to insert the locale message into.</param>
        /// <param name="continuous">Whether to assign the locale message once or continuously.</param>
        /// <param name="arguments">The arguments used to format the locale message.</param>
        /// <returns>The <see cref="IsModLocaleString">Mod</see>-<see cref="LocaleString"/> created from the key with the arguments.</returns>
        public static LocaleString GetLocaleString(this IIdentifiable identifiable, string key, string? format = null, bool continuous = true, Dictionary<string, object>? arguments = null)
            => identifiable.GetLocaleKey(key).AsLocaleKey(format!, continuous, arguments.AddModIndicator());

        /// <summary>
        /// Gets the formatted, localized message of the given locale <paramref name="key"/>
        /// prefixed with the <paramref name="identifiable"/>'s <see cref="IIdentifiable.FullId">FullId</see>
        /// according to the <see cref="CurrentLocale">current locale</see>.
        /// </summary>
        /// <param name="identifiable">The <see cref="IIdentifiable"/> whose <see cref="IIdentifiable.FullId">FullId</see> to use as a prefix.</param>
        /// <param name="key">The locale key to prefix.</param>
        public static string GetMessageInCurrent(this IIdentifiable identifiable, string key)
            => identifiable.GetMessageInLocale(key, CurrentLocale);

        /// <summary>
        /// Gets the formatted, localized message of the given locale <paramref name="key"/>
        /// prefixed with the <paramref name="identifiable"/>'s <see cref="IIdentifiable.FullId">FullId</see>
        /// according to the <see cref="FallbackLocale">fallback locale</see>.
        /// </summary>
        /// <param name="identifiable">The <see cref="IIdentifiable"/> whose <see cref="IIdentifiable.FullId">FullId</see> to use as a prefix.</param>
        /// <param name="key">The locale key to prefix.</param>
        public static string GetMessageInFallback(this IIdentifiable identifiable, string key)
            => identifiable.GetMessageInLocale(key, FallbackLocale);

        /// <summary>
        /// Gets the formatted, localized message of the given locale <paramref name="key"/>
        /// prefixed with the <paramref name="identifiable"/>'s <see cref="IIdentifiable.FullId">FullId</see>
        /// according to the given <paramref name="locale"/>.
        /// </summary>
        /// <param name="identifiable">The <see cref="IIdentifiable"/> whose <see cref="IIdentifiable.FullId">FullId</see> to use as a prefix.</param>
        /// <param name="key">The locale key to prefix.</param>
        /// <param name="locale">The locale to localize the message in.</param>
        public static string GetMessageInLocale(this IIdentifiable identifiable, string key, LocaleResourceData locale)
            => identifiable.GetLocaleString(key).FormatWithLocale(locale)!;

        /// <summary>
        /// Checks whether this <see cref="LocaleString"/> has a message in the <see cref="CurrentLocale">current locale</see>.
        /// </summary>
        /// <param name="localeString">The locale string to check for a message for.</param>
        /// <returns><c>true</c> if a message was found in the current locale; otherwise, <c>false</c>.</returns>
        public static bool HasMessageInCurrent(this LocaleString localeString)
            => localeString.HasMessageInLocale(CurrentLocale);

        /// <summary>
        /// Checks whether this locale key has a message in the <see cref="CurrentLocale">current locale</see>.
        /// </summary>
        /// <param name="localeKey">The locale key to check for a message for.</param>
        /// <returns><c>true</c> if a message was found in the current locale; otherwise, <c>false</c>.</returns>
        public static bool HasMessageInCurrent(this string? localeKey)
            => localeKey.HasMessageInLocale(CurrentLocale);

        /// <summary>
        /// Checks whether this <see cref="LocaleString"/> has a message in the <see cref="FallbackLocale">fallback locale</see>.
        /// </summary>
        /// <param name="localeString">The locale string to check for a message for.</param>
        /// <returns><c>true</c> if a message was found in the fallback locale; otherwise, <c>false</c>.</returns>
        public static bool HasMessageInFallback(this LocaleString localeString)
            => localeString.HasMessageInLocale(FallbackLocale);

        /// <summary>
        /// Checks whether this locale key has a message in the <see cref="FallbackLocale">fallback locale</see>.
        /// </summary>
        /// <param name="localeKey">The locale key to check for a message for.</param>
        /// <returns><c>true</c> if a message was found in the fallback locale; otherwise, <c>false</c>.</returns>
        public static bool HasMessageInFallback(this string? localeKey)
            => localeKey.HasMessageInLocale(FallbackLocale);

        /// <summary>
        /// Checks whether this <see cref="LocaleString"/> has a message in the given <paramref name="locale"/>.
        /// </summary>
        /// <param name="localeString">The locale string to check for a message for.</param>
        /// <param name="locale">The locale to check for a message in.</param>
        /// <returns><c>true</c> if a message was found in the locale; otherwise, <c>false</c>.</returns>
        public static bool HasMessageInLocale(this LocaleString localeString, LocaleResourceData locale)
            => localeString.content.HasMessageInLocale(locale);

        /// <summary>
        /// Checks whether this locale key has a message in the given <paramref name="locale"/>.
        /// </summary>
        /// <param name="localeKey">The locale key to check for a message for.</param>
        /// <param name="locale">The locale to check for a message in.</param>
        /// <returns><c>true</c> if a message was found in the locale; otherwise, <c>false</c>.</returns>
        public static bool HasMessageInLocale(this string? localeKey, LocaleResourceData locale)
            => localeKey is not null && locale is not null && locale._formatMessages.ContainsKey(localeKey);

        /// <summary>
        /// Determines whether this <see cref="LocaleString"/> belongs to an identifiable.
        /// </summary>
        /// <param name="localeString">The <see cref="LocaleString"/> to check.</param>
        /// <returns><c>true</c> if the given <see cref="LocaleString"/> belongs to an identifiable; otherwise, <c>false</c>.</returns>
        public static bool IsModLocaleString(this LocaleString localeString)
            => localeString.arguments.ContainsKey(ModLocaleStringIndicatorArgumentName);

        internal static async Task LoadFallbackLocaleAsync()
        {
            var locale = new LocaleResourceData();

            try
            {
                if (Engine.Current.Platform == Platform.Android)
                {
                    var localeContent = await Engine.Current.InternalResources.ReadTextResource($"Locale/{FallbackLocaleCode}");

                    if (localeContent != null)
                        locale.LoadDataAdditively(localeContent);
                }
                else
                {
                    var localePath = Path.Combine(Engine.Current.LocalePath, $"{FallbackLocaleCode}.json");

                    if (File.Exists(localePath))
                        await locale.LoadAdditively(localePath);
                }
            }
            catch (Exception arg)
            {
                UniLog.Error($"Error trying to load vanilla locale: {locale}\n{arg}", stackTrace: false);
            }

            await LocaleDataInjector.LoadLocalesAsync(locale, [FallbackLocaleCode]);

            FallbackLocale = locale;
        }

        private static (string, object)[] AddModIndicator(this (string, object)[]? arguments)
            => [.. (arguments ?? []), (ModLocaleStringIndicatorArgumentName, string.Empty)];

        private static Dictionary<string, object> AddModIndicator(this Dictionary<string, object>? arguments)
        {
            arguments ??= [];
            arguments[ModLocaleStringIndicatorArgumentName] = string.Empty;

            return arguments;
        }
    }
}