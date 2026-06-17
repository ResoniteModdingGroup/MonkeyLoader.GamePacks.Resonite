using System.Runtime.CompilerServices;

namespace MonkeyLoader.Resonite.UI
{
    /// <summary>
    /// Contains definitions to work with <see cref="NineSlice"/>.
    /// </summary>
    /// <remarks>
    /// Use with: <c>using static MonkeyLoader.Resonite.UI.Slices;</c>
    /// </remarks>
    [TypeForwardedFrom("MonkeyLoader.Resonite.UI.Slices, MonkeyLoader.Resonite.Integration")]
    public static class Slices
    {
        /// <summary>
        /// Alias for <see cref="Vertical.Bottom"/>.
        /// </summary>
        public const Vertical Bottom = Vertical.Bottom;

        /// <summary>
        /// Alias for <see cref="Horizontal.Center"/>.
        /// </summary>
        public const Horizontal Center = Horizontal.Center;

        /// <summary>
        /// Alias for <see cref="Horizontal.Left"/>.
        /// </summary>
        public const Horizontal Left = Horizontal.Left;

        /// <summary>
        /// Alias for <see cref="Vertical.Middle"/>.
        /// </summary>
        public const Vertical Middle = Vertical.Middle;

        /// <summary>
        /// Alias for <see cref="Horizontal.Right"/>.
        /// </summary>
        public const Horizontal Right = Horizontal.Right;

        /// <summary>
        /// Alias for <see cref="Vertical.Top"/>.
        /// </summary>
        public const Vertical Top = Vertical.Top;

        /// <summary>
        /// Contains more "fluent" definitions for first half / third.
        /// </summary>
        [TypeForwardedFrom("MonkeyLoader.Resonite.UI.Slices+First, MonkeyLoader.Resonite.Integration")]
        public static class First
        {
            /// <summary>
            /// Alias for <see cref="Half.First"/>.
            /// </summary>
            public const Half Half = Slices.Half.First;

            /// <summary>
            /// Alias for <see cref="Third.First"/>.
            /// </summary>
            public const Third Third = Slices.Third.First;
        }

        /// <summary>
        /// Contains more "fluent" definitions for second half / third.
        /// </summary>
        [TypeForwardedFrom("MonkeyLoader.Resonite.UI.Slices+Second, MonkeyLoader.Resonite.Integration")]
        public static class Second
        {
            /// <summary>
            /// Alias for <see cref="Half.Second"/>.
            /// </summary>
            public const Half Half = Slices.Half.Second;

            /// <summary>
            /// Alias for <see cref="Third.Second"/>.
            /// </summary>
            public const Third Third = Slices.Third.Second;
        }

        /// <summary>
        /// Distinguishes halfs.
        /// </summary>
        [TypeForwardedFrom("MonkeyLoader.Resonite.UI.Slices+Half, MonkeyLoader.Resonite.Integration")]
        public enum Half
        {
            /// <summary>
            /// The 1st half.
            /// </summary>
            First,

            /// <summary>
            /// The 2nd half.
            /// </summary>
            Second
        }

        /// <summary>
        /// Distinguishes horizontal slices.
        /// </summary>
        [TypeForwardedFrom("MonkeyLoader.Resonite.UI.Slices+Horizontal, MonkeyLoader.Resonite.Integration")]
        public enum Horizontal
        {
            /// <summary>
            /// The 1st horizontal slice.
            /// </summary>
            Left,

            /// <summary>
            /// The 2nd horizontal slice.
            /// </summary>
            Center,

            /// <summary>
            /// The 3rd horizontal slice.
            /// </summary>
            Right
        }

        /// <summary>
        /// Distinguishes thirds.
        /// </summary>
        [TypeForwardedFrom("MonkeyLoader.Resonite.UI.Slices+Third, MonkeyLoader.Resonite.Integration")]
        public enum Third
        {
            /// <summary>
            /// The 1st third.
            /// </summary>
            First,

            /// <summary>
            /// The 2nd third.
            /// </summary>
            Second,

            /// <summary>
            /// The 3rd third.
            /// </summary>
            Third
        }

        /// <summary>
        /// Distinguishes vertical slices.
        /// </summary>
        [TypeForwardedFrom("MonkeyLoader.Resonite.UI.Slices+Vertical, MonkeyLoader.Resonite.Integration")]
        public enum Vertical
        {
            /// <summary>
            /// The 1st vertical slice.
            /// </summary>
            Bottom,

            /// <summary>
            /// The 2nd vertical slice.
            /// </summary>
            Middle,

            /// <summary>
            /// The 3rd vertical slice.
            /// </summary>
            Top
        }
    }
}