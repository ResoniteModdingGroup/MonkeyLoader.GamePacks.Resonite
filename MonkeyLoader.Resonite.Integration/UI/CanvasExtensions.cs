using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLoader.Resonite.UI
{
    /// <summary>
    /// Contains helpful methods to deal with UIX <see cref="Canvas"/>es,
    /// their <see cref="RectTransform"/>s, and related types.
    /// </summary>
    public static class CanvasExtensions
    {
        /// <summary>
        /// Determines whether the given <paramref name="point"/>
        /// is within this <see cref="BoundingBox2D"/>.
        /// </summary>
        /// <param name="boundingBox">The bounding box to determine inclusion in.</param>
        /// <param name="point">The point to determine inclusion for.</param>
        /// <returns><c>true</c> if the <paramref name="point"/> is inside this <see cref="BoundingBox2D"/>; otherwise, <c>false</c>.</returns>
        public static bool Contains(this BoundingBox2D boundingBox, float2 point)
            => (point >= boundingBox.Min).All() && (point <= boundingBox.Max).All();

        /// <summary>
        /// Gets this <see cref="RectTransform"/>'s
        /// <see cref="RectTransform.ComputeGlobalComputeRect">global compute rect</see>
        /// in the <see cref="Canvas"/>'s <see cref="Canvas.UnitScale">Unit Scale</see>.
        /// </summary>
        /// <param name="rectTransform">The rect transform to determine the canvas unit scale bounds of.</param>
        /// <returns>The 2D bounding box of this <see cref="RectTransform"/> within its <see cref="Canvas"/>'s <see cref="Slot"/>'s local space.</returns>
        public static BoundingBox2D GetCanvasBounds(this RectTransform rectTransform)
        {
            var area = rectTransform.ComputeGlobalComputeRect();

            var bounds = BoundingBox2D.Empty();
            bounds.Encapsulate(area.ExtentMin / rectTransform.Canvas.UnitScale);
            bounds.Encapsulate(area.ExtentMax / rectTransform.Canvas.UnitScale);

            return bounds;
        }

        /// <summary>
        /// Gets this <see cref="RectTransform"/>'s
        /// <see cref="RectTransform.ComputeGlobalComputeRect">global compute rect's</see>
        /// <see cref="BoundingBox"/> in the global space.
        /// </summary>
        /// <param name="rectTransform">The rect transform to determine the global bounds of.</param>
        /// <returns>The 3D bounding box of this <see cref="RectTransform"/> within the global space.</returns>
        public static BoundingBox GetGlobalBounds(this RectTransform rectTransform)
        {
            var area = rectTransform.ComputeGlobalComputeRect();

            var bounds = BoundingBox.Empty();
            bounds.Encapsulate(rectTransform.Canvas.Slot.LocalPointToGlobal(area.ExtentMin / rectTransform.Canvas.UnitScale));
            bounds.Encapsulate(rectTransform.Canvas.Slot.LocalPointToGlobal(area.ExtentMax / rectTransform.Canvas.UnitScale));

            return bounds;
        }

        /// <summary>
        /// Resets this <see cref="RectTransform"/> to the default values.
        /// </summary>
        /// <param name="rectTransform">The rect transform to reset.</param>
        public static void Reset(this RectTransform rectTransform)
        {
            rectTransform.AnchorMin.Value = float2.Zero;
            rectTransform.AnchorMax.Value = float2.One;
            rectTransform.OffsetMin.Value = float2.Zero;
            rectTransform.OffsetMax.Value = float2.Zero;
            rectTransform.Pivot.Value = new(.5f, .5f);
        }
    }
}