using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.Features.FrooxEngine
{
    /// <summary>
    /// The whole FrooxEngine assembly.
    /// </summary>
    public class FrooxEngine : Feature
    {
        /// <inheritdoc/>
        public override AssemblyName Assembly { get; } = new AssemblyName("FrooxEngine");

        /// <inheritdoc/>
        public override string Description { get; } = "The whole FrooxEngine assembly.";

        /// <inheritdoc/>
        public override string Name { get; } = "Froox Engine";
    }
}