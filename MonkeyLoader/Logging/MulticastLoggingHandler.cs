using NuGet.Packaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Logging
{
    /// <summary>
    /// Implements an <see cref="LoggingHandler"/> that can delegate messages to multiple other handlers.
    /// </summary>
    public sealed class MulticastLoggingHandler : LoggingHandler
    {
        private readonly HashSet<LoggingHandler> _loggingHandlers = new();

        /// <inheritdoc/>
        public override bool Connected => _loggingHandlers.Any(handler => handler.Connected);

        /// <summary>
        /// Gets the currently <see cref="LoggingHandler.Connected">connected</see> logging handlers that this one delegates messages to.
        /// </summary>
        public IEnumerable<LoggingHandler> ConnectedHandlers => _loggingHandlers.Where(IsConnected);

        /// <summary>
        /// Gets all logging handlers that this one delegates messages to.
        /// </summary>
        public IEnumerable<LoggingHandler> LoggingHandlers => _loggingHandlers.AsSafeEnumerable();

        /// <summary>
        /// Creates a new multicast logging handler with the given handlers to delegate messages to.
        /// </summary>
        /// <param name="loggingHandlers">The logging handlers to delegate messages to.</param>
        public MulticastLoggingHandler(params LoggingHandler[] loggingHandlers)
            : this((IEnumerable<LoggingHandler>)loggingHandlers)
        { }

        /// <summary>
        /// Creates a new multicast logging handler with the given handlers to delegate messages to.
        /// </summary>
        /// <param name="loggingHandlers">The logging handlers to delegate messages to.</param>
        public MulticastLoggingHandler(IEnumerable<LoggingHandler> loggingHandlers)
        {
            _loggingHandlers.AddRange(loggingHandlers.Where(handler => handler is not (null or MissingLoggingHandler)));
        }

        /// <inheritdoc/>
        public override void Debug(Func<object> messageProducer)
        {
            foreach (var loggingHandler in ConnectedHandlers)
                loggingHandler.Debug(messageProducer);
        }

        /// <inheritdoc/>
        public override void Error(Func<object> messageProducer)
        {
            foreach (var loggingHandler in ConnectedHandlers)
                loggingHandler.Error(messageProducer);
        }

        /// <inheritdoc/>
        public override void Fatal(Func<object> messageProducer)
        {
            foreach (var loggingHandler in ConnectedHandlers)
                loggingHandler.Fatal(messageProducer);
        }

        /// <inheritdoc/>
        public override void Info(Func<object> messageProducer)
        {
            foreach (var loggingHandler in ConnectedHandlers)
                loggingHandler.Info(messageProducer);
        }

        /// <inheritdoc/>
        public override void Trace(Func<object> messageProducer)
        {
            foreach (var loggingHandler in ConnectedHandlers)
                loggingHandler.Trace(messageProducer);
        }

        /// <inheritdoc/>
        public override void Warn(Func<object> messageProducer)
        {
            foreach (var loggingHandler in ConnectedHandlers)
                loggingHandler.Warn(messageProducer);
        }

        private static bool IsConnected(LoggingHandler loggingHandler) => loggingHandler.Connected;
    }
}