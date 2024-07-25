using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using MonkeyLoader.Events;
using MonkeyLoader.Patching;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MonkeyLoader.Resonite.UI
{
    [HarmonyPatchCategory(nameof(CustomInspectorInjector))]
    [HarmonyPatch(typeof(WorkerInspector), nameof(WorkerInspector.BuildUIForComponent))]
    internal sealed class CustomInspectorInjector : ResoniteMonkey<CustomInspectorInjector>,
        IEventSource<BuildInspectorHeaderEvent>, IEventSource<BuildInspectorBodyEvent>
    {
        private static EventDispatching<BuildInspectorBodyEvent>? _buildInspectorBody;
        private static EventDispatching<BuildInspectorHeaderEvent>? _buildInspectorHeader;

        private static readonly MethodInfo _openContainerMethod = AccessTools.Method(typeof(WorkerInspector), "OnOpenContainerPressed");

        /// <inheritdoc/>
        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => Enumerable.Empty<IFeaturePatch>();

        protected override bool OnLoaded()
        {
            Mod.RegisterEventSource<BuildInspectorHeaderEvent>(this);
            Mod.RegisterEventSource<BuildInspectorBodyEvent>(this);

            return base.OnLoaded();
        }

        [HarmonyPrefix]
        private static bool BuildUIForComponentPrefix(WorkerInspector __instance, Worker worker, bool allowRemove, bool allowDuplicate, bool allowContainer, Predicate<ISyncMember> memberFilter)
        {
            var ui = new UIBuilder(__instance.Slot);
            RadiantUI_Constants.SetupEditorStyle(ui);
            ui.VerticalLayout(6f);

            if (worker is not Slot)
            {
                ui.Style.MinHeight = 32f;
                ui.HorizontalLayout(4f);
                ui.Style.MinHeight = 24f;
                ui.Style.FlexibleWidth = 1000f;

                OnBuildInspectorHeader(InspectorHeaderPosition.Start, ui, worker, allowDuplicate, allowRemove, memberFilter);

                LocaleString text = $"<b>{worker.GetType().GetNiceName()}</b>";
                colorX? tint = RadiantUI_Constants.BUTTON_COLOR;

                var button = ui.ButtonRef(in text, in tint, __instance.OnWorkerTypePressed, worker);
                button.Label.Color.Value = RadiantUI_Constants.LABEL_COLOR;

                OnBuildInspectorHeader(InspectorHeaderPosition.AfterName, ui, worker, allowDuplicate, allowRemove, memberFilter);

                if (allowDuplicate || allowRemove || allowContainer)
                {
                    ui.Style.FlexibleWidth = 0f;
                    ui.Style.MinWidth = 40f;

                    if (allowContainer && worker.FindNearestParent<Slot>() != null)
                    {
                        var rootUp = OfficialAssets.Graphics.Icons.Inspector.RootUp;
                        tint = RadiantUI_Constants.Sub.PURPLE;
                        ButtonRefRelay<Worker> openContainerRelay = ui.Button(rootUp, in tint).Slot.AttachComponent<ButtonRefRelay<Worker>>();
                        openContainerRelay.Argument.Target = worker;
                        openContainerRelay.ButtonPressed.Target = _openContainerMethod.CreateDelegate<ButtonEventHandler<Worker>>(__instance);
                    }

                    if (allowDuplicate)
                    {
                        var duplicateIcon = OfficialAssets.Graphics.Icons.Inspector.Duplicate;
                        tint = RadiantUI_Constants.Sub.GREEN;

                        ButtonRefRelay<Worker> duplicateRelay = ui.Button(duplicateIcon, in tint).Slot.AttachComponent<ButtonRefRelay<Worker>>();
                        duplicateRelay.Argument.Target = worker;
                        duplicateRelay.ButtonPressed.Target = __instance.OnDuplicateComponentPressed;
                    }

                    if (allowRemove)
                    {
                        var destroyIcon = OfficialAssets.Graphics.Icons.Inspector.Destroy;
                        tint = RadiantUI_Constants.Sub.RED;

                        ButtonRefRelay<Worker> destroyRelay = ui.Button(destroyIcon, in tint).Slot.AttachComponent<ButtonRefRelay<Worker>>();
                        destroyRelay.Argument.Target = worker;
                        destroyRelay.ButtonPressed.Target = __instance.OnRemoveComponentPressed;
                    }
                }

                button.Slot.AttachComponent<ReferenceProxySource>().Reference.Target = worker;
                OnBuildInspectorHeader(InspectorHeaderPosition.End, ui, worker, allowDuplicate, allowRemove, memberFilter);

                ui.NestOut();
            }

            if (worker is ICustomInspector customInspector)
            {
                ui.Style.MinHeight = 24f;
                customInspector.BuildInspectorUI(ui);
            }
            else
            {
                WorkerInspector.BuildInspectorUI(worker, ui, memberFilter);
            }

            OnBuildInspectorBody(ui, worker, allowDuplicate, allowRemove, memberFilter);

            ui.Style.MinHeight = 8f;
            ui.Panel();
            ui.NestOut();

            return false;
        }

        private static void OnBuildInspectorBody(UIBuilder ui, Worker worker,
            bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter)
        {
            var eventData = new BuildInspectorBodyEvent(ui, worker, allowDuplicate, allowDestroy, memberFilter);

            _buildInspectorBody?.Invoke(eventData);
        }

        private static void OnBuildInspectorHeader(InspectorHeaderPosition headerPosition, UIBuilder ui,
            Worker worker, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter)
        {
            var eventData = new BuildInspectorHeaderEvent(headerPosition, ui, worker, allowDuplicate, allowDestroy, memberFilter);

            _buildInspectorHeader?.Invoke(eventData);
        }

        event EventDispatching<BuildInspectorHeaderEvent>? IEventSource<BuildInspectorHeaderEvent>.Dispatching
        {
            add => _buildInspectorHeader += value;
            remove => _buildInspectorHeader -= value;
        }

        event EventDispatching<BuildInspectorBodyEvent>? IEventSource<BuildInspectorBodyEvent>.Dispatching
        {
            add => _buildInspectorBody += value;
            remove => _buildInspectorBody -= value;
        }
    }
}