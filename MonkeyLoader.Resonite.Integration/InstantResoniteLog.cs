using Elements.Core;
using HarmonyLib;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite
{
    internal sealed class InstantResoniteLog : Monkey<InstantResoniteLog>
    {
        public override bool CanBeDisabled => true;

        public override string Name => "Instant Resonite Log";

        protected override bool OnComputeDefaultEnabledState()
            => false;

        protected override void OnDisabled()
            => UniLog.FlushEveryMessage = false;

        protected override void OnEnabled()
            => UniLog.FlushEveryMessage = true;

        protected override bool OnLoaded()
        {
            UniLog.FlushEveryMessage = Enabled;
            return true;
        }
    }
}