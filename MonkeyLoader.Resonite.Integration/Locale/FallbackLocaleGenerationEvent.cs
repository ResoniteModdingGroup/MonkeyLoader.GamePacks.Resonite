using Elements.Assets;
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
        private readonly Dictionary<string, LocaleResource.Message> _messages;

        internal FallbackLocaleGenerationEvent(Dictionary<string, LocaleResource.Message> messages)
        {
            _messages = messages;
        }

        /// <summary>
        /// Adds the given message for the key if there's no message associated with it yet.
        /// </summary>
        /// <param name="key">The key to assign the message to.</param>
        /// <param name="message">The message to assign to the key.</param>
        /// <returns><c>true</c> if the message was newly assigned; otherwise, <c>false</c>.</returns>
        public bool AddMessage(string key, string message)
        {
            if (_messages.ContainsKey(key))
                return false;

            _messages.Add(key, new LocaleResource.Message(FallbackLocaleGenerator.LocaleCode, message));
            return true;
        }

        /// <summary>
        /// Gets the message pattern associated with the given key.
        /// </summary>
        /// <param name="key">The key to get the message for.</param>
        /// <returns>The message associated with the key.</returns>
        /// <exception cref="KeyNotFoundException">When there's no message associated with the key.</exception>
        public string GetMessage(string key) => _messages[key].messagePattern;

        /// <summary>
        /// Tries to get the message pattern associated with the given key.
        /// </summary>
        /// <param name="key">The key to get the message for.</param>
        /// <param name="message">The message associated with the key, or <c>null</c> if it wasn't found.</param>
        /// <returns><c>true</c> if the key was found; otherwise, <c>false</c>.</returns>
        public bool TryGetMessage(string key, [NotNullWhen(true)] out string? message)
        {
            if (_messages.TryGetValue(key, out var value))
            {
                message = value.messagePattern;
                return true;
            }

            message = null;
            return false;
        }
    }
}