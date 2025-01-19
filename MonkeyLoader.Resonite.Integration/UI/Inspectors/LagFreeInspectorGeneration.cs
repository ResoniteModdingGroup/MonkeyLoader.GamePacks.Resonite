using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.UI.Inspectors
{
    [HarmonyPatchCategory(nameof(LagFreeInspectorGeneration))]
    [HarmonyPatch(typeof(WorkerInspector), nameof(WorkerInspector.SetupContainer))]
    internal sealed class LagFreeInspectorGeneration : ResoniteMonkey<LagFreeInspectorGeneration>
    {
        public override bool CanBeDisabled => true;

        private static bool Filter(ISyncMember syncMember) => true;

        private static bool Filter(Worker worker) => true;

        private static bool Prefix(WorkerInspector __instance,
            Worker container, Predicate<ISyncMember>? memberFilter, Predicate<Worker>? workerFilter, bool includeContainer)
        {
            if (!Enabled)
                return true;

            __instance.StartTask(async () =>
            {
                __instance.Slot.ActiveSelf = false;

                await SetupContainerAsync(__instance,
                    container, memberFilter ?? Filter, workerFilter ?? Filter, includeContainer);

                __instance.Slot.ActiveSelf = true;
            });

            return false;
        }

        private static async Task SetupContainerAsync(WorkerInspector __instance,
            Worker container, Predicate<ISyncMember> memberFilter, Predicate<Worker> workerFilter, bool includeContainer)
        {
            __instance._targetContainer.Target = container;
            __instance._workerFilter.Target = workerFilter;

            var verticalLayout = __instance.Slot.AttachComponent<VerticalLayout>();
            verticalLayout.Spacing.Value = 4f;
            verticalLayout.ForceExpandHeight.Value = false;
            verticalLayout.ChildAlignment = Alignment.TopLeft;

            if (includeContainer)
                __instance.BuildUIForComponent(container, allowRemove: false, memberFilter: memberFilter);

            if (container is Slot slot)
            {
                foreach (var component in slot.Components)
                {
                    if (workerFilter(component) && component is not GizmoLink)
                    {
                        __instance.BuildUIForComponent(component, memberFilter: memberFilter);
                        await default(NextUpdate);
                    }
                }
            }

            if (container is not User user)
                return;

            foreach (var userComponent in user.Components)
            {
                if (workerFilter(userComponent))
                {
                    __instance.BuildUIForComponent(userComponent, memberFilter: memberFilter);
                    await default(NextUpdate);
                }
            }

            foreach (var stream in user.Streams)
            {
                if (workerFilter == null || workerFilter(stream))
                {
                    __instance.BuildUIForComponent(stream, memberFilter: memberFilter);
                    await default(NextUpdate);
                }
            }
        }
    }
}