using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Game
{
    public abstract class GameFeature
    {
        public abstract string Description { get; }
        public abstract string Name { get; }

        protected GameFeature()
        { }
    }
}