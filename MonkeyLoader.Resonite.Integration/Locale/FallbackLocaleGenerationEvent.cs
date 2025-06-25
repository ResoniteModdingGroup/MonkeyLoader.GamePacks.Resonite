using Elements.Assets;
using EnumerableToolkit;
using MonkeyLoader.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.Locale
{
    /// <summary>
    /// Represents the event data for the Fallback Locale Generation Event.
    /// </summary>
    /// <remarks>
    /// This event can be used by Monkeys that make use of locale keys to inject
    /// programmatically generated keys, if they haven't been defined previously.
    /// </remarks>
    public sealed class FallbackLocaleGenerationEvent : AsyncEvent
    {
        private static readonly Dictionary<string, object> _emptyArguments = [];

        private readonly LocaleResource _localeResource;

        /// <summary>
        /// Gets all currently known message keys with their associated messages.
        /// </summary>
        public IEnumerable<KeyValuePair<string, LocaleResource.Message>> Messages
            => _localeResource._formatMessages.AsSafeEnumerable();

        private Dictionary<string, LocaleResource.Message> FormatMessages => _localeResource._formatMessages;

        internal FallbackLocaleGenerationEvent(LocaleResource localeResource)
        {
            _localeResource = localeResource;
        }

        /// <summary>
        /// Adds the given message for the key if there's no message associated with it yet.
        /// </summary>
        /// <param name="key">The key to assign the message to.</param>
        /// <param name="message">The message to assign to the key.</param>
        /// <returns><c>true</c> if the message was newly assigned; otherwise, <c>false</c>.</returns>
        public bool AddMessage(string key, string message)
        {
            if (FormatMessages.ContainsKey(key))
                return false;

            FormatMessages.Add(key, new LocaleResource.Message(FallbackLocaleGenerator.LocaleCode, message));
            return true;
        }

        /// <summary>
        /// Formats the message pattern associated with the given <paramref name="key"/>
        /// using the provided <paramref name="arguments"/> for its variables.
        /// </summary>
        /// <param name="key">The key to format the message for.</param>
        /// <param name="arguments">The arguments to use for the message pattern's variables.</param>
        /// <returns>The formatted message if there is one associated with the <paramref name="key"/>; otherwise, <see langword="null"/>.</returns>
        public string? FormatMessage(string key, Dictionary<string, object> arguments)
            => _localeResource.Format(key, arguments);

        /// <summary>
        /// Formats the message pattern associated with the given <paramref name="key"/>
        /// using the provided argument for its variables.
        /// </summary>
        /// <inheritdoc cref="FormatMessage(string, Dictionary{string, object})"/>
        public string? FormatMessage(string key, string argumentName, object argument)
            => FormatMessage(key, new() { [argumentName] = argument });

        /// <summary>
        /// Formats the message pattern associated with the given <paramref name="key"/> without any arguments.
        /// </summary>
        /// <inheritdoc cref="FormatMessage(string, Dictionary{string, object})"/>
        public string? FormatMessage(string key)
            => FormatMessage(key, _emptyArguments);

        /// <summary>
        /// Gets the message pattern associated with the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key to get the message for.</param>
        /// <returns>The message associated with the <paramref name="key"/>.</returns>
        /// <exception cref="KeyNotFoundException">When there's no message associated with the <paramref name="key"/>.</exception>
        public string GetMessage(string key) => FormatMessages[key].messagePattern;

        /// <summary>
        /// Tries to format the message pattern associated with the given <paramref name="key"/>
        /// using the provided <paramref name="arguments"/> for its variables.
        /// </summary>
        /// <param name="key">The key to format the message for.</param>
        /// <param name="arguments">The arguments to use for the message pattern's variables.</param>
        /// <param name="formattedMessage">The formatted message if there is one associated with the <paramref name="key"/>; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the message was formatted successfully; otherwise, <see langword="false"/>.</returns>
        public bool TryFormatMessage(string key, Dictionary<string, object> arguments, [NotNullWhen(true)] out string? formattedMessage)
        {
            try
            {
                formattedMessage = FormatMessage(key, arguments);
                return formattedMessage is not null;
            }
            catch
            {
                formattedMessage = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to format the message pattern associated with the given <paramref name="key"/>
        /// using the provided argument for its variables.
        /// </summary>
        /// <param name="key">The key to format the message for.</param>
        /// <param name="argumentName">The name of the argument for formatting.</param>
        /// <param name="argument">The value of the argument for formatting.</param>
        /// <param name="formattedMessage">The formatted message if there is one associated with the <paramref name="key"/>; otherwise, <see langword="null"/>.</param>
        /// <inheritdoc cref="TryFormatMessage(string, Dictionary{string, object}, out string?)"/>
        public bool TryFormatMessage(string key, string argumentName, object argument, [NotNullWhen(true)] out string? formattedMessage)
            => TryFormatMessage(key, new() { [argumentName] = argument }, out formattedMessage);

        /// <summary>
        /// Tries to format the message pattern associated with the given <paramref name="key"/> without any arguments.
        /// </summary>
        /// <inheritdoc cref="TryFormatMessage(string, Dictionary{string, object}, out string?)"/>
        public bool TryFormatMessage(string key, [NotNullWhen(true)] out string? formattedMessage)
            => TryFormatMessage(key, _emptyArguments, out formattedMessage);

        /// <summary>
        /// Tries to get the message pattern associated with the given key.
        /// </summary>
        /// <returns><c>true</c> if the key was found; otherwise, <c>false</c>.</returns>
        /// <inheritdoc cref="TryFormatMessage(string, Dictionary{string, object}, out string?)"/>
        public bool TryGetMessage(string key, [NotNullWhen(true)] out string? message)
        {
            if (FormatMessages.TryGetValue(key, out var value))
            {
                message = value.messagePattern;
                return true;
            }

            message = null;
            return false;
        }
    }
}