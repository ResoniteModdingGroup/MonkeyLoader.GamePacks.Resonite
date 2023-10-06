using HarmonyLib;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using MonkeyLoader.Prepatching;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Configuration
{
    /// <summary>
    /// Represents a section of a <see cref="Configuration.Config"/> - e.g. for an <see cref="EarlyMonkey"/> or a <see cref="Monkey"/>.
    /// </summary>
    /// <remarks>
    /// Use your mod's <see cref="Configuration.Config"/> instance to <see cref="Config.LoadSection{TSection}()">load</see> sections.
    /// </remarks>
    public abstract class ConfigSection
    {
        private readonly HashSet<ConfigKey> keys;

        /// <summary>
        /// Gets the <see cref="Configuration.Config"/> that this section is a part of.
        /// </summary>
        public Config Config { get; internal set; }

        /// <summary>
        /// Gets a description of the config items found in this section.
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Gets all the config keys of this section.
        /// </summary>
        public IEnumerable<ConfigKey> Keys
        {
            get
            {
                foreach (var key in keys)
                    yield return key;
            }
        }

        /// <summary>
        /// Gets the name of the section.<br/>
        /// Must be unique for a given mod.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets whether this config section is allowed to be saved.<br/>
        /// This can be <c>false</c> if something went wrong while loading it.
        /// </summary>
        public bool Saveable { get; internal set; }

        /// <summary>
        /// Gets the semantic version for this config section.<br/>
        /// This is used to check if the defined and saved configs are compatible.
        /// </summary>
        public abstract Version Version { get; }

        /// <summary>
        /// Gets the way that an incompatible saved configuration should be treated.<br/>
        /// <see cref="IncompatibleConfigHandling.Error"/> by default
        /// </summary>
        protected virtual IncompatibleConfigHandling incompatibilityHandling => IncompatibleConfigHandling.Error;

        /// <summary>
        /// Creates a new config section instance.
        /// </summary>
        protected ConfigSection()
        {
            keys = new(getConfigKeys());

            foreach (var key in keys)
            {
                key.Section = this;
                key.DefiningKey = key;
            }
        }

        /// <summary>
        /// Checks if two <see cref="ConfigSection"/>s are unequal.
        /// </summary>
        /// <param name="left">The first section.</param>
        /// <param name="right">The second section.</param>
        /// <returns><c>true</c> if they're considered unequal.</returns>
        public static bool operator !=(ConfigSection? left, ConfigSection? right)
            => !(left == right);

        /// <summary>
        /// Checks if two <see cref="ConfigSection"/>s are equal.
        /// </summary>
        /// <param name="left">The first section.</param>
        /// <param name="right">The second section.</param>
        /// <returns><c>true</c> if they're considered equal.</returns>
        public static bool operator ==(ConfigSection? left, ConfigSection? right)
            => ReferenceEquals(left, right)
            || (left is not null && right is not null && left.Name == right.Name);

        /// <summary>
        /// Checks if the given object can be considered equal to this one.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns><c>true</c> if the other object is considered equal.</returns>
        public override bool Equals(object obj) => obj is ConfigSection section && section == this;

        /// <inheritdoc/>
        public override int GetHashCode() => Name.GetHashCode();

        internal void LoadSection(JObject source)
        {
            Version serializedVersion;

            try
            {
                serializedVersion = new Version((string)source[nameof(Version)]!);
            }
            catch (Exception ex)
            {
                // I know not what exceptions the JSON library will throw, but they must be contained
                Saveable = false;
                throw new ConfigLoadException($"Error loading version for section [{Name}]!", ex);
            }

            validateCompatibility(serializedVersion);

            foreach (var key in Keys)
            {
                try
                {
                    var token = source[key.Name];

                    if (token is not null)
                    {
                        var value = token.ToObject(key.ValueType, jsonSerializer);
                        key.Set(value);
                    }
                }
                catch (Exception ex)
                {
                    // I know not what exceptions the JSON library will throw, but they must be contained
                    Saveable = false;
                    throw new ConfigLoadException($"Error loading key [{key.Name}] of type [{key.ValueType}] in section [{key.Section.Name}]!", ex);
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="ConfigKey"/>s from all fields of this <see cref="ConfigSection"/> which have a <see cref="Type"/>
        /// derived from <see cref="ConfigKey"/> and don't have a <see cref="IgnoreConfigKeyAttribute"/>.
        /// </summary>
        /// <returns>The automatically tracked <see cref="ConfigKey"/>s.</returns>
        protected IEnumerable<ConfigKey> getAutoConfigKeys()
        {
            var configKeyType = typeof(ConfigKey);

            return GetType().GetFields(AccessTools.all)
                .Where(field => configKeyType.IsAssignableFrom(field.FieldType)
                             && field.GetCustomAttribute<IgnoreConfigKeyAttribute>() is null)
                .Select(field => field.GetValue(this))
                .Cast<ConfigKey>();
        }

        /// <summary>
        /// Gets all <see cref="ConfigKey"/>s which should be tracked for this <see cref="ConfigSection"/>.
        /// </summary>
        /// <remarks>
        /// Calls <see cref="getAutoConfigKeys"/> by default, but can be overridden to add others.
        /// </remarks>
        /// <returns></returns>
        protected virtual IEnumerable<ConfigKey> getConfigKeys() => getAutoConfigKeys();

        private static bool areVersionsCompatible(Version serializedVersion, Version currentVersion)
        {
            if (serializedVersion.Major != currentVersion.Major)
            {
                // major version differences are hard incompatible
                return false;
            }

            if (serializedVersion.Minor > currentVersion.Minor)
            {
                // if serialized config has a newer minor version than us
                // in other words, someone downgraded the mod but not the config
                // then we cannot load the config
                return false;
            }

            // none of the checks failed!
            return true;
        }

        private void validateCompatibility(Version serializedVersion)
        {
            if (!areVersionsCompatible(serializedVersion, Version))
            {
                switch (incompatibilityHandling)
                {
                    case IncompatibleConfigHandling.Clobber:
                        Config.Logger.Warn(() => $"Saved section [{Name}] version [{serializedVersion}] is incompatible with mod's version [{Version}]. Clobbering old config and starting fresh.");
                        return;

                    case IncompatibleConfigHandling.ForceLoad:
                        // continue processing
                        break;

                    case IncompatibleConfigHandling.Error: // fall through to default
                    default:
                        Saveable = false;
                        throw new ConfigLoadException($"Saved section [{Name}] version [{serializedVersion}] is incompatible with mod's version [{Version}]!");
                }
            }
        }
    }
}