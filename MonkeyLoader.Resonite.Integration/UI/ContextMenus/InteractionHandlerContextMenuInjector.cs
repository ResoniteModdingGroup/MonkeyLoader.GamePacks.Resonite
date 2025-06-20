using Elements.Core;
using FrooxEngine;
using FrooxEngine.CommonAvatar;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace MonkeyLoader.Resonite.UI.ContextMenus
{
    [HarmonyPatchCategory(nameof(InteractionHandlerContextMenuInjector))]
    [HarmonyPatch]
    internal sealed class InteractionHandlerContextMenuInjector
        : ConfiguredResoniteAsyncEventSourceMonkey<InteractionHandlerContextMenuInjector, ContextMenusConfig, ContextMenuItemsGenerationEvent>
    {
        public override bool CanBeDisabled => true;

        private static MethodInfo _saveItemsMethod = AccessTools.Method(typeof(InteractionHandlerContextMenuInjector), nameof(HandleSaveItems));
        private static MethodInfo _getEnabledMethod = AccessTools.Method(typeof(InteractionHandlerContextMenuInjector), nameof(GetEnabled));
        private static MethodInfo _notHasTooltipMethod = AccessTools.Method(typeof(InteractionHandlerContextMenuInjector), nameof(HandleNotHasTooltip));
        private static MethodInfo _getAlwaysShowLocomotionOrScalingMethod = AccessTools.Method(typeof(InteractionHandlerContextMenuInjector), nameof(GetAlwaysShowLocomotionOrScaling));
        private static MethodInfo _handleEndMethod = AccessTools.Method(typeof(InteractionHandlerContextMenuInjector), nameof(HandleEnd));
        private static FieldInfo _handlerField;
        private static FieldInfo _menuField;

        [HarmonyTargetMethod]
        static MethodBase TargetMethod()
        {
            var type = typeof(InteractionHandler);
            var nestedTypes = type.GetNestedTypes(BindingFlags.NonPublic);

            foreach (var nestedType in nestedTypes)
            {
                if (nestedType.Name.Contains("c__DisplayClass328_0"))
                {
                    var nestedTypes2 = nestedType.GetNestedTypes(BindingFlags.NonPublic);
                    var fields = nestedType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var field in fields)
                    {
                        if (field.Name.Contains("4__this"))
                        {
                            _handlerField = field;
                        }
                        if (field.Name.Contains("menu"))
                        {
                            _menuField = field;
                        }
                    }
                    foreach (var nestedType2 in nestedTypes2)
                    {
                        if (nestedType2.Name.Contains("<<OpenContextMenu>b__0>d"))
                        {
                            
                            return nestedType2.GetMethod("MoveNext", BindingFlags.NonPublic | BindingFlags.Instance);
                        }
                    }
                }
            }

            throw new Exception("Could not find async state machine");
        }

        private static bool GetEnabled()
        {
            return Enabled;
        }

        private static bool GetAlwaysShowLocomotionOrScaling()
        {
            return ConfigSection.AlwaysShowLocomotionOrScaling;
        }

        private static LocaleString AsMenuLocaleKey(string key)
            => key.AsLocaleKey(continuous: false, null);

        private static string GetPrettyInventoryPath()
        {
            if (InventoryBrowser.CurrentUserspaceInventory?.CurrentDirectory is null)
                return string.Empty;

            var path = InventoryBrowser.CurrentUserspaceInventory.CurrentDirectory.GetChainFromRoot();

            // With zero-width space to allow line-breaking after folder separators
            // With non-breaking space in folder names to avoid line-breaks in names ... which the font renderer seems to ignore :'C
            return $"{GetPrettyRootDirectoryName(path[0])}://\u200B{path.Skip(1).Join(directory => directory.Name, "/\u200B")}"
                .Replace(" ", "\u00A0");
        }

        private static string GetPrettyRootDirectoryName(RecordDirectory directory)
            => directory.Name != "Inventory" ? directory.Name
                : (directory.OwnerId == Engine.Current.Cloud.CurrentUserID ? "Personal"
                    : Engine.Current.Cloud.Groups.CurrentUserMemberships.FirstOrDefault(membership => membership.GroupId == directory.OwnerId)?.GroupName)
                        ?? "Unknown";

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var instArr = instructions.ToArray();
            bool saveItemDone = false;
            bool notHasTooltipDone = false;
            bool endDone = false;
            Label? afterSaveItemsBranchLabel = null;
            Label? afterNotHasTooltipBranchLabel = null;
            Label afterSaveItemsPatchLabel = generator.DefineLabel();
            Label afterNotHasTooltipPatchLabel = generator.DefineLabel();
            Label newBranchForNotTooltipLabel = generator.DefineLabel();
            Label afterEndLabel = generator.DefineLabel();
            for (int i = 0; i < instArr.Length; i++)
            {
                var instruction = instArr[i];

                if (afterSaveItemsBranchLabel != null && !saveItemDone)
                {
                    yield return new CodeInstruction(OpCodes.Call, _getEnabledMethod);
                    yield return new CodeInstruction(OpCodes.Brfalse, afterSaveItemsPatchLabel);

                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Ldfld, _handlerField);
                    //yield return new CodeInstruction(OpCodes.Ldloc_1);
                    //yield return new CodeInstruction(OpCodes.Ldfld, _handlerField);
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Ldfld, _menuField);
                    yield return new CodeInstruction(OpCodes.Call, _saveItemsMethod);

                    yield return new CodeInstruction(OpCodes.Br, afterSaveItemsBranchLabel.Value);

                    yield return new CodeInstruction(OpCodes.Nop) {labels = [afterSaveItemsPatchLabel]};
                    saveItemDone = true;
                }

                if (afterNotHasTooltipBranchLabel != null && !notHasTooltipDone)
                {
                    yield return new CodeInstruction(OpCodes.Call, _getAlwaysShowLocomotionOrScalingMethod) { labels = [newBranchForNotTooltipLabel] };
                    yield return new CodeInstruction(OpCodes.Brfalse, afterNotHasTooltipBranchLabel);

                    yield return new CodeInstruction(OpCodes.Call, _getEnabledMethod);
                    yield return new CodeInstruction(OpCodes.Brfalse, afterNotHasTooltipPatchLabel);

                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Ldfld, _handlerField);
                    //yield return new CodeInstruction(OpCodes.Ldloc_1);
                    //yield return new CodeInstruction(OpCodes.Ldfld, _handlerField);
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Ldfld, _menuField);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 4);
                    yield return new CodeInstruction(OpCodes.Call, _notHasTooltipMethod);

                    yield return new CodeInstruction(OpCodes.Br, afterNotHasTooltipBranchLabel);

                    yield return new CodeInstruction(OpCodes.Nop) { labels = [afterNotHasTooltipPatchLabel] };
                    notHasTooltipDone = true;
                }

                // look for CanSaveItem and replace the branch
                if (afterSaveItemsBranchLabel is null && instruction.opcode == OpCodes.Brfalse && instArr[i-1].Calls(AccessTools.Method(typeof(InteractionHandler), nameof(InteractionHandler.CanSaveItem))))
                {
                    afterSaveItemsBranchLabel = (Label)instruction.operand;
                }

                // look for: if (!hasTooltip || base.InputInterface.ScreenActive)
                if (afterNotHasTooltipBranchLabel is null && instruction.opcode == OpCodes.Brfalse && instArr[i-1].Calls(AccessTools.Method(typeof(InputInterface), "get_ScreenActive")))
                {
                    afterNotHasTooltipBranchLabel = (Label)instruction.operand;
                    instruction.operand = newBranchForNotTooltipLabel;
                }

                yield return instruction;

                // look for TaskAwaiter.GetResult with local 44
                if (!endDone && instruction.Calls(AccessTools.Method(typeof(TaskAwaiter), nameof(TaskAwaiter.GetResult))) && instArr[i - 1].opcode == OpCodes.Ldloca_S && instArr[i - 1].operand == (object)44)
                {

                    yield return new CodeInstruction(OpCodes.Call, _getEnabledMethod);
                    yield return new CodeInstruction(OpCodes.Brfalse, afterEndLabel);

                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Ldfld, _handlerField);
                    //yield return new CodeInstruction(OpCodes.Ldloc_1);
                    //yield return new CodeInstruction(OpCodes.Ldfld, _handlerField);
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Ldfld, _menuField);
                    yield return new CodeInstruction(OpCodes.Call, _handleEndMethod);

                    yield return new CodeInstruction(OpCodes.Nop) { labels = [afterEndLabel] };
                    endDone = true;
                }
            }
        }

        private static void HandleSaveItems(InteractionHandler handler, ContextMenu menu)
        {
            var labelLocaleKey = "Interaction.SaveToInventory";
            var canWrite = InventoryBrowser.CurrentUserspaceInventory?.CanWriteToCurrentDirectory ?? false;
            var canShowLocation = ConfigSection.ShowSaveLocation && InventoryBrowser.CurrentUserspaceInventory?.CurrentDirectory is not null && handler.Cloud.Session.CurrentUserID is not null;

            if (handler.Cloud.Session.CurrentUserID is null)
                labelLocaleKey = "Interaction.SaveToInventory.NotLoggedIn";
            else if (!canWrite)
                labelLocaleKey = "Interaction.SaveToInventory.NoWritePermission";

            var locationFormat = !canShowLocation ? null
                : $"{{0}}<br/><size=75%><i>{GetPrettyInventoryPath()}";

            menu.AddItem(labelLocaleKey.AsLocaleKey(locationFormat, true), OfficialAssets.Graphics.Icons.General.Save, new colorX(0.25f, 0.5f, 1f), handler.SaveGrabbed)
                .Button.Enabled = canWrite;
        }

        private static void HandleNotHasTooltip(InteractionHandler handler, ContextMenu menu, bool hasTooltip)
        {
            var wouldShowLocomotionOrScale = !hasTooltip || handler.InputInterface.ScreenActive;
            var showLocomotion = wouldShowLocomotionOrScale || ConfigSection.AlwaysShowLocomotion;
            var showScale = wouldShowLocomotionOrScale || ConfigSection.AlwaysShowScaling;

            var locomotion = handler.LocomotionController.Target;
            var module = locomotion?.ActiveModule;

            if (showLocomotion && locomotion != null && !locomotion.IsSupressed && locomotion.CanUseAnyLocomotion())
            {
                var locomotionLabel = string.Concat(str2: handler.GetLocalized(module?.LocomotionName) ?? handler.GetLocalized("Interaction.Locomotion.None"), str0: handler.GetLocalized("Interaction.Locomotion"), str1: "\n<size=75%>", str3: "</size>");
                menu.AddItem(locomotionLabel, module?.LocomotionIcon!, module?.LocomotionColor ?? colorX.Black, handler.OpenLocomotionMenu);
            }

            if (showScale && handler.CanScale)
            {
                var isAtDefaultScale = handler.Slot.ActiveUserRoot.IsAtScale(handler.Slot.ActiveUserRoot.GetDefaultScale());

                var showScalingToggle = ConfigSection.AlwaysAllowScaleToggle || isAtDefaultScale;

                var showResetScale = !isAtDefaultScale
                    && (!ConfigSection.AlwaysAllowScaleToggle || ConfigSection.ShowResetScaleWithToggle);

                if (showScalingToggle)
                    menu.AddToggleItem(locomotion!.ScalingEnabled, AsMenuLocaleKey("Interaction.ScalingEnabled"), AsMenuLocaleKey("Interaction.ScalingDisabled"), colorX.Green, colorX.Red, OfficialAssets.Graphics.Icons.Tool.SetScalable, OfficialAssets.Graphics.Icons.Tool.LockScale);

                if (showResetScale)
                    menu.AddItem(AsMenuLocaleKey("Interaction.ResetScale"), OfficialAssets.Graphics.Icons.General.ResetScale, new colorX(1f, 0.4f, 0.2f), handler.ResetUserScale);
            }

            if (!hasTooltip)
            {
                menu.AddToggleItem(handler._laserEnabled, AsMenuLocaleKey("Interaction.LaserEnabled"), AsMenuLocaleKey("Interaction.LaserDisabled"), new colorX(0.3f, 0.5f, 1f), colorX.Gray, OfficialAssets.Graphics.Icons.Tool.EnableLasers, OfficialAssets.Graphics.Icons.Tool.DisableLasers);

                if (handler.InputInterface.VR_Active)
                    menu.AddItem("Interaction.Grabbing".AsLocaleKey(), (Uri?)null, RadiantUI_Constants.Hero.ORANGE, handler.OpenGrabbingMenu);
            }
        }

        private static void HandleEnd(InteractionHandler handler, ContextMenu menu)
        {
            var eventData = ContextMenuItemsGenerationEvent.CreateFor(menu);
            Logger.Info(() => $"Dispatching CM event: {eventData.GetType().CompactDescription()}");

            handler.StartTask(async () => 
            {
                // ContextMenuItemsGenerationEvent is a SubscribableBaseEvent and will trigger derived handlers
                await DispatchAsync(eventData);
            });
        }
    }
}