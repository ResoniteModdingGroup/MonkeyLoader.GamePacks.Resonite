using System;
using System.Collections.Generic;
using System.Reflection;

namespace ResoniteModLoader
{
    /// <summary>
    /// Contains the actual mod loader.
    /// </summary>
    public class ModLoader
    {
        /// <summary>
        /// ResoniteModLoader's version
        /// </summary>
        public static readonly string VERSION = VERSION_CONSTANT;

        internal const string VERSION_CONSTANT = "2.5.1";

        /// <summary>
        /// Allows reading metadata for all loaded mods
        /// </summary>
        /// <returns>A new list containing each loaded mod</returns>
        public static IEnumerable<ResoniteModBase> Mods()
        {
            return LoadedMods
                .Select(m => (ResoniteModBase)m.ResoniteMod)
                .ToList();
        }
    }
}