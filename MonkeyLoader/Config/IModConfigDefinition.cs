// Adapted from the NeosModLoader project.

using System;
using System.Collections.Generic;

namespace MonkeyLoader.Config
{
    /// <summary>
    /// Represents an interface for mod configurations.
    /// </summary>
    public interface IModConfigDefinition
    {
        /// <summary>
        /// Gets the set of configuration keys defined in this configuration definition.
        /// </summary>
        ISet<ModConfigKey> ConfigurationItemDefinitions { get; }

        /// <summary>
        /// Gets the mod that owns this configuration definition.
        /// </summary>
        //ResoniteModBase Owner { get; }

        /// <summary>
        /// Gets the semantic version for this configuration definition. This is used to check if the defined and saved configs are compatible.
        /// </summary>
        Version Version { get; }
    }
}