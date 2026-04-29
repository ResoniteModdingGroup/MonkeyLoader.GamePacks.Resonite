using EnumerableToolkit;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Resonite.UI.Inspectors;

namespace MonkeyLoader.Resonite.UI.ContextMenus
{
    [HarmonyPatchCategory(nameof(ContextMenuInjector))]
    [HarmonyPatch(typeof(ContextMenuExtensions), nameof(ContextMenuExtensions.OpenContextMenu))]
    internal sealed class ContextMenuInjector : ResoniteAsyncEventSourceMonkey<ContextMenuInjector, ContextMenuItemsGenerationEvent>
    {
        public override Sequence<string> SubgroupPath => SubgroupDefinitions.ContextMenu;

        protected override bool OnLoaded()
        {
            ContextMenuItemsGenerationEvent.AddConcreteEvent<InspectorMemberActions>(static contextMenu => new InspectorMemberActionsMenuItemsGenerationEvent(contextMenu), true);

            ContextMenuItemsGenerationEvent.AddConcreteEvent(typeof(FieldDriveReceiver<>), DriveReceiverMenuItemsGenerationEvent.CreateForDriveReceiver);
            ContextMenuItemsGenerationEvent.AddConcreteEvent(typeof(ReferenceDriveReceiver<>), DriveReceiverMenuItemsGenerationEvent.CreateForDriveReceiver);

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
                Logger.Info(() => $"Dispatching CM event: {eventData.GetType().CompactDescription()}");

                // ContextMenuItemsGenerationEvent is a SubscribableBaseEvent and will trigger derived handlers
                await DispatchAsync(eventData);

                if (ContextMenusConfig.Instance.LimitContextMenuItems)
                    eventData.ContextMenu.TryAddPagination(ContextMenusConfig.Instance.ContextMenuItemLimit);
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return __instance;
        }
    }
}