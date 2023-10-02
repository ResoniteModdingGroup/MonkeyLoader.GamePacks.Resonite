using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Logging
{
    /// <summary>
    /// Contains the logging functionality mods and patchers can use to log messages to game-specific channels.
    /// </summary>
    public sealed class MonkeyLogger
    {
        /// <summary>
        /// Gets or sets the <see cref="ILoggingHandler"/> used to send logging requests to the game-specific channels.
        /// </summary>
        public ILoggingHandler Handler { get; set; }

        /// <summary>
        /// Gets or sets the current <see cref="LoggingLevel"/> used to filter requests.
        /// </summary>
        public LoggingLevel Level { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="MonkeyLogger"/> class with the given <see cref="ILoggingHandler"/> to handle requests.
        /// </summary>
        /// <param name="handler">The <see cref="ILoggingHandler"/> used to send logging requests to the game-specific channels.</param>
        public MonkeyLogger(ILoggingHandler handler)
        {
            Handler = handler;
        }

        /// <summary>
        /// Logs events considered to be useful during debugging when more granular information is needed.
        /// </summary>
        /// <param name="messageProducer">The producer to log if possible.</param>
        public void Debug(Func<object> messageProducer) => logInternal(LoggingLevel.Debug, messageProducer);

        /// <summary>
        /// Logs that one or more functionalities are not working, preventing some from working correctly.
        /// </summary>
        /// <param name="messageProducer">The producer to log if possible.</param>
        public void Error(Func<object> messageProducer) => logInternal(LoggingLevel.Error, messageProducer);

        /// <summary>
        /// Logs that one or more key functionalities, or the whole system isn't working.
        /// </summary>
        /// <param name="messageProducer">The producer to log if possible.</param>
        public void Fatal(Func<object> messageProducer) => logInternal(LoggingLevel.Fatal, messageProducer);

        /// <summary>
        /// Logs that something happened, which is purely informative and can be ignored during normal use.
        /// </summary>
        /// <param name="messageProducer">The producer to log if possible.</param>
        public void Info(Func<object> messageProducer) => logInternal(LoggingLevel.Info, messageProducer);

        /// <summary>
        /// Determines whether the given <see cref="LoggingLevel"/> should be logged at the current <see cref="Level">Level</see>.
        /// </summary>
        /// <param name="level">The <see cref="LoggingLevel"/> to check.</param>
        /// <returns><c>true</c> if the given <see cref="LoggingLevel"/> should be logged right now.</returns>
        public bool ShouldLog(LoggingLevel level) => Level >= level;

        /// <summary>
        /// Logs step by step execution of code that can be ignored during standard operation,
        /// but may be useful during extended debugging sessions.
        /// </summary>
        /// <param name="messageProducer">The producer to log if possible.</param>
        public void Trace(Func<object> messageProducer) => logInternal(LoggingLevel.Trace, messageProducer);

        /// <summary>
        /// Logs that unexpected behavior happened, but work is continuing and the key functionalities are operating as expected.
        /// </summary>
        /// <param name="messageProducer">The producer to log if possible.</param>
        public void Warn(Func<object> messageProducer) => logInternal(LoggingLevel.Warn, messageProducer);

        private void logInternal(LoggingLevel level, Func<object> messageProducer)
        {
            if (!ShouldLog(level))
                return;

            logLevelToLogger(level)(makeMessageProducer(level, messageProducer));
        }

        private Action<Func<object>> logLevelToLogger(LoggingLevel level)
        {
            return level switch
            {
                LoggingLevel.Fatal => Handler.Fatal,
                LoggingLevel.Error => Handler.Error,
                LoggingLevel.Warn => Handler.Warn,
                LoggingLevel.Info => Handler.Info,
                LoggingLevel.Debug => Handler.Debug,
                LoggingLevel.Trace => Handler.Trace,
                _ => _ => { }
            };
        }

        private string logLevelToString(LoggingLevel level)
        {
            return level switch
            {
                LoggingLevel.Fatal => "[FATAL]",
                LoggingLevel.Error => "[ERROR]",
                LoggingLevel.Warn => "[WARN] ",
                LoggingLevel.Info => "[INFO] ",
                LoggingLevel.Debug => "[DEBUG]",
                LoggingLevel.Trace => "[TRACE]",
                _ => "[WHAT?]"
            };
        }

        private Func<object> makeMessageProducer(LoggingLevel level, Func<object> messageProducer)
            => () => $"{logLevelToString(level)} [MonkeyLoader] {messageProducer()}";
    }
}