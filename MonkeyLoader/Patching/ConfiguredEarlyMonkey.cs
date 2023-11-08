using MonkeyLoader.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Patching
{
    /// <summary>
    /// Represents the base class for pre-patchers that modify a game's assemblies in memory before they get loaded,
    /// while using a <c><typeparamref name="TConfigSection"/> <see cref="ConfigSection">ConfigSection</see></c>.<br/>
    /// Game assemblies and their types must not be directly referenced from these.
    /// </summary>
    /// <inheritdoc/>
    /// <typeparam name="TMonkey">The type of the actual patcher.</typeparam>
    /// <typeparam name="TConfigSection">The type of the config section to load.</typeparam>
    public abstract class ConfiguredEarlyMonkey<TMonkey, TConfigSection> : EarlyMonkey<TMonkey>
        where TMonkey : ConfiguredEarlyMonkey<TMonkey, TConfigSection>, new()
        where TConfigSection : ConfigSection, new()
    {
        /// <summary>
        /// Gets the loaded config section for this pre-patcher after it has been <see cref="EarlyMonkey{TMonkey}.Prepare()">prepared</see>.
        /// </summary>
        protected static TConfigSection ConfigSection { get; private set; } = null!;

        /// <summary>
        /// Allows creating only a single <typeparamref name="TMonkey"/> instance.
        /// </summary>
        protected ConfiguredEarlyMonkey()
        { }

        /// <remarks>
        /// <i>By default:</i> Loads this pre-patcher's <c><typeparamref name="TConfigSection"/>
        /// <see cref="ConfigSection">ConfigSection</see></c> and returns <c>true</c>.
        /// </remarks>
        /// <inheritdoc/>
        protected override bool Prepare()
        {
            ConfigSection = Config.LoadSection<TConfigSection>();

            return base.Prepare();
        }
    }
}