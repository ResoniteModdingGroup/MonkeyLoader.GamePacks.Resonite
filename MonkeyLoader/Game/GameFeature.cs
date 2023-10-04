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
    /// <remarks>
    /// These provide a non-exhaustive list of different features.<br/>
    /// Game data packs should strive to provide at least the most common ones,
    /// but mods can also add their own (sub-)features.
    /// <para/>
    /// Game data packs and mods should use inheritance to indicate sub-features.<br/>
    /// For example <c>ControllerInput : InputSystem</c> would mean that
    /// <c>ControllerInput</c> is a sub-feature of the <c>InputSystem</c>.
    /// </remarks>
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