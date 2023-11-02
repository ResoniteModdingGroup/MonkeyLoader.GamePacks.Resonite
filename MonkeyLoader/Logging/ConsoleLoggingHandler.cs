using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyLoader.Logging
{
    /// <summary>
    /// Implements an <see cref="LoggingHandler"/> that writes messages to the <see cref="Console"/>.
    /// </summary>
    public sealed class ConsoleLoggingHandler : LoggingHandler
    {
        /// <inheritdoc/>
        public override bool Connected => true;

        /// <inheritdoc/>
        public override void Debug(Func<object> messageProducer) => Log(messageProducer().ToString());

        /// <inheritdoc/>
        public override void Error(Func<object> messageProducer) => Log(messageProducer().ToString());

        /// <inheritdoc/>
        public override void Fatal(Func<object> messageProducer) => Log(messageProducer().ToString());

        /// <inheritdoc/>
        public override void Info(Func<object> messageProducer) => Log(messageProducer().ToString());

        /// <summary>
        /// Writes a message prefixed with a timestamp to the <see cref="Console"/>.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public void Log(string message) => Console.WriteLine($"[{DateTime.Now:HH:mm:ss:ffff}] {message}");

        /// <inheritdoc/>
        public override void Trace(Func<object> messageProducer) => Log(messageProducer().ToString());

        /// <inheritdoc/>
        public override void Warn(Func<object> messageProducer) => Log(messageProducer().ToString());
    }
}