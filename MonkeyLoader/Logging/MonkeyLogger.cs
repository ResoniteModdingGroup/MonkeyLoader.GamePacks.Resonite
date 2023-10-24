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
        /// Gets the identifier that's added to this logger's messages to determine the source.
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// Gets or sets the current <see cref="LoggingLevel"/> used to filter requests.
        /// </summary>
        public LoggingLevel Level { get; set; }

        /// <summary>
        /// Gets the <see cref="MonkeyLoader"/> instance that this logger works for.
        /// </summary>
        public MonkeyLoader Loader { get; }

        /// <summary>
        /// Gets the <see cref="ILoggingHandler"/> used to send logging requests to the game-specific channels.<br/>
        /// Messages need to be queued when this is <c>null</c> and they would've been logged.
        /// </summary>
        private ILoggingHandler? Handler => Loader.LoggingHandler;

        /// <summary>
        /// Creates a new logger instance starting with the same <see cref="Level">LoggingLevel</see>
        /// as the given <paramref name="logger"/> instance and owned by the same <see cref="Loader">MonkeyLoader</see>.
        /// </summary>
        /// <param name="logger">The logger instance to copy.</param>
        /// <param name="extraIdentifier">The extra identifier to append to the <paramref name="logger"/>'s.</param>
        public MonkeyLogger(MonkeyLogger logger, string extraIdentifier)
        {
            Level = logger.Level;
            Loader = logger.Loader;
            Identifier = $"{logger.Identifier}|{extraIdentifier}";
        }

        /// <summary>
        /// Creates a new logger instance starting with <see cref="LoggingLevel.Info"/> working for the given loader.
        /// </summary>
        /// <param name="loader">The loader that this logger works for.</param>
        internal MonkeyLogger(MonkeyLoader loader)
        {
            Loader = loader;
            Level = LoggingLevel.Info;
            Identifier = "MonkeyLoader";
        }

        /// <summary>
        /// Logs events considered to be useful during debugging when more granular information is needed.
        /// </summary>
        /// <param name="messageProducer">The producer to log if possible.</param>
        public void Debug(Func<object> messageProducer) => LogInternal(LoggingLevel.Debug, messageProducer);

        /// <summary>
        /// Logs that one or more functionalities are not working, preventing some from working correctly.
        /// </summary>
        /// <param name="messageProducer">The producer to log if possible.</param>
        public void Error(Func<object> messageProducer) => LogInternal(LoggingLevel.Error, messageProducer);

        /// <summary>
        /// Logs that one or more key functionalities, or the whole system isn't working.
        /// </summary>
        /// <param name="messageProducer">The producer to log if possible.</param>
        public void Fatal(Func<object> messageProducer) => LogInternal(LoggingLevel.Fatal, messageProducer);

        /// <summary>
        /// Logs that something happened, which is purely informative and can be ignored during normal use.
        /// </summary>
        /// <param name="messageProducer">The producer to log if possible.</param>
        public void Info(Func<object> messageProducer) => LogInternal(LoggingLevel.Info, messageProducer);

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
        public void Trace(Func<object> messageProducer) => LogInternal(LoggingLevel.Trace, messageProducer);

        /// <summary>
        /// Logs that unexpected behavior happened, but work is continuing and the key functionalities are operating as expected.
        /// </summary>
        /// <param name="messageProducer">The producer to log if possible.</param>
        public void Warn(Func<object> messageProducer) => LogInternal(LoggingLevel.Warn, messageProducer);

        internal void FlushDeferredMessages()
        {
            lock (Loader.DeferredMessages)
            {
                while (Loader.DeferredMessages.Count > 0)
                {
                    var deferredMessage = Loader.DeferredMessages.Dequeue();
                    LogLevelToLogger(deferredMessage.LoggingLevel)(() => deferredMessage.Message);
                }
            }
        }

        private Action<Func<object>> DeferMessage(LoggingLevel level)
            => (Func<object> messageProducer) =>
            {
                lock (Loader.DeferredMessages)
                    Loader.DeferredMessages.Enqueue(new DeferredMessage(this, level, messageProducer()));
            };

        private void LogInternal(LoggingLevel level, Func<object> messageProducer)
        {
            if (!ShouldLog(level))
                return;

            LogLevelToLogger(level)(MakeMessageProducer(level, messageProducer));
        }

        private Action<Func<object>> LogLevelToLogger(LoggingLevel level)
        {
            if (Handler is null)
                return DeferMessage(level);

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

        private string LogLevelToString(LoggingLevel level) => level switch
        {
            LoggingLevel.Fatal => "[FATAL]",
            LoggingLevel.Error => "[ERROR]",
            LoggingLevel.Warn => "[WARN] ",
            LoggingLevel.Info => "[INFO] ",
            LoggingLevel.Debug => "[DEBUG]",
            LoggingLevel.Trace => "[TRACE]",
            _ => "[WHAT?]"
        };

        private Func<object> MakeMessageProducer(LoggingLevel level, Func<object> messageProducer)
            => () => $"{LogLevelToString(level)} [{Identifier}] {messageProducer()}";

        internal readonly struct DeferredMessage
        {
            public readonly MonkeyLogger Logger;
            public readonly LoggingLevel LoggingLevel;
            public readonly object Message;

            public DeferredMessage(MonkeyLogger logger, LoggingLevel level, object message)
            {
                Logger = logger;
                LoggingLevel = level;
                Message = message;
            }
        }
    }
}