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
        private static MethodInfo _getEnabledMethod = AccessTools.Method(typeof(CustomInspectorInjector), nameof(GetEnabled));

        private static VerticalLayout _verticalLayout;

        private static void StoreVerticalLayout(VerticalLayout layout)
        {
            _verticalLayout = layout;
        }

        private static bool GetEnabled()
        {
            return Enabled;
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            bool storedVerticalLayout = false;
            Label? headerLabel = null;
            Label? headerTextLabel = null;
            bool headerDone = false;
            bool headerTextDone = false;
            bool bodyDone = false;
            bool afterHeaderBranch = false;
            bool afterHeaderTextBranch = false;
            List<Label?> labels = new();

            Label afterHeaderPatchLabel = generator.DefineLabel();
            Label afterHeaderOriginalLabel = generator.DefineLabel();
            bool injectedAfterHeaderOriginal = false;

            Label afterHeaderTextPatchLabel = generator.DefineLabel();
            Label afterHeaderTextOriginalLabel = generator.DefineLabel();
            bool injectedAfterHeaderTextOriginal = false;

            Label afterBodyPatchLabel = generator.DefineLabel();
            Label afterBodyOriginalLabel = generator.DefineLabel();
            bool injectedAfterBodyOriginal = false;

            var instArr = instructions.ToArray();
            for (int i = 0; i < instArr.Length; i++)
            {
                var instruction = instArr[i];
                if (headerLabel is null && instruction.opcode == OpCodes.Brtrue && instArr[i-1].opcode == OpCodes.Isinst && instArr[i - 1].operand == (object)typeof(Slot))
                {
                    headerLabel = (Label)instruction.operand;
                    labels.Add(headerLabel);
                }
                if (headerTextLabel is null && instruction.opcode == OpCodes.Brfalse_S && instArr[i-1].opcode == OpCodes.Ldloc_1 && instArr[i-2].opcode == OpCodes.Stloc_1 && 
                    instArr[i-3].Calls(AccessTools.Method(typeof(CustomAttributeExtensions), nameof(CustomAttributeExtensions.GetCustomAttribute), [typeof(MemberInfo)], [typeof(InspectorHeaderAttribute)])))
                {
                    headerTextLabel = (Label)instruction.operand;
                    labels.Add(headerTextLabel);
                }
                bool didBranch = false;
                if (instruction.Branches(out var label))
                {
                    didBranch = true;
                }
                if (!didBranch)
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
                            break;
                        }
                    }
                    if (headerLabel != null && !headerDone)
                    {
                        // check Enabled
                        yield return new CodeInstruction(OpCodes.Call, _getEnabledMethod);
                        yield return new CodeInstruction(OpCodes.Brfalse, afterHeaderPatchLabel);

                        // do header
                        yield return new CodeInstruction(OpCodes.Ldloc_0);
                        yield return new CodeInstruction(OpCodes.Ldarg, 0);
                        yield return new CodeInstruction(OpCodes.Ldarg, 1);
                        yield return new CodeInstruction(OpCodes.Ldarg, 2);
                        yield return new CodeInstruction(OpCodes.Ldarg, 3);
                        yield return new CodeInstruction(OpCodes.Ldarg, 4);
                        yield return new CodeInstruction(OpCodes.Ldarg, 5);
                        yield return new CodeInstruction(OpCodes.Call, _buildHeaderMethod);

                        // skip original
                        yield return new CodeInstruction(OpCodes.Br, afterHeaderOriginalLabel);

                        yield return new CodeInstruction(OpCodes.Nop) { labels = [afterHeaderPatchLabel] };
                        headerDone = true;
                    }
                    if (headerTextLabel != null && !headerTextDone)
                    {
                        // check Enabled
                        yield return new CodeInstruction(OpCodes.Call, _getEnabledMethod);
                        yield return new CodeInstruction(OpCodes.Brfalse, afterHeaderTextPatchLabel);

                        // do header text
                        yield return new CodeInstruction(OpCodes.Ldloc_0);
                        yield return new CodeInstruction(OpCodes.Ldarg, 1);
                        yield return new CodeInstruction(OpCodes.Call, _buildHeaderTextMethod);

                        // skip original
                        yield return new CodeInstruction(OpCodes.Br, afterHeaderTextOriginalLabel);

                        yield return new CodeInstruction(OpCodes.Nop) { labels = [afterHeaderTextPatchLabel] };
                        headerTextDone = true;
                    }
                }
                if (headerDone && afterHeaderBranch && !injectedAfterHeaderOriginal)
                {
                    yield return new CodeInstruction(OpCodes.Nop) { labels = [afterHeaderOriginalLabel] };
                    injectedAfterHeaderOriginal = true;
                }
                if (headerTextDone && afterHeaderTextBranch && !injectedAfterHeaderTextOriginal)
                {
                    yield return new CodeInstruction(OpCodes.Nop) { labels = [afterHeaderTextOriginalLabel] };
                    injectedAfterHeaderTextOriginal = true;
                }
                if (bodyDone && !injectedAfterBodyOriginal)
                {
                    yield return new CodeInstruction(OpCodes.Nop) { labels = [afterBodyOriginalLabel] };
                    injectedAfterBodyOriginal = true;
                }
                yield return instruction;
                if (!storedVerticalLayout && instruction.Calls(AccessTools.Method(typeof(UIBuilder), nameof(UIBuilder.VerticalLayout), [typeof(float), typeof(float), typeof(Alignment?), typeof(bool?), typeof(bool?)])))
                {
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return new CodeInstruction(OpCodes.Call, _storeVerticalLayoutMethod);
                    storedVerticalLayout = true;
                }
                if (!bodyDone && instruction.Calls(AccessTools.Method(typeof(WorkerInspector), nameof(WorkerInspector.BuildInspectorUI))))
                {
                    // check Enabled
                    yield return new CodeInstruction(OpCodes.Call, _getEnabledMethod);
                    yield return new CodeInstruction(OpCodes.Brfalse, afterBodyPatchLabel);

                    // do body
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldarg, 0);
                    yield return new CodeInstruction(OpCodes.Ldarg, 1);
                    yield return new CodeInstruction(OpCodes.Ldarg, 2);
                    yield return new CodeInstruction(OpCodes.Ldarg, 3);
                    yield return new CodeInstruction(OpCodes.Ldarg, 4);
                    yield return new CodeInstruction(OpCodes.Ldarg, 5);
                    yield return new CodeInstruction(OpCodes.Call, _buildBodyMethod);

                    // skip original
                    yield return new CodeInstruction(OpCodes.Br, afterBodyOriginalLabel);

                    yield return new CodeInstruction(OpCodes.Nop) { labels = [afterBodyPatchLabel] };
                    bodyDone = true;
                }
            }
        }

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

            var eventData = new BuildInspectorHeaderEvent(ui, inspector, worker, allowContainer, allowDuplicate, allowDestroy, memberFilter);

            Dispatch(eventData);

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