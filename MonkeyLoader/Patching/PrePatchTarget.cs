using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyLoader.Patching
{
    public sealed class PrePatchTarget
    {
        private readonly HashSet<string> _types;

        public AssemblyName Assembly { get; }

        public IEnumerable<string> Types
        {
            get
            {
                foreach (var type in _types)
                    yield return type;
            }
        }

        public PrePatchTarget(AssemblyName assembly, params string[] types)
        {
            Assembly = assembly;
            this._types = new(types);
        }

        public PrePatchTarget(AssemblyName assembly, IEnumerable<string> types)
        {
            Assembly = assembly;
            this._types = types.ToHashSet();
        }

        internal IEnumerable<TypeDefinition> GetTypeDefinitions(AssemblyDefinition assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (_types.Contains(type.FullName))
                    yield return type;
            }
        }
    }
}