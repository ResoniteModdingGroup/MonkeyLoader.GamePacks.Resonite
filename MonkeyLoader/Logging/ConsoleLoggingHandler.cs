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
    /// An <see cref="ILoggingHandler"/> that writes lines to the <see cref="Console"/>.
    /// </summary>
    public sealed class ConsoleLoggingHandler : ILoggingHandler
    {
        /// <inheritdoc/>
        public void Debug(Func<object> messageProducer) => Log(messageProducer().ToString());

        /// <inheritdoc/>
        public void Error(Func<object> messageProducer) => Log(messageProducer().ToString());

        /// <inheritdoc/>
        public void Fatal(Func<object> messageProducer) => Log(messageProducer().ToString());

        /// <inheritdoc/>
        public void Info(Func<object> messageProducer) => Log(messageProducer().ToString());

        /// <summary>
        /// Writes a message prefixed with a timestamp to the <see cref="Console"/>.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public void Log(string message) => Console.WriteLine($"[{DateTime.Now:HH:mm:ss:ffff}] {message}");

        /// <inheritdoc/>
        public void Trace(Func<object> messageProducer) => Log(messageProducer().ToString());

        /// <inheritdoc/>
        public void Warn(Func<object> messageProducer) => Log(messageProducer().ToString());
    }
}