using Elements.Core;
using FrooxEngine;
using FrooxEngine.CommonAvatar;
using FrooxEngine.UIX;
using HarmonyLib;
using MonkeyLoader.Resonite.UI.Inspectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.UI.ContextMenus
{
    [HarmonyPatchCategory(nameof(ContextMenuInjector))]
    [HarmonyPatch(typeof(ContextMenuExtensions), nameof(ContextMenuExtensions.OpenContextMenu))]
    internal sealed class ContextMenuInjector : ResoniteAsyncEventSourceMonkey<ContextMenuInjector, ContextMenuItemsGenerationEvent>
    {
        protected override bool OnLoaded()
        {
            ContextMenuItemsGenerationEvent.AddConcreteEvent<InspectorMemberActions>(static contextMenu => new InspectorMemberActionsMenuItemsGenerationEvent(contextMenu), true);

            return base.OnLoaded();
        }

        [HarmonyPostfix]
        private static async Task<ContextMenu?> OpenContextMenuPostfixAsync(Task<ContextMenu?> __result)
        {
            if (await __result.ConfigureAwait(false) is not ContextMenu __instance)
                return null;

            // RunSynchronously to add items after the vanilla implementation

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            __instance.RunSynchronouslyAsync(async () =>
            {
                var eventData = ContextMenuItemsGenerationEvent.CreateFor(__instance);

                // ContextMenuItemsGenerationEvent is a SubscribableBaseEvent and will trigger derived handlers
                await DispatchAsync(eventData);
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return __instance;
        }
    }
}