using FrooxEngine;
using FrooxEngine.UIX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.UI
{
    /// <summary>
    /// Contains helper methods to generate common elements of <see cref="WorkerInspector"/>s.
    /// </summary>
    public static class InspectorHelper
    {
        /// <summary>
        /// Gets an <see cref="IComparer{T}"/> that compares <see cref="ICustomInspectorSegment"/>s
        /// based on their <see cref="ICustomInspectorSegment.Priority">priority</see>.
        /// </summary>
        public static IComparer<ICustomInspectorSegment> CustomInspectorSegmentComparer { get; } = new CustomInspectorSegmentComparerImpl();

        public static void BuildOneWayReferenceSegmentUI<TReference>(UIBuilder ui, string name, TReference reference)
            where TReference : class, IWorldElement
        {
            var refSlot = ui.Current.AddSlot($"{name}-Reference");
            refSlot.DestroyWhenLocalUserLeaves();

            var referenceField = refSlot.AttachComponent<ReferenceField<TReference>>();
            referenceField.Reference.Target = reference;
            referenceField.Reference.DriveFrom(referenceField.Reference);

            SyncMemberEditorBuilder.Build(referenceField.Reference, name,
                referenceField.GetSyncMemberFieldInfo(nameof(ReferenceField<TReference>.Reference)), ui);

            ui.Current[ui.Current.ChildrenCount - 1].DestroyWhenLocalUserLeaves();
        }

        private sealed class CustomInspectorSegmentComparerImpl : IComparer<ICustomInspectorSegment>
        {
            public int Compare(ICustomInspectorSegment x, ICustomInspectorSegment y)
            {
                if (ReferenceEquals(x, y))
                    return 0;

                var priorityComparison = x.Priority.CompareTo(y.Priority);

                if (priorityComparison != 0)
                    return priorityComparison;

                // TODO: something better reeee
                // not really suitable x.x
                return x.GetHashCode() - y.GetHashCode();
            }
        }
    }
}