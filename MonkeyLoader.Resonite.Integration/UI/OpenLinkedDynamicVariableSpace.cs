using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.UI
{
    internal sealed class OpenLinkedDynamicVariableSpace
        : ResoniteInspectorMonkey<OpenLinkedDynamicVariableSpace, BuildInspectorHeaderEvent>
    {
        public override int Priority => HarmonyLib.Priority.First;

        public override bool CanBeDisabled => true;

        public OpenLinkedDynamicVariableSpace() : base(typeof(DynamicVariableBase<>))
        { }

        protected override bool AppliesTo(BuildInspectorHeaderEvent eventData) => Enabled;

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => [];

        protected override void Handle(BuildInspectorHeaderEvent eventData)
        {
            if (Traverse.Create(eventData.Worker).Field("handler").Field("_currentSpace").GetValue() is not DynamicVariableSpace space)
                return;

            InspectorHelper.BuildHeaderOpenParentButtons(eventData.UI, space);
        }
    }
}