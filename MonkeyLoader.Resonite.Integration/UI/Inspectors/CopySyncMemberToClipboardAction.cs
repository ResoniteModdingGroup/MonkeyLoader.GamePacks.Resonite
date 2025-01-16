using FrooxEngine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.UI.Inspectors
{
    internal sealed class CopySyncMemberToClipboardAction
        : ResoniteAsyncEventHandlerMonkey<CopySyncMemberToClipboardAction, GenerateInspectorMemberActionsMenuItemsAsyncEvent>
    {
        public override int Priority => HarmonyLib.Priority.Normal;

        protected override bool AppliesTo(GenerateInspectorMemberActionsMenuItemsAsyncEvent eventData)
            => base.AppliesTo(eventData) && eventData.Target is IField;

        protected override Task Handle(GenerateInspectorMemberActionsMenuItemsAsyncEvent eventData)
        {
            var field = (IField)eventData.Target;
            var menuItem = eventData.ContextMenu.AddItem("Copy to Clipboard",
                OfficialAssets.Graphics.Icons.General.Duplicate, RadiantUI_Constants.Sub.GREEN);

            menuItem.Button.LocalPressed += (button, _) =>
            {
                button.World.InputInterface.Clipboard.SetText(field.BoxedValue.ToString());
                button.World.LocalUser.CloseContextMenu(eventData.Instance);
            };

            return Task.CompletedTask;
        }
    }
}