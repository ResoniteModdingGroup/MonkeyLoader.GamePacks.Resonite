// Adapted from the NeosModLoader project.

using MonkeyLoader.Meta;
using System;
using System.Collections.Generic;

namespace MonkeyLoader.Configuration
{
    /// <summary>
    /// Defines a mod configuration. This should be defined by a <see cref="ResoniteMod"/> using the <see cref="ResoniteMod.DefineConfiguration(ModConfigurationDefinitionBuilder)"/> method.
    /// </summary>
    public class ConfigDefinition : IConfigDefinition
    {
        internal bool AutoSave;

        // this is a ridiculous hack because HashSet.TryGetValue doesn't exist in .NET 4.6.2
        private Dictionary<ConfigKey, ConfigKey> configurationItemDefinitionsSelfMap;

        /// <inheritdoc/>
        // clone the collection because I don't trust giving public API users shallow copies one bit
        public ISet<ConfigKey> ConfigurationItemDefinitions
            => new HashSet<ConfigKey>(configurationItemDefinitionsSelfMap.Keys);

        /// <inheritdoc/>
        public Mod Owner { get; private set; }

        /// <inheritdoc/>
        public Version Version { get; private set; }

        internal ConfigDefinition(Mod owner, Version version, HashSet<ConfigKey> configurationItemDefinitions, bool autoSave)
        {
            Owner = owner;
            Version = version;
            AutoSave = autoSave;

            configurationItemDefinitionsSelfMap = new Dictionary<ConfigKey, ConfigKey>(configurationItemDefinitions.Count);
            foreach (ConfigKey key in configurationItemDefinitions)
            {
                key.DefiningKey = key; // early init this property for the defining key itself
                configurationItemDefinitionsSelfMap.Add(key, key);
            }
        }

        internal bool TryGetDefiningKey(ConfigKey key, out ConfigKey? definingKey)
        {
            if (key.DefiningKey != null)
            {
                // we've already cached the defining key
                definingKey = key.DefiningKey;
                return true;
            }

            // first time we've seen this key instance: we need to hit the map
            if (configurationItemDefinitionsSelfMap.TryGetValue(key, out definingKey))
            {
                // initialize the cache for this key
                key.DefiningKey = definingKey;
                return true;
            }
            else
            {
                // not a real key
                definingKey = null;
                return false;
            }
        }
    }

    /// <summary>
    /// Represents an interface for mod configurations.
    /// </summary>
    public interface IConfigDefinition
    {
        /// <summary>
        /// Gets the set of configuration keys defined in this configuration definition.
        /// </summary>
        ISet<ConfigKey> ConfigurationItemDefinitions { get; }

        /// <summary>
        /// Gets the mod that owns this configuration definition.
        /// </summary>
        Mod Owner { get; }

        /// <summary>
        /// Gets the semantic version for this configuration definition. This is used to check if the defined and saved configs are compatible.
        /// </summary>
        Version Version { get; }
    }
}