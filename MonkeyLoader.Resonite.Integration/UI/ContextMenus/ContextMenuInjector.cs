using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Logging;
using MonkeyLoader.Resonite.UI.Inspectors;

namespace MonkeyLoader.Resonite.UI.ContextMenus
{
    [HarmonyPatchCategory(nameof(ContextMenuInjector))]
    [HarmonyPatch(typeof(ContextMenuExtensions), nameof(ContextMenuExtensions.OpenContextMenu))]
    internal sealed class ContextMenuInjector : ResoniteAsyncEventSourceMonkey<ContextMenuInjector, ContextMenuItemsGenerationEvent>
    {
        protected override bool OnLoaded()
        {
            ContextMenuPaginationExtensions.GetDefaultMaxItems = static () => ContextMenusConfig.Instance.ContextMenuItemLimit;
            ContextMenuPaginationExtensions.GetLimitContextMenuItems = static () => ContextMenusConfig.Instance.LimitContextMenuItems;

            ContextMenuItemsGenerationEvent.AddConcreteEvent<InspectorMemberActions>(static contextMenu => new InspectorMemberActionsMenuItemsGenerationEvent(contextMenu), true);

            ContextMenuItemsGenerationEvent.AddConcreteEvent(typeof(FieldDriveReceiver<>), DriveReceiverMenuItemsGenerationEvent.CreateForDriveReceiver);
            ContextMenuItemsGenerationEvent.AddConcreteEvent(typeof(ReferenceDriveReceiver<>), DriveReceiverMenuItemsGenerationEvent.CreateForDriveReceiver);

            return base.OnLoaded();
        }

        [HarmonyPostfix]
        private static async Task<ContextMenu?> OpenContextMenuPostfixAsync(Task<ContextMenu?> __result)
        {
            var lastDroppedGrabbables = InteractionHandlerContextMenuInjector.GetLastDroppedGrabbables(TimeSpan.FromMilliseconds(250));

            if (await __result.ConfigureAwait(false) is not ContextMenu __instance)
                return null;

            // RunSynchronously to add items after the vanilla implementation

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            __instance.RunSynchronouslyAsync(async () =>
            {
                var eventData = ContextMenuItemsGenerationEvent.CreateFor(__instance, lastDroppedGrabbables);
                Logger.Info(() => $"Dispatching CM event: {eventData.GetType().CompactDescription()}");

                // ContextMenuItemsGenerationEvent is a SubscribableBaseEvent and will trigger derived handlers
                await DispatchAsync(eventData);

                if (ContextMenusConfig.Instance.LimitContextMenuItems)
                    eventData.ContextMenu.TryAddPagination(ContextMenusConfig.Instance.ContextMenuItemLimit);

                if (!Logger.ShouldLog(LoggingLevel.Debug))
                    return;

                Logger.Debug(() => "Dropped grabbables:");
                Logger.Debug(eventData.LastDroppedGrabbables.Grabbables.Select(item => item.ParentHierarchyToString()).ToArray());

                Logger.Debug(() => "Dropped values:");
                Logger.Debug(eventData.LastDroppedGrabbables.BoxedValues);

                Logger.Debug(() => "Dropped references:");
                Logger.Debug(eventData.LastDroppedGrabbables.UntypedReferences.Select(FieldExtensions.GetReferenceLabel));

                Logger.Debug(() => "Dropped delegates:");
                Logger.Debug(eventData.LastDroppedGrabbables.UntypedDelegates.Select(del => $"{del.Method.CompactDescription()} on {(del.Target as IWorldElement).GetReferenceLabel()}"));
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return __instance;
        }
    }
}