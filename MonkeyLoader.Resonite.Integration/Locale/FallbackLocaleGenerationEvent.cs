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
    public sealed class FallbackLocaleGenerationEvent : IAsyncEvent
    {
        private readonly Dictionary<string, LocaleResource.Message> _messages;

        internal FallbackLocaleGenerationEvent(Dictionary<string, LocaleResource.Message> messages)
        {
            _messages = messages;
        }

        public bool AddMessage(string key, string message)
        {
            if (_messages.ContainsKey(key))
                return false;

            _messages.Add(key, new LocaleResource.Message(FallbackLocaleGenerator.LocaleCode, message));
            return true;
        }

        public string GetMessage(string key) => _messages[key].messagePattern;

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