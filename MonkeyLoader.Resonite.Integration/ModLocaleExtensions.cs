using Elements.Core;
using FrooxEngine;
using MonkeyLoader.Configuration;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Contains helper methods to create <see cref="LocaleString"/>s prefixed with a <see cref="Mod.Id"/>.
    /// </summary>
    public static class ModLocaleExtensions
    {
        /// <summary>
        /// Creates a locale key prefixed with the <see cref="Mod.Id"/> in the format:
        /// <c>$"{<paramref name="mod"/>.<see cref="Mod.Id">Id</see>}.{<paramref name="key"/>}"</c>.
        /// </summary>
        /// <returns>The prefixed locale key.</returns>
        public static string GetLocaleKey(this Mod mod, string key)
            => $"{mod.Id}.{key}";

        /// <summary>
        /// Creates a locale key prefixed with the <see cref="ConfigSection.FullId"/> in the format:
        /// <c>$"{<paramref name="configSection"/>.<see cref="ConfigSection.FullId">FullId</see>}.{<paramref name="key"/>}"</c>.
        /// </summary>
        /// <returns>The prefixed locale key.</returns>
        public static string GetLocaleKey(this ConfigSection configSection, string key)
            => $"{configSection.FullId}.{key}";

        /// <summary>
        /// Creates a locale key prefixed with the <see cref="IDefiningConfigKey.FullId"/> in the format:
        /// <c>$"{<paramref name="configKey"/>.<see cref="IDefiningConfigKey.FullId">FullId</see>}.{<paramref name="key"/>}"</c>.
        /// </summary>
        /// <returns>The prefixed locale key.</returns>
        public static string GetLocaleKey(this IDefiningConfigKey configKey, string key)
            => $"{configKey.FullId}.{key}";

        /// <summary>
        /// Creates a locale key prefixed with the <see cref="IMonkey.FullId"/> in the format:
        /// <c>$"{<paramref name="monkey"/>.<see cref="IMonkey.FullId">FullId</see>}.{<paramref name="key"/>}"</c>.
        /// </summary>
        /// <returns>The prefixed locale key.</returns>
        public static string GetLocaleKey(this IMonkey monkey, string key)
            => $"{monkey.FullId}.{key}";

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