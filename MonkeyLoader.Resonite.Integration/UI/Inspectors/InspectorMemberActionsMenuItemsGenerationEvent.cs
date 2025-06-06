using FrooxEngine;
using MonkeyLoader;
using MonkeyLoader.Resonite.UI.ContextMenus;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MonkeyLoader.Resonite.UI.Inspectors
{
    /// <summary>
    /// Represents the event data for the <see cref="InspectorMemberActions"/> <see cref="ContextMenu"/> items generation event.
    /// </summary>
    public sealed class InspectorMemberActionsMenuItemsGenerationEvent : ContextMenuItemsGenerationEvent<InspectorMemberActions>
    {
        public int? BlendshapeIndex { get; }

        /// <summary>
        /// Gets the <see cref="ButtonEventData">data</see> for the button press
        /// that triggered opening the <see cref="ContextMenu">ContextMenu</see>.
        /// </summary>
        public ButtonEventData ButtonEventData { get; }

        [MemberNotNullWhen(true, nameof(SkinnedMesh), nameof(BlendshapeIndex))]
        public bool HasSkinnedMesh => SkinnedMesh is not null;

        /// <summary>
        /// Gets the <see cref="InspectorMemberActions"/> that the
        /// <see cref="ContextMenuItemsGenerationEvent.ContextMenu">ContextMenu</see>
        /// is being summoned by.
        /// </summary>
        [Obsolete("Use Summoner instead.")]
        public InspectorMemberActions MemberActions => Summoner;

        public SkinnedMeshRenderer? SkinnedMesh { get; }

        /// <summary>
        /// Gets the <see cref="Target">Target</see>'s parent <see cref="FrooxEngine.Slot"/>.
        /// </summary>
        /// <remarks>
        /// This may be <c>null</c> if <see cref="Target">Target</see>'s parent is a <see cref="UserComponent"/>.
        /// </remarks>
        public Slot? Slot { get; }

        /// <summary>
        /// Gets the <see cref="ISyncMember"/> that
        /// the <see cref="ContextMenu">ContextMenu</see> was opened for.
        /// </summary>
        public ISyncMember Target { get; }

        /// <summary>
        /// Gets the <see cref="Target">Target</see>'s <see cref="Slot">Slot</see>'s
        /// <see cref="Slot.ActiveUser">ActiveUser</see>, or its parent <see cref="FrooxEngine.User"/>
        /// if it belongs to a <see cref="UserComponent"/>.
        /// </summary>
        /// <remarks>
        /// When the <see cref="Target">Target</see>'s parent is a <see cref="Component"/>
        /// and its <see cref="Slot">Slot</see> does not have an <see cref="Slot.ActiveUser">ActiveUser</see>,
        /// this will be <c>null</c>.
        /// </remarks>
        public User? User { get; }

        /// <inheritdoc/>
        public InspectorMemberActionsMenuItemsGenerationEvent(ContextMenu contextMenu) : base(contextMenu)
        {
            Target = Summoner.Member.Target;
            SkinnedMesh = Summoner.SkinnedMesh;
            BlendshapeIndex = HasSkinnedMesh ? Summoner.BlendshapeIndex : null;

            Slot = Target.FindNearestParent<Slot>();
            User = Slot?.ActiveUser ?? Target.FindNearestParent<User>();
        }
    }
}