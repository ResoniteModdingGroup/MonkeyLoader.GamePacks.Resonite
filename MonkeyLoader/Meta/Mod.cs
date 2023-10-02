using MonkeyLoader.Configuration;
using MonkeyLoader.Patching;
using MonkeyLoader.Prepatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zio;

namespace MonkeyLoader.Meta
{
    public sealed class Mod
    {
        private readonly List<EarlyMonkey> earlyMonkeys = new();
        private readonly List<Monkey> monkeys = new();
        public Config Config { get; }
        public string Description { get; }

        public IEnumerable<EarlyMonkey> EarlyMonkeys
        {
            get
            {
                foreach (var earlyMonkey in earlyMonkeys)
                    yield return earlyMonkey;
            }
        }

        public IFileSystem FileSystem { get; }

        public IEnumerable<Monkey> Monkeys
        {
            get
            {
                foreach (var monkey in monkeys)
                    yield return monkey;
            }
        }

        public string Name { get; }
        public Version Version { get; }
    }
}