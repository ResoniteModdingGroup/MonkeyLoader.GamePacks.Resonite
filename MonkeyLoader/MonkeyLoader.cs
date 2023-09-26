using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader
{
    public sealed class MonkeyLoader
    {
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
    }
}