using MonkeyLoader.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.Features
{
    /// <summary>
    /// Anything Protoflux.
    /// </summary>
    public class Protoflux : GameFeature
    {
        /// <inheritdoc/>
        public override string Description { get; } = "Anything Protoflux";

        /// <inheritdoc/>
        public override string Name { get; } = "Protoflux";
    }

    /// <summary>
    /// The Visuals of the Protoflux Nodes.
    /// </summary>
    public class ProtofluxNodeVisuals : Protoflux
    {
        /// <inheritdoc/>
        public override string Description { get; } = "The Visuals of the Protoflux Nodes.";

        /// <inheritdoc/>
        public override string Name { get; } = "Protoflux Node Visuals";
    }

    /// <summary>
    /// The Tool used to edit Protoflux.
    /// </summary>
    public class ProtofluxTool : Protoflux
    {
        /// <inheritdoc/>
        public override string Description { get; } = "The Tool used to edit Protoflux.";

        /// <inheritdoc/>
        public override string Name { get; } = "Protoflux Tool";
    }
}