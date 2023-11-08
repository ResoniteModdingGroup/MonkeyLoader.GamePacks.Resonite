using Elements.Core;
using MonkeyLoader.Logging;
using System;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Maps the <see cref="MonkeyLogger"/> functions to Resonite's <see cref="UniLog"/>.
    /// </summary>
    public sealed class ResoniteLoggingHandler : LoggingHandler
    {
        /// <summary>
        /// Gets a cached instance of the <see cref="ResoniteLoggingHandler"/>.
        /// </summary>
        public static ResoniteLoggingHandler Instance { get; } = new();

        /// <inheritdoc/>
        public override bool Connected => true;

        /// <inheritdoc/>
        public override void Debug(Func<object> messageProducer) => UniLog.Log(messageProducer());

        /// <inheritdoc/>
        public override void Error(Func<object> messageProducer) => UniLog.Error(messageProducer().ToString());

        /// <inheritdoc/>
        public override void Fatal(Func<object> messageProducer) => UniLog.Error(messageProducer().ToString());

        /// <inheritdoc/>
        public override void Info(Func<object> messageProducer) => UniLog.Log(messageProducer());

        /// <inheritdoc/>
        public override void Trace(Func<object> messageProducer) => UniLog.Log(messageProducer());

        /// <inheritdoc/>
        public override void Warn(Func<object> messageProducer) => UniLog.Warning(messageProducer().ToString());
    }
}