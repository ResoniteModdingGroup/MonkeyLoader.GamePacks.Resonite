using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader
{
    public readonly struct AssemblyName
    {
        public readonly string Filename { get; }
        public readonly string Name { get; }

        public AssemblyName(string filename)
        {
            Filename = filename;
            Name = Path.GetFileName(filename);
        }

        public static implicit operator string(AssemblyName assemblyName) => assemblyName.Name;

        public static bool operator !=(AssemblyName left, AssemblyName right)
            => !string.Equals(left.Name, right.Name, StringComparison.InvariantCultureIgnoreCase);

        public static bool operator ==(AssemblyName left, AssemblyName right)
            => string.Equals(left.Name, right.Name, StringComparison.InvariantCultureIgnoreCase);

        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
            => obj is AssemblyName assemblyName && assemblyName == this;

        /// <inheritdoc/>
        public override readonly int GetHashCode() => Name.GetHashCode();

        /// <inheritdoc/>
        public override readonly string ToString() => Name;
    }
}