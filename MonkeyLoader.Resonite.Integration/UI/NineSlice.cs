using Elements.Core;
using FrooxEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static MonkeyLoader.Resonite.UI.Slices;
using Half = MonkeyLoader.Resonite.UI.Slices.Half;

namespace MonkeyLoader.Resonite.UI
{
    /// <summary>
    /// Represents a nine slice with its <see cref="VerticalThirds">vertical</see>
    /// and <see cref="HorizontalThirds">horizontal</see> <see cref="Third"/> segments.<br/>
    /// Allows getting the <see cref="Rect"/> and <see cref="float4">Borders</see>
    /// for each of the ways to further divide it.
    /// </summary>
    public readonly struct NineSlice
    {
        /// <summary>
        /// Gets the thirds splitting this nine slice
        /// <see cref="Horizontal">horizontally</see> into Left, Center, and Right.
        /// </summary>
        public Thirds HorizontalThirds { get; }

        /// <summary>
        /// Gets the thirds splitting this nine slice
        /// <see cref="Vertical">vertically</see> into Bottom, Middle, and Top.
        /// </summary>
        public Thirds VerticalThirds { get; }

        /// <summary>
        /// Creates a new nine slice instance using the given <see cref="Thirds"/>.
        /// </summary>
        /// <param name="verticalThirds">The thirds splitting this nine slice <see cref="Vertical">vertically</see> into Bottom, Middle, and Top.</param>
        /// <param name="horizontalThirds">The thirds splitting this nine slice <see cref="Horizontal">horizontally</see> into Left, Center, and Right.</param>
        public NineSlice(Thirds verticalThirds, Thirds horizontalThirds)
        {
            VerticalThirds = verticalThirds;
            HorizontalThirds = horizontalThirds;
        }

        /// <summary>
        /// Creates a new nine slice instance by converting the given edge definitions
        /// into the <see cref="Thirds"/> of it.
        /// </summary>
        /// <remarks>
        /// The <see cref="float4"/>'s values are to be seen as the absolute positions of the:<br/>
        /// Outer edge of 1st third, shared edge of 1st and 2nd third, shared edge of 2nd and 3rd third, and outer edge of 3rd third.
        /// </remarks>
        /// <param name="verticalEdges">The edges splitting this nine slice <see cref="Vertical">vertically</see> into Bottom, Middle, and Top.</param>
        /// <param name="horizontalEdges">The edges splitting this nine slice <see cref="Horizontal">horizontally</see> into Left, Center, and Right.</param>
        public NineSlice(float4 verticalEdges, float4 horizontalEdges)
            : this(new Thirds(verticalEdges), new Thirds(horizontalEdges))
        { }

        /// <summary>
        /// Configures the given <see cref="SpriteProvider"/> with the nine slice <see cref="Rect"/>
        /// and borders using the given vertical and horizontal slices of the <see cref="Thirds"/>.
        /// </summary>
        /// <param name="spriteProvider">The sprite provider to configure.</param>
        /// <param name="vertical">The <see cref="Vertical"/> slice to use.</param>
        /// <param name="horizontal">The <see cref="Horizontal"/> slice to use.</param>
        /// <exception cref="ArgumentException">When the <paramref name="vertical"/>-value or <paramref name="horizontal"/>-value is invalid.</exception>
        public void Configure(SpriteProvider spriteProvider, Vertical vertical, Horizontal horizontal)
        {
            spriteProvider.Rect.Value = GetRect(vertical, horizontal);
            spriteProvider.Borders.Value = GetBorders(vertical, horizontal);
        }

        /// <summary>
        /// Configures the given <see cref="SpriteProvider"/> with the nine slice <see cref="Rect"/>
        /// and borders using the whole horizontal area and the given vertical slice of the <see cref="Thirds"/>.
        /// </summary>
        /// <param name="spriteProvider">The sprite provider to configure.</param>
        /// <param name="vertical">The <see cref="Vertical"/> slice to use.</param>
        /// <exception cref="ArgumentException">When the <paramref name="vertical"/>-value is invalid.</exception>
        public void Configure(SpriteProvider spriteProvider, Vertical vertical)
        {
            spriteProvider.Rect.Value = GetRect(vertical);
            spriteProvider.Borders.Value = GetBorders(vertical);
        }

        /// <summary>
        /// Configures the given <see cref="SpriteProvider"/> with the nine slice <see cref="Rect"/>
        /// and borders using the whole vertical area and given horizontal slice of the <see cref="Thirds"/>.
        /// </summary>
        /// <param name="spriteProvider">The sprite provider to configure.</param>
        /// <param name="horizontal">The <see cref="Horizontal"/> slice to use.</param>
        /// <exception cref="ArgumentException">When the <paramref name="horizontal"/>-value is invalid.</exception>
        public void Configure(SpriteProvider spriteProvider, Horizontal horizontal)
        {
            spriteProvider.Rect.Value = GetRect(horizontal);
            spriteProvider.Borders.Value = GetBorders(horizontal);
        }

        /// <summary>
        /// Configures the given <see cref="SpriteProvider"/> with the nine slice <see cref="Rect"/>
        /// and borders using the whole area of the <see cref="Thirds"/>.
        /// </summary>
        /// <param name="spriteProvider">The sprite provider to configure.</param>
        public void Configure(SpriteProvider spriteProvider)
        {
            spriteProvider.Rect.Value = GetRect();
            spriteProvider.Borders.Value = GetBorders();
        }

        /// <summary>
        /// Gets the nine slice borders using the whole horizontal area and
        /// given <paramref name="vertical"/> slice of the <see cref="Thirds"/>.
        /// </summary>
        /// <param name="vertical">The <see cref="Vertical"/> slice to use.</param>
        /// <returns>The borders for the vertical slice.</returns>
        /// <exception cref="ArgumentException">When the <paramref name="vertical"/>-value is invalid.</exception>
        public float4 GetBorders(Vertical vertical)
        {
            var leftRatio = HorizontalThirds.GetRatio(First.Third);
            var rightRatio = HorizontalThirds.GetRatio(Third.Third);

            return vertical switch
            {
                Bottom => new(leftRatio, VerticalThirds.GetRatio(First.Half), rightRatio, 0),
                Middle => new(leftRatio, 0, rightRatio, 0),
                Top => new(leftRatio, 0, rightRatio, VerticalThirds.GetRatio(Second.Half)),
                _ => ThrowInvalidEnumValue<float4>(nameof(vertical))
            };
        }

        /// <summary>
        /// Gets the nine slice borders using the whole area of the <see cref="Thirds"/>.
        /// </summary>
        /// <returns>The borders for the whole area.</returns>
        public float4 GetBorders()
            => new(HorizontalThirds.GetRatio(First.Third), VerticalThirds.GetRatio(First.Third),
                HorizontalThirds.GetRatio(Third.Third), VerticalThirds.GetRatio(Third.Third));

        /// <summary>
        /// Gets the nine slice borders using the whole vertical area and
        /// given <paramref name="horizontal"/> slice of the <see cref="Thirds"/>.
        /// </summary>
        /// <param name="horizontal">The <see cref="Horizontal"/> slice to use.</param>
        /// <returns>The borders for the horizontal slice.</returns>
        /// <exception cref="ArgumentException">When the <paramref name="horizontal"/>-value is invalid.</exception>
        public float4 GetBorders(Horizontal horizontal)
        {
            var bottomRatio = VerticalThirds.GetRatio(First.Third);
            var topRatio = VerticalThirds.GetRatio(Third.Third);

            return horizontal switch
            {
                Left => new(HorizontalThirds.GetRatio(First.Half), bottomRatio, 0, topRatio),
                Center => new(0, bottomRatio, 0, topRatio),
                Right => new(0, bottomRatio, HorizontalThirds.GetRatio(Second.Half), topRatio),
                _ => ThrowInvalidEnumValue<float4>(nameof(horizontal))
            };
        }

        /// <summary>
        /// Gets the nine slice borders using the given vertical and horizontal slices of the <see cref="Thirds"/>.
        /// </summary>
        /// <param name="vertical">The <see cref="Vertical"/> slice to use.</param>
        /// <param name="horizontal">The <see cref="Horizontal"/> slice to use.</param>
        /// <returns>The borders for the slice.</returns>
        /// <exception cref="ArgumentException">When the <paramref name="vertical"/>-value or <paramref name="horizontal"/>-value is invalid.</exception>
        public float4 GetBorders(Vertical vertical, Horizontal horizontal)
        {
            return (vertical, horizontal) switch
            {
                (Bottom, Left) => new(HorizontalThirds.GetRatio(First.Half), VerticalThirds.GetRatio(First.Half), 0, 0),
                (Bottom, Center) => new(0, VerticalThirds.GetRatio(First.Half), 0, 0),
                (Bottom, Right) => new(0, VerticalThirds.GetRatio(First.Half), HorizontalThirds.GetRatio(Second.Half), 0),
                (Middle, Left) => new(HorizontalThirds.GetRatio(First.Half), 0, 0, 0),
                (Middle, Center) => float4.Zero,
                (Middle, Right) => new(0, 0, HorizontalThirds.GetRatio(Second.Half), 0),
                (Top, Left) => new(HorizontalThirds.GetRatio(First.Half), 0, 0, VerticalThirds.GetRatio(Second.Half)),
                (Top, Center) => new(0, 0, 0, VerticalThirds.GetRatio(Second.Half)),
                (Top, Right) => new(0, 0, HorizontalThirds.GetRatio(Second.Half), VerticalThirds.GetRatio(Second.Half)),
                _ => ThrowInvalidEnumValue<float4>($"{nameof(vertical)} || {nameof(horizontal)}")
            };
        }

        /// <summary>
        /// Gets the nine slice <see cref="Rect"/> using the given vertical and horizontal slices of the <see cref="Thirds"/>.
        /// </summary>
        /// <param name="vertical">The <see cref="Vertical"/> slice to use.</param>
        /// <param name="horizontal">The <see cref="Horizontal"/> slice to use.</param>
        /// <returns>The <see cref="Rect"/> for the slice.</returns>
        /// <exception cref="ArgumentException">When the <paramref name="vertical"/>-value or <paramref name="horizontal"/>-value is invalid.</exception>
        public Rect GetRect(Vertical vertical, Horizontal horizontal)
        {
            return (vertical, horizontal) switch
            {
                (Bottom, Left) => new(HorizontalThirds.Start, VerticalThirds.Start, HorizontalThirds.GetLength(First.Half), VerticalThirds.GetLength(First.Half)),
                (Bottom, Center) => new(HorizontalThirds.Middle, VerticalThirds.Start, 0, VerticalThirds.GetLength(First.Half)),
                (Bottom, Right) => new(HorizontalThirds.Middle, VerticalThirds.Start, HorizontalThirds.GetLength(Second.Half), VerticalThirds.GetLength(First.Half)),
                (Middle, Left) => new(HorizontalThirds.Start, VerticalThirds.Middle, HorizontalThirds.GetLength(First.Half), 0),
                (Middle, Center) => new(HorizontalThirds.Middle, VerticalThirds.Middle, 0, 0),
                (Middle, Right) => new(HorizontalThirds.Middle, VerticalThirds.Middle, HorizontalThirds.GetLength(Second.Half), 0),
                (Top, Left) => new(HorizontalThirds.Start, VerticalThirds.Middle, HorizontalThirds.GetLength(First.Half), VerticalThirds.GetLength(Second.Half)),
                (Top, Center) => new(HorizontalThirds.Middle, VerticalThirds.Middle, 0, VerticalThirds.GetLength(Second.Half)),
                (Top, Right) => new(HorizontalThirds.Middle, VerticalThirds.Middle, HorizontalThirds.GetLength(Second.Half), VerticalThirds.GetLength(Second.Half)),
                _ => ThrowInvalidEnumValue<Rect>($"{nameof(vertical)} || {nameof(horizontal)}")
            };
        }

        /// <summary>
        /// Gets the nine slice <see cref="Rect"/> using the whole vertical area
        /// and given horizontal slice of the <see cref="Thirds"/>.
        /// </summary>
        /// <param name="horizontal">The <see cref="Horizontal"/> slice to use.</param>
        /// <returns>The <see cref="Rect"/> for the horizontal slice.</returns>
        /// <exception cref="ArgumentException">When the <paramref name="horizontal"/>-value is invalid.</exception>
        public Rect GetRect(Horizontal horizontal)
        {
            var bottom = VerticalThirds.Start;
            var height = VerticalThirds.Length;

            return horizontal switch
            {
                Left => new(HorizontalThirds.Start, bottom, HorizontalThirds.GetLength(First.Half), height),
                Center => new(HorizontalThirds.Middle, bottom, 0, height),
                Right => new(0, bottom, HorizontalThirds.GetLength(Second.Half), height),
                _ => ThrowInvalidEnumValue<Rect>(nameof(horizontal))
            };
        }

        /// <summary>
        /// Gets the nine slice <see cref="Rect"/> using the whole area of the <see cref="Thirds"/>.
        /// </summary>
        /// <returns>The <see cref="Rect"/> for the whole area.</returns>
        public Rect GetRect()
            => new(HorizontalThirds.Start, VerticalThirds.Start,
                HorizontalThirds.Length, VerticalThirds.Length);

        /// <summary>
        /// Gets the nine slice <see cref="Rect"/> using the whole horizontal area
        /// and the given vertical slice of the <see cref="Thirds"/>.
        /// </summary>
        /// <param name="vertical">The <see cref="Vertical"/> slice to use.</param>
        /// <returns>The <see cref="Rect"/> for the vertical slice.</returns>
        /// <exception cref="ArgumentException">When the <paramref name="vertical"/>-value is invalid.</exception>
        public Rect GetRect(Vertical vertical)
        {
            var left = HorizontalThirds.Start;
            var width = HorizontalThirds.Length;

            return vertical switch
            {
                Bottom => new(left, VerticalThirds.Start, width, VerticalThirds.GetLength(First.Half)),
                Middle => new(left, VerticalThirds.Middle, width, 0),
                Top => new(left, 0, width, VerticalThirds.GetLength(Second.Half)),
                _ => ThrowInvalidEnumValue<Rect>(nameof(vertical))
            };
        }

        [DoesNotReturn]
        private static T ThrowInvalidEnumValue<T>(string paramName)
            => throw new ArgumentException("Invalid value!", paramName);

        /// <summary>
        /// Represents the thirds of one axis of a nine slice using a <see cref="float4"/>.
        /// </summary>
        public readonly struct Thirds
        {
            /// <summary>
            /// Gets the raw edge definition of these thirds.
            /// </summary>
            public float4 Edges { get; }

            /// <summary>
            /// Gets the absolute position of the 3rd third's outer edge.
            /// </summary>
            public float End => Edges[3];

            /// <summary>
            /// Gets the absolute length of these thirds from
            /// <see cref="Start">Start</see> to <see cref="End">End</see>.
            /// </summary>
            public float Length => Edges[3] - Edges[0];

            /// <summary>
            /// Gets the absolute middle position of the 2nd third.
            /// </summary>
            public float Middle => (Edges[1] + Edges[2]) / 2;

            /// <summary>
            /// Gets the absolute position of the 1st third's outer edge.
            /// </summary>
            public float Start => Edges[0];

            /// <summary>
            /// Creates a new thirds instance using the given edge definition.
            /// </summary>
            /// <remarks>
            /// The <see cref="float4"/>'s values are to be seen as the absolute positions of the:<br/>
            /// Outer edge of 1st third, shared edge of 1st and 2nd third, shared edge of 2nd and 3rd third, and outer edge of 3rd third.
            /// </remarks>
            /// <param name="edges">The edge definition to use.</param>
            /// <exception cref="ArgumentException">When the <paramref name="edges"/>-values aren't monotonically growing.</exception>
            public Thirds(float4 edges)
            {
                if (edges[3] < edges[2] || edges[2] < edges[1] || edges[1] < edges[0])
                    throw new ArgumentException("Edge values must be monotonically growing!", nameof(edges));

                Edges = edges;
            }

            /// <summary>
            /// Gets the absolute length between <see cref="Start">Start</see> and
            /// <see cref="Middle">Middle</see>, or <see cref="Middle">Middle</see> and <see cref="End">End</see>.
            /// </summary>
            /// <param name="half">The half to get the length of.</param>
            /// <returns>The absolute length of that half.</returns>
            /// <exception cref="ArgumentException">When the <paramref name="half"/>-value is invalid.</exception>
            public float GetLength(Half half)
                => half switch
                {
                    Half.First => Middle - Edges[0],
                    Half.Second => Edges[3] - Middle,
                    _ => ThrowInvalidEnumValue<float>(nameof(half))
                };

            /// <summary>
            /// Gets the ratio of the 1st third's length to the first half,
            /// or the 3rd third's to the second half.
            /// </summary>
            /// <param name="half">The half to get the ratio for.</param>
            /// <returns>The ratio of outer third to half.</returns>
            /// <exception cref="ArgumentException">When the <paramref name="half"/>-value is invalid.</exception>
            public float GetRatio(Half half)
                => half switch
                {
                    Half.First => (Edges[1] - Edges[0]) / GetLength(Half.First),
                    Half.Second => (Edges[3] - Edges[2]) / GetLength(Half.Second),
                    _ => ThrowInvalidEnumValue<float>(nameof(half))
                };

            /// <summary>
            /// Gets the ratio of a third's length to the <see cref="Length">total length</see> of these thirds.
            /// </summary>
            /// <param name="third">The third to get the ratio for.</param>
            /// <returns>The ratio of a third to the total.</returns>
            /// <exception cref="ArgumentException">When the <paramref name="third"/>-value is invalid.</exception>
            public float GetRatio(Third third)
                => third switch
                {
                    Third.First => (Edges[1] - Edges[0]) / Length,
                    Third.Second => (Edges[2] - Edges[1]) / Length,
                    Third.Third => (Edges[3] - Edges[2]) / Length,
                    _ => ThrowInvalidEnumValue<float>(nameof(third))
                };
        }
    }
}