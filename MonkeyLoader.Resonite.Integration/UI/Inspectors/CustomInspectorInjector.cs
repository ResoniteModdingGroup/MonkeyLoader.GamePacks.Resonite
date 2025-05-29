using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyLoader.Resonite.UI.Inspectors
{
    [HarmonyPatchCategory(nameof(CustomInspectorInjector))]
    [HarmonyPatch(typeof(WorkerInspector), nameof(WorkerInspector.BuildUIForComponent))]
    internal sealed class CustomInspectorInjector
        : ResoniteEventSourceMonkey<CustomInspectorInjector,
            BuildInspectorHeaderEvent, ResolveInspectorHeaderTextEvent, BuildInspectorBodyEvent>
    {
        public override bool CanBeDisabled => true;

        [HarmonyPrefix]
        private static bool BuildUIForComponentPrefix(WorkerInspector __instance, Worker worker,
            bool allowRemove, bool allowDuplicate, bool allowContainer, Predicate<ISyncMember> memberFilter)
        {
            if (!Enabled)
                return true;

            var ui = new UIBuilder(__instance.Slot);
            RadiantUI_Constants.SetupEditorStyle(ui);
            ui.Style.RequireLockInToPress = true;
            var vertical = ui.VerticalLayout(6f);

            if (worker is not Slot)
            {
                ui.Style.MinHeight = 32f;
                ui.HorizontalLayout(4f);
                ui.Style.MinHeight = 24f;
                ui.Style.FlexibleWidth = 1000f;

                OnBuildInspectorHeader(ui, __instance, worker, allowContainer, allowDuplicate, allowRemove, memberFilter);

                ui.NestInto(vertical.Slot);
            }

            OnBuildInspectorHeaderText(ui, worker);

            if (worker is ICustomInspector customInspector)
            {
                try
                {
                    ui.Style.MinHeight = 24f;
                    customInspector.BuildInspectorUI(ui);
                }
                catch (Exception ex)
                {
                    ui.Text((LocaleString)"EXCEPTION BUILDING UI. See log");
                    UniLog.Error(ex.ToString(), stackTrace: false);
                }
            }
            else
            {
                WorkerInspector.BuildInspectorUI(worker, ui, memberFilter);
            }

            OnBuildInspectorBody(ui, __instance, worker, allowContainer, allowDuplicate, allowRemove, memberFilter);

            ui.Style.MinHeight = 8f;
            ui.Panel();
            ui.NestOut();

            return false;
        }

        private static void OnBuildInspectorBody(UIBuilder ui, WorkerInspector inspector, Worker worker,
            bool allowContainer, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter)
        {
            var root = ui.Root;

            var eventData = new BuildInspectorBodyEvent(ui, inspector, worker, allowContainer, allowDuplicate, allowDestroy, memberFilter);

            Dispatch(eventData);

            ui.NestInto(root);
        }

        private static void OnBuildInspectorHeader(UIBuilder ui, WorkerInspector inspector, Worker worker,
            bool allowContainer, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter)
        {
            var root = ui.Root;

            var eventData = new BuildInspectorHeaderEvent(ui, inspector, worker, allowContainer, allowDuplicate, allowDestroy, memberFilter);

            Dispatch(eventData);

            ui.NestInto(root);
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