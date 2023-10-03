using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Game
{
    /// <summary>
    /// Base type for all feature definitions.
    /// </summary>
    public abstract class GameFeature
    {
        /// <summary>
        /// Gets the description of the feature.
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Gets the name of the feature.
        /// </summary>
        public abstract string Name { get; }
    }
}