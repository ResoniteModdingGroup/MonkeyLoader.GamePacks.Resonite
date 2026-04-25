using FrooxEngine;
using MonkeyLoader;
using MonkeyLoader.Resonite.UI.ContextMenus;
using System.Diagnostics.CodeAnalysis;

namespace MonkeyLoader.Resonite.UI.Inspectors
{
    /// <summary>
    /// Represents the event data for the <see cref="InspectorMemberActions"/> <see cref="ContextMenu"/> items generation event.
    /// </summary>
    public sealed class InspectorMemberActionsMenuItemsGenerationEvent : ContextMenuItemsGenerationEvent<InspectorMemberActions>, ITargetSyncMemberEvent
    {
        /// <summary>
        /// Gets the index of the <see cref="Target">Target</see> in the
        /// <see cref="SkinnedMesh">SkinnedMesh</see>'s <see cref="SkinnedMeshRenderer.BlendShapeWeights">blend shapes</see>.
        /// </summary>
        /// <value>
        /// The blend shape index if the <see cref="Target">Target</see>
        /// <see cref="HasSkinnedMesh">is on</see> a <see cref="SkinnedMeshRenderer"/>;
        /// otherwise <see langword="null"/>.</value>
        public int? BlendshapeIndex { get; }

        /// <summary>
        /// Gets the <see cref="FrooxEngine.ButtonEventData">data</see> for the button press
        /// that triggered opening the <see cref="ContextMenu">ContextMenu</see>.
        /// </summary>
        [Obsolete("Not available anymore.")]
        public ButtonEventData ButtonEventData { get; }

        /// <summary>
        /// Gets whether the <see cref="Target">Target</see> is on a <see cref="SkinnedMeshRenderer"/>.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if <see cref="SkinnedMesh">SkinnedMesh</see>
        /// and <see cref="BlendshapeIndex">BlendShapeIndex</see> are not
        /// <see langword="null"/>; otherwise, <see langword="false"/>.
        /// </value>
        [MemberNotNullWhen(true, nameof(SkinnedMesh), nameof(BlendshapeIndex))]
        public bool HasSkinnedMesh => SkinnedMesh is not null;

        /// <summary>
        /// Gets the <see cref="InspectorMemberActions"/> that the
        /// <see cref="ContextMenuItemsGenerationEvent.ContextMenu">ContextMenu</see>
        /// is being summoned by.
        /// </summary>
        [Obsolete("Use Summoner instead.")]
        public InspectorMemberActions MemberActions => Summoner;

        /// <summary>
        /// Gets the <see cref="SkinnedMeshRenderer"/> that the <see cref="Target">Target</see>
        /// is a <see cref="SkinnedMeshRenderer.BlendShapeWeights">blend shape</see> for.
        /// </summary>
        public SkinnedMeshRenderer? SkinnedMesh { get; }

        /// <inheritdoc/>
        public Slot? Slot { get; }

        /// <summary>
        /// Gets the target <see cref="ISyncMember"/> of the <see cref="InspectorMemberActions"/> that
        /// the <see cref="ContextMenuItemsGenerationEvent.ContextMenu">ContextMenu</see> was opened for.
        /// </summary>
        public ISyncMember Target { get; }

        /// <inheritdoc/>
        public User? User { get; }

        /// <inheritdoc/>
        public InspectorMemberActionsMenuItemsGenerationEvent(ContextMenu contextMenu) : base(contextMenu)
        {
            Target = Summoner.Member.Target;
            SkinnedMesh = Summoner.SkinnedMesh;
            // The has-check is important, otherwise the property throws an exception
            BlendshapeIndex = HasSkinnedMesh ? Summoner.BlendshapeIndex : null;

            Slot = Target.FindNearestParent<Slot>();
            User = Slot?.ActiveUser ?? Target.FindNearestParent<User>();
        }
    }
}