using FrooxEngine;
using MonkeyLoader.Events;
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
    public sealed class InspectorMemberActionsMenuItemsGenerationEvent : AsyncEvent
    {
        /// <summary>
        /// Gets the <see cref="ButtonEventData">data</see> for the button press
        /// that triggered opening the <see cref="ContextMenu">ContextMenu</see>.
        /// </summary>
        public ButtonEventData ButtonEventData { get; }

        /// <summary>
        /// Gets the <see cref="FrooxEngine.ContextMenu"/> that was opened.
        /// </summary>
        /// <remarks>
        /// You need to pass the <see cref="MemberActions">MemberActions</see>
        /// when <see cref="ContextMenuExtensions.CloseContextMenu">closing</see>
        /// the <see cref="ContextMenu">ContextMenu</see> from your added event handlers.
        /// </remarks>
        public ContextMenu ContextMenu { get; }

        /// <summary>
        /// Gets the <see cref="InspectorMemberActions"/> component
        /// that triggered opening the <see cref="ContextMenu">ContextMenu</see>.
        /// </summary>
        public InspectorMemberActions MemberActions { get; }

        /// <summary>
        /// Gets the <see cref="ISyncMember"/> that
        /// the <see cref="ContextMenu">ContextMenu</see> was opened for.
        /// </summary>
        public ISyncMember Target { get; }

        internal InspectorMemberActionsMenuItemsGenerationEvent(
            InspectorMemberActions instance, ButtonEventData buttonEventData, ISyncMember target)
        {
            MemberActions = instance;
            ButtonEventData = buttonEventData;
            Target = target;
            ContextMenu = instance.LocalUser.GetUserContextMenu();
        }
    }
}