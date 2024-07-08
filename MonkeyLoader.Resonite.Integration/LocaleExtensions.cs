using Elements.Core;
using FrooxEngine;
using MonkeyLoader.Configuration;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Contains helper methods to create <see cref="LocaleString"/>s prefixed with a <see cref="Mod.Id"/>.
    /// </summary>
    public static class LocaleExtensions
    {
        /// <summary>
        /// Gets the latest loaded <see cref="Elements.Assets.LocaleResource"/> data.
        /// </summary>
        public static LocaleResource CurrentLocale => Userspace.Current.GetCoreLocale().Asset;

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
        public static string? Format(this LocaleString localeString, bool returnNullIfNotFound = false)
        {
            if (!localeString.isLocaleKey)
                return localeString.content;

            if (CurrentLocale.Format(localeString.content, localeString.EvaluateArguments(), returnNullIfNotFound) is not string localized)
                return null;

            var format = string.IsNullOrEmpty(localeString.format) ? "{0}" : localeString.format;

            return string.Format(format, localized);
        }

        /// <summary>
        /// Creates a locale key prefixed with <paramref name="identifiable"/>'s <see cref="IIdentifiable.FullId">FullId</see>, in the format:
        /// <c>$"{<paramref name="identifiable"/>.<see cref="IIdentifiable.FullId">FullId</see>}.{<paramref name="key"/>}"</c>.
        /// </summary>
        /// <returns>The prefixed locale key.</returns>
        public static string GetLocaleKey(this IIdentifiable identifiable, string key)
            => $"{identifiable.FullId}.{key}";

        /// <summary>
        /// Uses <c>$"{<paramref name="mod"/>.<see cref="Mod.Id">Id</see>}.{<paramref name="key"/>}"</c>
        /// as the key to create a <see cref="LocaleString"/> using the other arguments.
        /// </summary>
        /// <returns>The <see cref="LocaleString"/> created from the key with the arguments.</returns>
        public static LocaleString GetLocaleString(this Mod mod, string key, string argName, object argField)
            => mod.GetLocaleKey(key).AsLocaleKey(argName, argField);

        /// <summary>
        /// Uses <c>$"{<paramref name="mod"/>.<see cref="Mod.Id">Id</see>}.{<paramref name="key"/>}"</c>
        /// as the key to create a <see cref="LocaleString"/> using the other arguments.
        /// </summary>
        /// <returns>The <see cref="LocaleString"/> created from the key with the arguments.</returns>
        public static LocaleString GetLocaleString(this Mod mod, string key, string format, string argName, object argField)
            => mod.GetLocaleKey(key).AsLocaleKey(format, argName, argField);

        /// <summary>
        /// Uses <c>$"{<paramref name="mod"/>.<see cref="Mod.Id">Id</see>}.{<paramref name="key"/>}"</c>
        /// as the key to create a <see cref="LocaleString"/> using the other arguments.
        /// </summary>
        /// <returns>The <see cref="LocaleString"/> created from the key with the arguments.</returns>
        public static LocaleString GetLocaleString(this Mod mod, string key, params (string, object)[] arguments)
            => mod.GetLocaleKey(key).AsLocaleKey(arguments);

        /// <summary>
        /// Uses <c>$"{<paramref name="mod"/>.<see cref="Mod.Id">Id</see>}.{<paramref name="key"/>}"</c>
        /// as the key to create a <see cref="LocaleString"/> using the other arguments.
        /// </summary>
        /// <returns>The <see cref="LocaleString"/> created from the key with the arguments.</returns>
        public static LocaleString GetLocaleString(this Mod mod, string key, string format, params (string, object)[] arguments)
            => mod.GetLocaleKey(key).AsLocaleKey(format, arguments);

        /// <summary>
        /// Uses <c>$"{<paramref name="mod"/>.<see cref="Mod.Id">Id</see>}.{<paramref name="key"/>}"</c>
        /// as the key to create a <see cref="LocaleString"/> using the other arguments.
        /// </summary>
        /// <returns>The <see cref="LocaleString"/> created from the key with the arguments.</returns>
        public static LocaleString GetLocaleString(this Mod mod, string key, bool continuous, Dictionary<string, object>? arguments = null)
            => mod.GetLocaleKey(key).AsLocaleKey(continuous, arguments!);

        /// <summary>
        /// Uses <c>$"{<paramref name="mod"/>.<see cref="Mod.Id">Id</see>}.{<paramref name="key"/>}"</c>
        /// as the key to create a <see cref="LocaleString"/> using the other arguments.
        /// </summary>
        /// <returns>The <see cref="LocaleString"/> created from the key with the arguments.</returns>
        public static LocaleString GetLocaleString(this Mod mod, string key, string? format = null, bool continuous = true, Dictionary<string, object>? arguments = null)
            => mod.GetLocaleKey(key).AsLocaleKey(format!, continuous, arguments!);
    }
}