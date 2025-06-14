using Elements.Core;
using FrooxEngine;
using FrooxEngine.CommonAvatar;
using FrooxEngine.UIX;
using HarmonyLib;
using System;
using System.Linq;
using static FrooxEngine.InteractionHandler;

namespace MonkeyLoader.Resonite.UI.ContextMenus
{
    [HarmonyPatchCategory(nameof(InteractionHandlerContextMenuInjector))]
    [HarmonyPatch(typeof(InteractionHandler), nameof(InteractionHandler.OpenContextMenu))]
    internal sealed class InteractionHandlerContextMenuInjector
        : ConfiguredResoniteAsyncEventSourceMonkey<InteractionHandlerContextMenuInjector, ContextMenusConfig, ContextMenuItemsGenerationEvent>
    {
        public override bool CanBeDisabled => true;

        private static LocaleString AsMenuLocaleKey(string key)
            => key.AsLocaleKey(continuous: false, null);

        [HarmonyPrefix]
        private static bool OpenContextMenuPrefix(InteractionHandler __instance, MenuOptions options, float? speedOverride)
        {
            if (!Enabled)
                return true;

            if (__instance.ContextMenu.Target is not ContextMenu menu)
                return false;

            if (__instance.IsUserspaceLaserActive)
            {
                if (__instance.World == Userspace.UserspaceWorld)
                    __instance.TryTriggerContextMenuAction();

                return false;
            }

            __instance.StartTask(async delegate
            {
                ContextMenuOptions cmOptions = new()
                {
                    speedOverride = speedOverride
                };

                if (__instance.TryTriggerContextMenuAction() || !await menu.OpenMenu(__instance, __instance.PointReference, cmOptions))
                    return;

                var hasTooltip = __instance.ActiveTool is not null;

                switch (options)
                {
                    case MenuOptions.Default:
                        Userspace.ControllerData controllerData = Userspace.GetControllerData(__instance.Side);

                        if (__instance.IsHoldingObjects || controllerData.userspaceHoldingThings || (hasTooltip && !__instance._toolLocked))
                        {
                            menu.AddItem(AsMenuLocaleKey("Interaction.Destroy"), OfficialAssets.Graphics.Icons.General.Cancel, new colorX?(new colorX(1f, 0.3f, 0.3f)), __instance.DestroyGrabbed);
                            menu.AddItem(AsMenuLocaleKey("Interaction.Duplicate"), OfficialAssets.Graphics.Icons.Item.Duplicate, new colorX?(new colorX(0.3f, 1f, 0.4f)), __instance.DuplicateGrabbed);

                            if (__instance.CanSaveItem(__instance.Grabber))
                            {
                                var labelLocaleKey = "Interaction.SaveToInventory";
                                var canWrite = InventoryBrowser.CurrentUserspaceInventory?.CanWriteToCurrentDirectory ?? false;

                                if (__instance.Cloud.Session.CurrentUserID is null)
                                    labelLocaleKey = "Interaction.SaveToInventory.NotLoggedIn";
                                else if (!canWrite)
                                    labelLocaleKey = "Interaction.SaveToInventory.NoWritePermission";

                                menu.AddItem(AsMenuLocaleKey(labelLocaleKey), OfficialAssets.Graphics.Icons.General.Save, new colorX?(new colorX(0.25f, 0.5f, 1f)), __instance.SaveGrabbed)
                                    .Button.Enabled = canWrite;
                            }

                            var menuItemSources = Pool.BorrowList<IContextMenuItemSource>();
                            foreach (var grabbedObject in __instance.Grabber.GrabbedObjects)
                                grabbedObject.Slot.GetComponentsInChildren(menuItemSources);

                            var counter = new KeyCounter<string>();
                            foreach (IContextMenuItemSource source in menuItemSources)
                            {
                                if (source.SingleItemKey is not null)
                                    counter.Increment(source.SingleItemKey);
                            }

                            foreach (var key in counter)
                            {
                                if (key.Value <= 1)
                                    continue;

                                var firstItemKept = false;
                                menuItemSources.RemoveAll(itemSource =>
                                {
                                    if (itemSource.SingleItemKey != key.Key)
                                        return false;

                                    if (itemSource.KeepFirstSingleItem && !firstItemKept)
                                    {
                                        firstItemKept = true;
                                        return false;
                                    }

                                    return true;
                                });
                            }

                            foreach (IContextMenuItemSource itemSource in menuItemSources)
                                itemSource.GenerateMenuItems(menu, menuItemSources);

                            Pool.Return(ref menuItemSources);
                        }

                        var grabbedTooltip = __instance.TryGetGrabbedTool(out var grabbable);

                        if (__instance.EquippingEnabled && (hasTooltip || grabbedTooltip is not null))
                        {
                            if (__instance._toolLocked && hasTooltip)
                                menu.AddItem(AsMenuLocaleKey("Interaction.DequipTool"), OfficialAssets.Graphics.Icons.General.HandDropping, new colorX?(new colorX(0.8f, 0.8f, 0.8f)), __instance.Dequip);
                            else
                                menu.AddItem(AsMenuLocaleKey("Interaction.EquipTool"), OfficialAssets.Graphics.Icons.General.Fist, new colorX?(new colorX(0.8f, 0.8f, 0.8f)), __instance.EquipGrabbed);
                        }

                        if (!__instance.IsHoldingObjects || __instance.HasGripEquippedTool)
                        {
                            __instance._undoItem.Target = menu.AddItem(AsMenuLocaleKey("Interaction.Undo"), OfficialAssets.Graphics.Icons.General.Undo, new colorX?(new colorX(1f, 0.1f, 0.1f)), __instance.Undo);
                            __instance._redoItem.Target = menu.AddItem(AsMenuLocaleKey("Interaction.Redo"), OfficialAssets.Graphics.Icons.General.Redo, new colorX?(new colorX(0.2f, 0.4f, 1f)), __instance.Redo);

                            __instance.UpdateUndoRedoItems();
                        }

                        var wouldShowLocomotionOrScale = !hasTooltip || __instance.InputInterface.ScreenActive;

                        if (wouldShowLocomotionOrScale || ConfigSection.AlwaysShowLocomotionOrScaling)
                        {
                            var showLocomotion = wouldShowLocomotionOrScale || ConfigSection.AlwaysShowLocomotion;
                            var showScale = wouldShowLocomotionOrScale || ConfigSection.AlwaysShowScaling;

                            var locomotion = __instance.LocomotionController.Target;
                            var module = locomotion?.ActiveModule;

                            if (showLocomotion && locomotion != null && !locomotion.IsSupressed && locomotion.CanUseAnyLocomotion())
                            {
                                var locomotionLabel = string.Concat(str2: __instance.GetLocalized(module?.LocomotionName) ?? __instance.GetLocalized("Interaction.Locomotion.None"), str0: __instance.GetLocalized("Interaction.Locomotion"), str1: "\n<size=75%>", str3: "</size>");
                                menu.AddItem(locomotionLabel, module?.LocomotionIcon!, module?.LocomotionColor ?? colorX.Black, __instance.OpenLocomotionMenu);
                            }

                            if (showScale && __instance.CanScale)
                            {
                                var isAtDefaultScale = __instance.Slot.ActiveUserRoot.IsAtScale(__instance.Slot.ActiveUserRoot.GetDefaultScale());

                                var showScalingToggle = ConfigSection.AlwaysAllowScaleToggle || isAtDefaultScale;

                                var showResetScale = !isAtDefaultScale
                                    && (!ConfigSection.AlwaysAllowScaleToggle || ConfigSection.ShowResetScaleWithToggle);

                                if (showScalingToggle)
                                    menu.AddToggleItem(locomotion!.ScalingEnabled, AsMenuLocaleKey("Interaction.ScalingEnabled"), AsMenuLocaleKey("Interaction.ScalingDisabled"), colorX.Green, colorX.Red, OfficialAssets.Graphics.Icons.Tool.SetScalable, OfficialAssets.Graphics.Icons.Tool.LockScale);

                                if (showResetScale)
                                    menu.AddItem(AsMenuLocaleKey("Interaction.ResetScale"), OfficialAssets.Graphics.Icons.General.ResetScale, new colorX(1f, 0.4f, 0.2f), __instance.ResetUserScale);
                            }
                        }

                        if (!hasTooltip)
                        {
                            menu.AddToggleItem(__instance._laserEnabled, AsMenuLocaleKey("Interaction.LaserEnabled"), AsMenuLocaleKey("Interaction.LaserDisabled"), new colorX(0.3f, 0.5f, 1f), colorX.Gray, OfficialAssets.Graphics.Icons.Tool.EnableLasers, OfficialAssets.Graphics.Icons.Tool.DisableLasers);

                            if (__instance.InputInterface.VR_Active)
                                menu.AddItem("Interaction.Grabbing".AsLocaleKey(), (Uri?)null, RadiantUI_Constants.Hero.ORANGE, __instance.OpenGrabbingMenu);
                        }

                        __instance.ActiveTool?.GenerateMenuItems(__instance, menu);

                        var rootItems = Pool.BorrowList<RootContextMenuItem>();
                        __instance.LocalUserRoot.GetRegisteredComponents(rootItems);

                        foreach (var rootItem in rootItems)
                        {
                            var rootItemSource = rootItem.Item.Target;

                            if (rootItemSource is not null && rootItemSource.Enabled && (!rootItem.OnlyForSide.Value.HasValue || rootItem.OnlyForSide.Value == __instance.Side.Value) && (!rootItem.ExcludeOnTools.Value || !hasTooltip) && (!rootItem.ExcludePrimaryHand.Value || __instance.Side.Value != __instance.InputInterface.PrimaryHand) && (!rootItem.ExcludeSecondaryHand.Value || __instance.Side.Value == __instance.InputInterface.PrimaryHand))
                                rootItemSource.SetupItem(menu);
                        }

                        Pool.Return(ref rootItems);
                        break;

                    case MenuOptions.Locomotion:
                        if (__instance.LocomotionController.Target is null)
                        {
                            __instance.CloseContextMenu();
                            return;
                        }

                        foreach (var locomotionModule in __instance.LocomotionController.Target.LocomotionModules)
                        {
                            if (locomotionModule == null)
                                continue;

                            var isCurrentModule = locomotionModule == __instance.LocomotionController.Target.ActiveModule;
                            var name = FrooxEngine.LocaleHelper.GetLocalized(str: locomotionModule.LocomotionName, element: __instance);
                            var color = locomotionModule.LocomotionColor;

                            if (isCurrentModule)
                                name = "<b>" + name + "</b>";
                            else
                                color = MathX.Lerp(in color, colorX.White, 0.3f);

                            var item = menu.AddItem(name, locomotionModule.LocomotionIcon, color);
                            item.Button.Enabled = __instance.LocomotionController.Target.CanUseModule(locomotionModule);
                            item.Button.SetupRefAction(__instance.SetLocomotion, locomotionModule);
                            item.Highlight.Value = isCurrentModule;

                            UniLog.Log($"Module: {locomotionModule.LocomotionName}, CanUse: {item.Button.Enabled}, World: {__instance.World.Name}");
                        }

                        break;

                    case MenuOptions.Grabbing:
                        menu.AddItem(("Interaction.Grab." + __instance._handGrabType.Value).AsLocaleKey(), GetIcon(__instance._handGrabType.Value), GetColor(__instance._handGrabType.Value), __instance.OpenHandGrabMenu);
                        menu.AddToggleItem(__instance._grabToggle, "Interaction.Grab.StickyGrab".AsLocaleKey(), "Interaction.Grab.HoldToHold".AsLocaleKey(), in RadiantUI_Constants.Hero.CYAN, in RadiantUI_Constants.Hero.YELLOW);
                        break;

                    case MenuOptions.LaserGrab:
                        menu.AddItem("Straighten", OfficialAssets.Common.Icons.Bang, colorX.Yellow, __instance.OnStraighten);
                        menu.AddItem("Rotate Up", OfficialAssets.Common.Icons.Up_Arrow, colorX.Green, __instance.OnRotateUp);
                        menu.AddItem("Rotate Right", OfficialAssets.Common.Icons.Right_Arrow, colorX.Red, __instance.OnRotateRight);
                        menu.AddItem("Rotate Forward", OfficialAssets.Common.Icons.Reload, colorX.Blue, __instance.OnRotateForward);
                        menu.AddItem("Unconstrained Rotation", OfficialAssets.Common.Icons.Circle, colorX.White, __instance.OnRotateUnconstrained);
                        break;

                    case MenuOptions.HandGrab:
                        foreach (var grabType in Enum.GetValues(typeof(HandGrabType)).OfType<HandGrabType>())
                        {
                            var isCurrent = grabType == __instance._handGrabType.Value;
                            menu.AddItem(("Interaction.Grab." + grabType).AsLocaleKey(isCurrent ? "<b>{0}</b>" : ""), GetIcon(grabType)!, GetColor(grabType))
                                .Button.SetupAction(__instance.SetGrabType, grabType);
                        }

                        break;
                }

                await __instance.PositionContextMenu(menu);

                var eventData = ContextMenuItemsGenerationEvent.CreateFor(menu);
                Logger.Info(() => $"Dispatching CM event: {eventData.GetType().CompactDescription()}");

                // ContextMenuItemsGenerationEvent is a SubscribableBaseEvent and will trigger derived handlers
                await DispatchAsync(eventData);
            });

            return false;
        }
    }
}