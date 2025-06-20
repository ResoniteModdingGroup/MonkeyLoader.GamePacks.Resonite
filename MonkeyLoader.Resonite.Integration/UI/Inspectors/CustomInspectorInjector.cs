using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace MonkeyLoader.Resonite.UI.Inspectors
{
    [HarmonyPatchCategory(nameof(CustomInspectorInjector))]
    [HarmonyPatch(typeof(WorkerInspector), nameof(WorkerInspector.BuildUIForComponent))]
    internal sealed class CustomInspectorInjector
        : ResoniteEventSourceMonkey<CustomInspectorInjector,
            BuildInspectorHeaderEvent, ResolveInspectorHeaderTextEvent, BuildInspectorBodyEvent>
    {
        public override bool CanBeDisabled => true;

        private static MethodInfo _buildHeaderMethod = AccessTools.Method(typeof(CustomInspectorInjector), nameof(OnBuildInspectorHeader));
        private static MethodInfo _buildHeaderTextMethod = AccessTools.Method(typeof(CustomInspectorInjector), nameof(OnBuildInspectorHeaderText));
        private static MethodInfo _buildBodyMethod = AccessTools.Method(typeof(CustomInspectorInjector), nameof(OnBuildInspectorBody));
        private static MethodInfo _storeVerticalLayoutMethod = AccessTools.Method(typeof(CustomInspectorInjector), nameof(StoreVerticalLayout));

        private static VerticalLayout _verticalLayout;

        private static void StoreVerticalLayout(VerticalLayout layout)
        {
            _verticalLayout = layout;
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool storedVerticalLayout = false;
            Label? headerLabel = null;
            Label? headerTextLabel = null;
            bool headerDone = false;
            bool headerTextDone = false;
            bool bodyDone = false;
            bool afterHeaderBranch = false;
            bool afterHeaderTextBranch = false;
            int numBranches = 0;
            int branchDepth = 0;
            List<Label?> labels = new();
            foreach (var instruction in instructions)
            {
                bool didBranch = false;
                if (instruction.Branches(out var label))
                {
                    if (numBranches == 0)
                        headerLabel = label;
                    else if (numBranches == 6)
                        headerTextLabel = label;
                    numBranches++;
                    branchDepth++;
                    didBranch = true;
                    labels.Add(label);
                }
                if (!didBranch)
                {
                    if (branchDepth > 0)
                    {
                        foreach (var storedLabel in labels.ToArray())
                        {
                            if (instruction.labels.Contains(storedLabel!.Value) && instruction.operand != (object)storedLabel!.Value)
                            {
                                if (storedLabel == headerLabel)
                                    afterHeaderBranch = true;
                                else if (storedLabel == headerTextLabel)
                                    afterHeaderTextBranch = true;
                                labels.Remove(storedLabel);
                                branchDepth--;
                                break;
                            }
                        }
                    }
                    if (numBranches == 1 && !headerDone)
                    {
                        // do header
                        yield return new CodeInstruction(OpCodes.Ldloc_0);
                        yield return new CodeInstruction(OpCodes.Ldarg, 0);
                        yield return new CodeInstruction(OpCodes.Ldarg, 1);
                        yield return new CodeInstruction(OpCodes.Ldarg, 2);
                        yield return new CodeInstruction(OpCodes.Ldarg, 3);
                        yield return new CodeInstruction(OpCodes.Ldarg, 4);
                        yield return new CodeInstruction(OpCodes.Ldarg, 5);
                        yield return new CodeInstruction(OpCodes.Call, _buildHeaderMethod);
                        headerDone = true;
                    }
                    if (numBranches == 7 && !headerTextDone)
                    {
                        // do header text
                        yield return new CodeInstruction(OpCodes.Ldloc_0);
                        yield return new CodeInstruction(OpCodes.Ldarg, 1);
                        yield return new CodeInstruction(OpCodes.Call, _buildHeaderTextMethod);
                        headerTextDone = true;
                    }
                }
                if (headerDone && !afterHeaderBranch) continue;
                if (headerTextDone && !afterHeaderTextBranch) continue;
                yield return instruction;
                if (!storedVerticalLayout && instruction.Calls(AccessTools.Method(typeof(UIBuilder), nameof(UIBuilder.VerticalLayout), [typeof(float), typeof(float), typeof(Alignment?), typeof(bool?), typeof(bool?)])))
                {
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return new CodeInstruction(OpCodes.Call, _storeVerticalLayoutMethod);
                    storedVerticalLayout = true;
                }
                if (!bodyDone && instruction.Calls(AccessTools.Method(typeof(WorkerInspector), nameof(WorkerInspector.BuildInspectorUI))))
                {
                    // do body
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldarg, 0);
                    yield return new CodeInstruction(OpCodes.Ldarg, 1);
                    yield return new CodeInstruction(OpCodes.Ldarg, 2);
                    yield return new CodeInstruction(OpCodes.Ldarg, 3);
                    yield return new CodeInstruction(OpCodes.Ldarg, 4);
                    yield return new CodeInstruction(OpCodes.Ldarg, 5);
                    yield return new CodeInstruction(OpCodes.Call, _buildBodyMethod);
                    bodyDone = true;
                }
            }
        }

        //[HarmonyPrefix]
        //private static bool BuildUIForComponentPrefix(WorkerInspector __instance, Worker worker,
        //    bool allowRemove, bool allowDuplicate, bool allowContainer, Predicate<ISyncMember> memberFilter)
        //{
        //    if (!Enabled)
        //        return true;

        //    var ui = new UIBuilder(__instance.Slot);
        //    RadiantUI_Constants.SetupEditorStyle(ui);
        //    ui.Style.RequireLockInToPress = true;
        //    var vertical = ui.VerticalLayout(6f);

        //    if (worker is not Slot)
        //    {
        //        ui.Style.MinHeight = 32f;
        //        ui.HorizontalLayout(4f);
        //        ui.Style.MinHeight = 24f;
        //        ui.Style.FlexibleWidth = 1000f;

        //        OnBuildInspectorHeader(ui, __instance, worker, allowContainer, allowDuplicate, allowRemove, memberFilter);

        //        ui.NestInto(vertical.Slot);
        //    }

        //    OnBuildInspectorHeaderText(ui, worker);

        //    if (worker is ICustomInspector customInspector)
        //    {
        //        try
        //        {
        //            ui.Style.MinHeight = 24f;
        //            customInspector.BuildInspectorUI(ui);
        //        }
        //        catch (Exception ex)
        //        {
        //            ui.Text((LocaleString)"EXCEPTION BUILDING UI. See log");
        //            UniLog.Error(ex.ToString(), stackTrace: false);
        //        }
        //    }
        //    else
        //    {
        //        WorkerInspector.BuildInspectorUI(worker, ui, memberFilter);
        //    }

        //    OnBuildInspectorBody(ui, __instance, worker, allowContainer, allowDuplicate, allowRemove, memberFilter);

        //    ui.Style.MinHeight = 8f;
        //    ui.Panel();
        //    ui.NestOut();

        //    return false;
        //}

        private static void OnBuildInspectorBody(UIBuilder ui, WorkerInspector inspector, Worker worker,
            bool allowDestroy, bool allowDuplicate, bool allowContainer, Predicate<ISyncMember> memberFilter)
        {
            var root = ui.Root;

            var eventData = new BuildInspectorBodyEvent(ui, inspector, worker, allowContainer, allowDuplicate, allowDestroy, memberFilter);

            Dispatch(eventData);

            ui.NestInto(root);
        }

        private static void OnBuildInspectorHeader(UIBuilder ui, WorkerInspector inspector, Worker worker,
            bool allowDestroy, bool allowDuplicate, bool allowContainer, Predicate<ISyncMember> memberFilter)
        {
            ui.Style.MinHeight = 32f;
            ui.HorizontalLayout(4f);
            ui.Style.MinHeight = 24f;
            ui.Style.FlexibleWidth = 1000f;

            var root = ui.Root;

            var eventData = new BuildInspectorHeaderEvent(ui, inspector, worker, allowContainer, allowDuplicate, allowDestroy, memberFilter);

            Dispatch(eventData);

            ui.NestInto(root);
            ui.NestInto(_verticalLayout.Slot);
        }

        private static void OnBuildInspectorHeaderText(UIBuilder ui, Worker worker)
        {
            var eventData = new ResolveInspectorHeaderTextEvent(worker);

            Dispatch(eventData);

            if (eventData.ItemCount is 0)
                return;

            // The expander code is based on SlotInspector.OnChanges

            var root = ui.Root;

            ui.PushStyle();
            ui.Style.MinHeight = 32;
            ui.Style.ForceExpandHeight = false;
            ui.Style.ChildAlignment = Alignment.TopLeft;

            var headerLayout = ui.HorizontalLayout(4);
            var headerButton = headerLayout.Slot.AttachComponent<Button>();
            var expander = headerLayout.Slot.AttachComponent<Expander>();
            var expanderIndicator = headerLayout.Slot.AttachComponent<TextExpandIndicator>();

            ui.Style.MinWidth = 32;
            ui.Style.FlexibleWidth = -1;

            var expanderButton = ui.Button();
            expanderButton.ColorDrivers[0].MoveTo(headerButton);
            expanderIndicator.Text.Target = expanderButton.Slot.GetComponentInChildren<Text>().Content;
            expanderButton.Destroy();

            ui.Style.FlexibleWidth = 1;
            var label = ui.Text(Mod.GetLocaleString("Inspector.HeaderTextLabel"), alignment: Alignment.MiddleLeft);
            var labelColorDriver = headerButton.ColorDrivers.Add();
            labelColorDriver.ColorDrive.Target = label.Color;
            RadiantUI_Constants.SetupLabelDriverColors(labelColorDriver);

            ui.NestOut();
            ui.Style.FlexibleWidth = -1;
            ui.Style.MinHeight = -1;

            var childrenLayout = ui.HorizontalLayout(4);
            expander.SectionRoot.Target = childrenLayout.Slot;
            expanderIndicator.SectionRoot.Target = childrenLayout.Slot;
            DefaultInspectorHeaderConfig.Instance.StartHeaderTextExpanded.DriveWithLocalOverride(childrenLayout.Slot.ActiveSelf_Field);

            ui.Style.MinWidth = 32;
            ui.Empty("Spacer");

            ui.Style.FlexibleWidth = 1;
            var textLayout = ui.VerticalLayout(6);
            expanderIndicator.ChildrenRoot.Target = textLayout.Slot;

            ui.PopStyle();
            ui.PushStyle();

            foreach (var headerText in eventData.Items)
            {
                ui.Style.MinHeight = headerText.MinHeight;
                ui.Text(headerText.Text, alignment: Alignment.TopLeft);
            }

            ui.PopStyle();
            ui.NestInto(root);
        }
    }
}