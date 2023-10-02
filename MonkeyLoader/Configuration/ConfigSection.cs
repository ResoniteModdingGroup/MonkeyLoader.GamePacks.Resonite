using HarmonyLib;
using MonkeyLoader.Prepatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Configuration
{
    /// <summary>
    /// Represents a section of a <see cref="Config"/> - e.g. for an <see cref="EarlyMonkey"/> or a <see cref="Monkey"/>.
    /// </summary>
    public abstract class ConfigSection
    {
        /// <summary>
        /// Gets a description of the config items found in this section.
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Gets the name of the section.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets all <see cref="ConfigKey"/>s which should be tracked for this <see cref="ConfigSection"/>.
        /// </summary>
        /// <remarks>
        /// Calls <see cref="getAutoConfigKeys"/> by default, but can be overridden to add others.
        /// </remarks>
        /// <returns></returns>
        protected internal virtual IEnumerable<ConfigKey> GetConfigKeys()
            => getAutoConfigKeys();

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
    }
}