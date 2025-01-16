using FrooxEngine;
using MonkeyLoader.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLoader.Resonite.UI.Inspectors
{
    public sealed class GenerateInspectorMemberActionsMenuItemsAsyncEvent : AsyncEvent
    {
        public ButtonEventData ButtonEventData { get; }
        public ContextMenu ContextMenu { get; }
        public InspectorMemberActions Instance { get; }
        public ISyncMember Target { get; }

        internal GenerateInspectorMemberActionsMenuItemsAsyncEvent(InspectorMemberActions instance, ButtonEventData buttonEventData, ISyncMember target)
        {
            Instance = instance;
            ButtonEventData = buttonEventData;
            Target = target;
            ContextMenu = instance.LocalUser.GetUserContextMenu();
        }
    }
}