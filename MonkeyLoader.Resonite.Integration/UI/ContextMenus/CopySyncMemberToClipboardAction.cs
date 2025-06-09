using FrooxEngine;
using MonkeyLoader.Resonite.UI.Inspectors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.UI.ContextMenus
{
    /// <summary>
    /// This is for demo / testing purposes for the new context menu injection only
    /// </summary>
    internal sealed class CopySyncMemberToClipboardAction
        : ResoniteAsyncEventHandlerMonkey<CopySyncMemberToClipboardAction, InspectorMemberActionsMenuItemsGenerationEvent>
    {
        public override bool CanBeDisabled => true;

        public override int Priority => HarmonyLib.Priority.Normal;

        protected override bool AppliesTo(InspectorMemberActionsMenuItemsGenerationEvent eventData)
            => base.AppliesTo(eventData) && eventData.Target is IField && eventData.Target is not ISyncRef;

        protected override Task Handle(InspectorMemberActionsMenuItemsGenerationEvent eventData)
        {
            Logger.Info(() => "Derived contextmenuitemsgeneration event received!");

            var field = (IField)eventData.Target;
            var menuItem = eventData.ContextMenu.AddItem(Mod.GetLocaleString("Derived event, woooh!"),
                OfficialAssets.Graphics.Icons.General.Duplicate, RadiantUI_Constants.Sub.ORANGE);

            // Context Menu is local user only anyways, no need to use local action button
            menuItem.Button.LocalPressed += (button, _) =>
            {
                button.World.InputInterface.Clipboard.SetText(field.BoxedValue.ToString());
                button.World.LocalUser.CloseContextMenu(eventData.Summoner);
            };

            return Task.CompletedTask;
        }
    }
}