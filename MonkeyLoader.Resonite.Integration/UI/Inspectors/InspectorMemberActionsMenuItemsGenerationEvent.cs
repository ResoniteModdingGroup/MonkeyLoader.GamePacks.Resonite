using FrooxEngine;
using MonkeyLoader.Resonite.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLoader.Resonite.UI.Inspectors
{
    /// <summary>
    /// Represents the event data for the Fallback Locale Generation Event.
    /// </summary>
    /// <remarks>
    /// This event can be used by Monkeys that make use of locale keys to inject
    /// programmatically generated keys, if they haven't been defined previously.
    /// </remarks>
    public sealed class InspectorMemberActionsMenuItemsGenerationEvent : ContextMenuItemsGenerationEvent<InspectorMemberActions>
    {
        /// <summary>
        /// Gets the <see cref="ButtonEventData">data</see> for the button press
        /// that triggered opening the <see cref="ContextMenu">ContextMenu</see>.
        /// </summary>
        public ButtonEventData ButtonEventData { get; }

        /// <summary>
        /// Gets the <see cref="InspectorMemberActions"/> that the
        /// <see cref="ContextMenuItemsGenerationEvent.ContextMenu">ContextMenu</see>
        /// is being summoned by.
        /// </summary>
        [Obsolete("Use Summoner instead.")]
        public InspectorMemberActions MemberActions => Summoner;

        /// <summary>
        /// Gets the <see cref="Target">Target</see>'s parent <see cref="FrooxEngine.Slot"/>.
        /// </summary>
        /// <remarks>
        /// This may be <c>null</c> if <see cref="Target">Target</see>'s parent is a <see cref="UserComponent"/>.
        /// </remarks>
        public Slot? Slot { get; }

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

        /// <summary>
        /// Gets the <see cref="ISyncMember"/> that
        /// the <see cref="ContextMenu">ContextMenu</see> was opened for.
        /// </summary>
        public ISyncMember Target { get; }

        internal InspectorMemberActionsMenuItemsGenerationEvent(User summoningUser,
            InspectorMemberActions summoner, ButtonEventData buttonEventData, ISyncMember target)
                : base(summoningUser, summoner)
        {
            ButtonEventData = buttonEventData;
            Target = target;

            Slot = target.FindNearestParent<Slot>();
            User = Slot?.ActiveUser ?? target.FindNearestParent<User>();
        }
    }
}