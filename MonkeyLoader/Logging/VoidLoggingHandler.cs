using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Logging
{
    /// <summary>
    /// Implements a <see cref="LoggingHandler"/> that prevents messages from piling up, but doesn't do anything.<br/>
    /// Set this as the only <see cref="MonkeyLoader.LoggingHandler"/> to void all logging.
    /// </summary>
    public sealed class VoidLoggingHandler : LoggingHandler
    {
        /// <summary>
        /// Gets a cached instance of the <see cref="VoidLoggingHandler"/>.
        /// </summary>
        public static VoidLoggingHandler Instance { get; } = new();

        /// <inheritdoc/>
        public override bool Connected => true;

        /// <inheritdoc/>
        public override void Debug(Func<object> messageProducer)
        { }

        /// <inheritdoc/>
        public override void Error(Func<object> messageProducer)
        { }

        /// <inheritdoc/>
        public override void Fatal(Func<object> messageProducer)
        { }

        /// <inheritdoc/>
        public override void Info(Func<object> messageProducer)
        { }

        /// <inheritdoc/>
        public override void Trace(Func<object> messageProducer)
        { }

        /// <inheritdoc/>
        public override void Warn(Func<object> messageProducer)
        { }
    }
}