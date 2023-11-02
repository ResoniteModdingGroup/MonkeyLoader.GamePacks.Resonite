using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Logging
{
    /// <summary>
    /// Implements a <see cref="LoggingHandler"/> that can never log anything.
    /// </summary>
    public sealed class MissingLoggingHandler : LoggingHandler
    {
        /// <summary>
        /// Gets a cached instance of the <see cref="MissingLoggingHandler"/>.
        /// </summary>
        public static MissingLoggingHandler Instance { get; } = new();

        /// <inheritdoc/>
        public override bool Connected => false;

        /// <inheritdoc/>
        public override void Debug(Func<object> messageProducer) => throw new NotImplementedException();

        /// <inheritdoc/>
        public override void Error(Func<object> messageProducer) => throw new NotImplementedException();

        /// <inheritdoc/>
        public override void Fatal(Func<object> messageProducer) => throw new NotImplementedException();

        /// <inheritdoc/>
        public override void Info(Func<object> messageProducer) => throw new NotImplementedException();

        /// <inheritdoc/>
        public override void Trace(Func<object> messageProducer) => throw new NotImplementedException();

        /// <inheritdoc/>
        public override void Warn(Func<object> messageProducer) => throw new NotImplementedException();
    }
}