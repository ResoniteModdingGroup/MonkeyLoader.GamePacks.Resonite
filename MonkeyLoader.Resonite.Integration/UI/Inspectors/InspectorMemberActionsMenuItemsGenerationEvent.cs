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

        static InspectorMemberActionsMenuItemsGenerationEvent()
        {
            AddConcreteEvent<InspectorMemberActions>(contextMenu => new InspectorMemberActionsMenuItemsGenerationEvent(contextMenu), true);
        }

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