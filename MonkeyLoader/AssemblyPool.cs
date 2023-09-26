using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader
{
    public class AssemblyPool : Dictionary<AssemblyName, AssemblyDefinition>
    {
        private readonly Dictionary<AssemblyName, MemoryStream> assemblySnapshots = new();

        /// <inheritdoc/>
        public AssemblyPool()
        { }

        /// <inheritdoc/>
        public AssemblyPool(int capacity) : base(capacity)
        { }

        /// <inheritdoc/>
        public AssemblyPool(IDictionary<AssemblyName, AssemblyDefinition> dictionary) : base(dictionary)
        { }

        public void Restore(AssemblyName assemblyName)
        {
            if (!assemblySnapshots.TryGetValue(assemblyName, out var snapshotStream) || snapshotStream.Position == 0)
                throw new InvalidOperationException($"Assembly [{assemblyName}] doesn't have an associated snapshot to restore!");

            snapshotStream.Position = 0;
            this[assemblyName] = AssemblyDefinition.ReadAssembly(snapshotStream);
        }

        public void Snapshot(AssemblyName assemblyName)
        {
            var assembly = this[assemblyName];

            lock (assembly)
            {
                if (!assemblySnapshots.TryGetValue(assemblyName, out var snapshotStream))
                {
                    snapshotStream = new MemoryStream();
                    assemblySnapshots.Add(assemblyName, snapshotStream);
                }

                snapshotStream.SetLength(0);
                assembly.Write(snapshotStream);
            }
        }
    }
}