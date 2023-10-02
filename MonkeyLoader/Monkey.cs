using MonkeyLoader.Logging;

namespace MonkeyLoader
{
    public sealed class Monkey
    {
        /// <summary>
        /// Gets the <see cref="MonkeyLogger"/> that this pre-patcher can use to log messages to game-specific channels.
        /// </summary>
        public MonkeyLogger Logger { get; internal set; }
    }
}