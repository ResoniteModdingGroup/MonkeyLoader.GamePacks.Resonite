using Elements.Core;
using FrooxEngine.UIX;
using FrooxEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLoader.Resonite.UI.Inspectors
{
    [HarmonyPatchCategory(nameof(SlotComponentReceiverMenuInjector))]
    [HarmonyPatch(typeof(SlotComponentReceiver), nameof(SlotComponentReceiver.TryReceive))]
    internal sealed class SlotComponentReceiverMenuInjector : ResoniteMonkey<SlotComponentReceiverMenuInjector>
    {
        public override bool CanBeDisabled => true;

        private static bool Prefix(SlotComponentReceiver __instance, IEnumerable<IGrabbable> items, Canvas.InteractionData eventData, in float3 globalPoint, out bool __result)
        {
            __result = false;

            if (__instance.Target.Target is null)
                return false;

            foreach (var item in items)
            {
                foreach (ReferenceProxy componentsInChild in item.Slot.GetComponentsInChildren<ReferenceProxy>())
                {
                    if (componentsInChild.Reference.Target is not Component component || __instance.Target.Target == component.Slot)
                        continue;

                    __instance.StartTask(async () =>
                    {
                        var contextMenu = await __instance.LocalUser.OpenContextMenu(__instance, eventData.source.Slot);

                        var copyItem = contextMenu.AddItem("Inspector.Actions.CopyComponent".AsLocaleKey(), (Uri)null!, RadiantUI_Constants.Hero.GREEN);
                        var moveItem = contextMenu.AddItem("Inspector.Actions.MoveComponent".AsLocaleKey(), (Uri)null!, RadiantUI_Constants.Hero.PURPLE);
                        var cancelItem = contextMenu.AddItem("General.Cancel".AsLocaleKey(), (Uri)null!, colorX.White);

                        copyItem.Button.LocalPressed += delegate
                        {
                            // Use Slot.DuplicateComponent instead of the vanilla CopyComponent because
                            // this is fixed in the Community Bug-Fix Collection
                            __instance.Target.Target.DuplicateComponent(component);
                            __instance.LocalUser.CloseContextMenu(__instance);
                        };

                        moveItem.Button.LocalPressed += delegate
                        {
                            __instance.Target.Target.MoveComponent(component);
                            __instance.LocalUser.CloseContextMenu(__instance);
                        };

                        cancelItem.Button.LocalPressed += delegate
                        {
                            __instance.LocalUser.CloseContextMenu(__instance);
                        };
                    });

                    __result = true;
                    return false;
                }
            }

            return false;
        }
    }
}