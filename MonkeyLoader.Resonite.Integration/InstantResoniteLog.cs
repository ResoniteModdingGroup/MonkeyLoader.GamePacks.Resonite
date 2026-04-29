using Elements.Core;
using EnumerableToolkit;
using MonkeyLoader.Patching;
using MonkeyLoader.Resonite.DataFeeds;

namespace MonkeyLoader.Resonite
{
    internal sealed class InstantResoniteLog : Monkey<InstantResoniteLog>, ISubgroupedDataFeedItem
    {
        public override bool CanBeDisabled => true;

        public override string Name => "Instant Resonite Log";

        public Sequence<string> SubgroupPath => SubgroupDefinitions.Development;

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