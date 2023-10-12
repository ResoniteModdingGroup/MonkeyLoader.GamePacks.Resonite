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
    /// An <see cref="ILoggingHandler"/> that writes lines to a file.
    /// </summary>
    public sealed class FileLoggingHandler : ILoggingHandler
    {
        private readonly StreamWriter streamWriter;

        /// <summary>
        /// Creates a new file logging handler with the file at the given path as the target.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        public FileLoggingHandler(string path) : this(new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
        { }

        /// <summary>
        /// Creates a new file logging handler with the given <see cref="FileStream"/> as the target.
        /// </summary>
        /// <param name="fileStream">The file to write to.</param>
        public FileLoggingHandler(FileStream fileStream)
        {
            streamWriter = new StreamWriter(fileStream);
        }

        /// <inheritdoc/>
        public void Debug(Func<object> messageProducer) => Log(messageProducer().ToString());

        /// <inheritdoc/>
        public void Error(Func<object> messageProducer) => Log(messageProducer().ToString());

        /// <inheritdoc/>
        public void Fatal(Func<object> messageProducer) => Log(messageProducer().ToString());

        /// <inheritdoc/>
        public void Info(Func<object> messageProducer) => Log(messageProducer().ToString());

        /// <summary>
        /// Writes a message prefixed with a timestamp to the log file.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public void Log(string message)
        {
            lock (streamWriter)
            {
                streamWriter.WriteLine($"[{DateTime.Now:HH:mm:ss:ffff}] {message}");
                streamWriter.Flush();
            }
        }

        /// <inheritdoc/>
        public void Trace(Func<object> messageProducer) => Log(messageProducer().ToString());

        /// <inheritdoc/>
        public void Warn(Func<object> messageProducer) => Log(messageProducer().ToString());
    }
}