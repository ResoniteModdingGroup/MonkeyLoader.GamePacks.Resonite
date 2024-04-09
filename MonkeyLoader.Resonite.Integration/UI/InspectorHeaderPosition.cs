namespace MonkeyLoader.Resonite.UI
{
    /// <summary>
    /// Describes where added header elements will appear.
    /// </summary>
    public enum InspectorHeaderPosition
    {
        /// <summary>
        /// At the start (left) of the header bar.
        /// </summary>
        Start,

        /// <summary>
        /// To the left of the name segment (same as <see cref="Start"/>).
        /// </summary>
        BeforeName = Start,

        /// <summary>
        /// To the right of the name segment (before duplicate and remove).
        /// </summary>
        AfterName,

        /// <summary>
        /// At the end (right) of the header bar (after duplicate and remove).
        /// </summary>
        End
    }
}