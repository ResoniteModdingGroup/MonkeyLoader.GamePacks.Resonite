using MonkeyLoader.Configuration;
using MonkeyLoader.Logging;
using MonkeyLoader.Prepatching;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader
{
    public sealed class MonkeyLoader
    {
        public ConfigManager ConfigManager { get; private set; }
        public bool HasLoadedMods { get; private set; }
        public LocationConfiguration Locations { get; private set; }
        public MonkeyLogger Logger { get; private set; }
        //public IEnumerable<Monkey> Mods
        //{
        //    get
        //    {
        //        foreach (var mod in mods)
        //            yield return mod;
        //    }
        //}

        public MonkeyLoader(LocationConfiguration? locations = null)
        {
            Locations = locations ?? new();
        }

        private void prepatch(IEnumerable<EarlyMonkey> prePatchers)
        {
            // should be case insensitive
            var assemblyDefinitions = new Dictionary<string, AssemblyDefinition>();
            var neededDefinitions = new Dictionary<string, AssemblyDefinition>();

            foreach (var prePatcher in prePatchers)
            {
                neededDefinitions.Clear();
            }
        }
    }
}