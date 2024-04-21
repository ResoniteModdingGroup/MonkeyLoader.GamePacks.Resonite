using FrooxEngine.UIX;

namespace MonkeyLoader.Resonite.Events
{
    /// <summary>
    /// Abstract base class for all sorts of UI-generation events.
    /// </summary>
    public abstract class BuildUIEvent
    {
        /// <summary>
        /// Gets the <see cref="UIBuilder"/> that should be used to generate extra UI elements.
        /// </summary>
        /// <remarks>
        /// The style and <see cref="UIBuilder.Current"/> target should be the same after handling the event.<br/>
        /// Use <see cref="UIBuilder.PushStyle"/> before making style changes,
        /// and revert them by using <see cref="UIBuilder.PopStyle"/> once done.
        /// </remarks>
        public UIBuilder UI { get; }

        /// <summary>
        /// Creates a new UI-generation event instance with the given <see cref="UIBuilder"/>.
        /// </summary>
        /// <param name="ui">The <see cref="UIBuilder"/> to use while generating extra UI elements.</param>
        protected BuildUIEvent(UIBuilder ui)
        {
            UI = ui;
        }
    }
}