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

        public AssemblyName(string name, bool isPath = false)
        {
            Filename = name;
            Name = isPath ? Path.GetFileNameWithoutExtension(name) : name;
        }

        public static implicit operator string(in AssemblyName assemblyName) => assemblyName.Name;

        public static bool operator !=(in AssemblyName left, in AssemblyName right)
            => !string.Equals(left.Name, right.Name, StringComparison.InvariantCultureIgnoreCase);

        public static bool operator ==(in AssemblyName left, in AssemblyName right)
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