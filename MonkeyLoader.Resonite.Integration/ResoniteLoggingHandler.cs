using Elements.Core;
using MonkeyLoader.Logging;
using System;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Maps the <see cref="MonkeyLogger"/> functions to Resonite's <see cref="UniLog"/>.
    /// </summary>
    public sealed class ResoniteLoggingHandler : ILoggingHandler
    {
        /// <inheritdoc/>
        public void Debug(Func<object> messageProducer) => UniLog.Log(messageProducer());

        /// <inheritdoc/>
        public void Error(Func<object> messageProducer) => UniLog.Error(messageProducer().ToString());

        /// <inheritdoc/>
        public void Fatal(Func<object> messageProducer) => UniLog.Error(messageProducer().ToString());

        /// <inheritdoc/>
        public void Info(Func<object> messageProducer) => UniLog.Log(messageProducer());

        /// <inheritdoc/>
        public void Trace(Func<object> messageProducer) => UniLog.Log(messageProducer());

        /// <inheritdoc/>
        public void Warn(Func<object> messageProducer) => UniLog.Warning(messageProducer().ToString());
    }
}