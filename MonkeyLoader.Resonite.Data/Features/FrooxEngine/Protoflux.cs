using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.Features.FrooxEngine
{
    /// <summary>
    /// Everything Protoflux in the FrooxEngine assembly.
    /// </summary>
    public class FrooxEngineProtoflux : FrooxEngine
    {
        /// <inheritdoc/>
        public override string Description { get; } = "Everything Protoflux in the FrooxEngine assembly.";

        /// <inheritdoc/>
        public override string Name { get; } = "Protoflux";
    }

    /// <summary>
    /// The Visuals of the Protoflux Nodes.
    /// </summary>
    public class ProtofluxNodeVisuals : FrooxEngineProtoflux
    {
        /// <inheritdoc/>
        public override string Description { get; } = "The Visuals of the Protoflux Nodes.";

        /// <inheritdoc/>
        public override string Name { get; } = "Protoflux Node Visuals";
    }

    /// <summary>
    /// The Tool used to edit Protoflux.
    /// </summary>
    public class ProtofluxTool : FrooxEngineProtoflux
    {
        /// <inheritdoc/>
        public override string Description { get; } = "The Tool used to edit Protoflux.";

        /// <inheritdoc/>
        public override string Name { get; } = "Protoflux Tool";
    }
}