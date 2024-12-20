using FrooxEngine.UIX;
using FrooxEngine;
using MonkeyLoader.Resonite.Events;
using System.Reflection;
using MonkeyLoader.Events;

namespace MonkeyLoader.Resonite.UI.Inspectors
{
    /// <summary>
    /// Represents the base class for the events fired during construction of a <see cref="MemberEditor"/>
    /// </summary>
    /// <remarks>
    /// This base class is dispatched as an event as well.
    /// </remarks>
    [DispatchableBaseEvent]
    public abstract class BuildMemberEditorEvent : CancelableBuildUIEvent
    {
        /// <summary>
        /// Gets the reflection data for the field that stores the <see cref="Member">Member</see>
        /// that a <see cref="MemberEditor"/> is being constructed for.
        /// </summary>
        public FieldInfo FieldInfo { get; }

        /// <summary>
        /// Gets the size for the <see cref="Name">Name</see> label
        /// that should be displayed for the <see cref="MemberEditor"/>.<br/>
        /// Not all member editors get passed a label size.
        /// </summary>
        public float? LabelSize { get; }

        /// <summary>
        /// Gets the sync member that a <see cref="MemberEditor"/> is being constructed for.
        /// </summary>
        public ISyncMember Member { get; }

        /// <summary>
        /// Gets the name that should be used for the <see cref="MemberEditor"/> label.
        /// </summary>
        public string Name { get; }

        internal BuildMemberEditorEvent(ISyncMember member, string name, FieldInfo fieldInfo, UIBuilder ui, float? labelSize = null)
            : base(ui)
        {
            Member = member;
            Name = name;
            FieldInfo = fieldInfo;
            LabelSize = labelSize;
        }
    }
}