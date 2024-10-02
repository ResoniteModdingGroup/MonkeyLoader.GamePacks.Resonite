using FrooxEngine.UIX;
using FrooxEngine;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyLoader.Resonite.UI.Inspectors
{
    /// <summary>
    /// Represents the event fired during construction of a <see cref="MemberEditor"/> for an <see cref="ISyncArray"/>.
    /// </summary>
    public sealed class BuildArrayEditorEvent : BuildMemberEditorEvent
    {
        /// <summary>
        /// Gets the sync array that a <see cref="MemberEditor"/> is being constructed for.
        /// </summary>
        public new ISyncArray Member { get; }

        internal BuildArrayEditorEvent(ISyncArray member, string name, FieldInfo fieldInfo, UIBuilder ui, float labelSize)
            : base(member, name, fieldInfo, ui, labelSize)
        {
            Member = member;
        }
    }

    /// <summary>
    /// Represents the event fired during construction of a <see cref="MemberEditor"/> for an <see cref="ISyncBag"/>.
    /// </summary>
    public sealed class BuildBagEditorEvent : BuildMemberEditorEvent
    {
        /// <summary>
        /// Gets the sync bag that a <see cref="MemberEditor"/> is being constructed for.
        /// </summary>
        public new ISyncBag Member { get; }

        internal BuildBagEditorEvent(ISyncBag member, string name, FieldInfo fieldInfo, UIBuilder ui)
            : base(member, name, fieldInfo, ui)
        {
            Member = member;
        }
    }

    /// <summary>
    /// Represents the event fired during construction of a <see cref="MemberEditor"/> for an <see cref="IField"/>.
    /// </summary>
    public sealed class BuildFieldEditorEvent : BuildMemberEditorEvent
    {
        /// <summary>
        /// Gets the field that a <see cref="MemberEditor"/> is being constructed for.
        /// </summary>
        public new IField Member { get; }

        internal BuildFieldEditorEvent(IField member, string name, FieldInfo fieldInfo, UIBuilder ui, float labelSize)
            : base(member, name, fieldInfo, ui, labelSize)
        {
            Member = member;
        }
    }

    /// <summary>
    /// Represents the event fired during construction of a <see cref="MemberEditor"/> for an <see cref="ISyncList"/>.
    /// </summary>
    public sealed class BuildListEditorEvent : BuildMemberEditorEvent
    {
        /// <summary>
        /// Gets the sync list that a <see cref="MemberEditor"/> is being constructed for.
        /// </summary>
        public new ISyncList Member { get; }

        internal BuildListEditorEvent(ISyncList member, string name, FieldInfo fieldInfo, UIBuilder ui)
            : base(member, name, fieldInfo, ui)
        {
            Member = member;
        }
    }

    /// <summary>
    /// Represents the event fired during construction of a <see cref="MemberEditor"/> for an <see cref="SyncObject"/>.
    /// </summary>
    public sealed class BuildObjectEditorEvent : BuildMemberEditorEvent
    {
        /// <summary>
        /// Gets the sync object that a <see cref="MemberEditor"/> is being constructed for.
        /// </summary>
        public new SyncObject Member { get; }

        internal BuildObjectEditorEvent(SyncObject member, string name, FieldInfo fieldInfo, UIBuilder ui, float labelSize)
            : base(member, name, fieldInfo, ui, labelSize)
        {
            Member = member;
        }
    }

    /// <summary>
    /// Represents the event fired during construction of a <see cref="MemberEditor"/> for an <see cref="SyncPlayback"/>.
    /// </summary>
    public sealed class BuildPlaybackEditorEvent : BuildMemberEditorEvent
    {
        /// <summary>
        /// Gets the sync playback that a <see cref="MemberEditor"/> is being constructed for.
        /// </summary>
        public new SyncPlayback Member { get; }

        internal BuildPlaybackEditorEvent(SyncPlayback member, string name, FieldInfo fieldInfo, UIBuilder ui, float labelSize)
            : base(member, name, fieldInfo, ui, labelSize)
        {
            Member = member;
        }
    }
}