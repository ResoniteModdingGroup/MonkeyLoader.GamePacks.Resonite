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
    /// Implements an <see cref="LoggingHandler"/> that writes messages to a file.
    /// </summary>
    public sealed class FileLoggingHandler : LoggingHandler
    {
        private readonly StreamWriter _streamWriter;

        /// <inheritdoc/>
        public override bool Connected => _streamWriter.BaseStream.CanWrite;

        /// <summary>
        /// Creates a new file logging handler with the file at the given path as the target.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        public FileLoggingHandler(string path) : this(new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
        { }

        /// <summary>
        /// Creates a new file logging handler with the given <see cref="FileStream"/> as the target.
        /// </summary>
        /// <param name="fileStream">The file to write to.</param>
        public FileLoggingHandler(FileStream fileStream)
        {
            fileStream.SetLength(0);
            _streamWriter = new StreamWriter(fileStream);
            _streamWriter.AutoFlush = true;
        }

        /// <inheritdoc/>
        public override void Debug(Func<object> messageProducer) => Log(messageProducer().ToString());

        /// <inheritdoc/>
        public override void Error(Func<object> messageProducer) => Log(messageProducer().ToString());

        /// <inheritdoc/>
        public override void Fatal(Func<object> messageProducer) => Log(messageProducer().ToString());

        /// <inheritdoc/>
        public override void Info(Func<object> messageProducer) => Log(messageProducer().ToString());

        /// <summary>
        /// Writes a message prefixed with a timestamp to the log file.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public void Log(string message)
        {
            lock (_streamWriter)
            {
                _streamWriter.WriteLine($"[{DateTime.Now:HH:mm:ss:ffff}] {message}");
            }
        }

        /// <inheritdoc/>
        public override void Trace(Func<object> messageProducer) => Log(messageProducer().ToString());

        /// <inheritdoc/>
        public override void Warn(Func<object> messageProducer) => Log(messageProducer().ToString());
    }
}