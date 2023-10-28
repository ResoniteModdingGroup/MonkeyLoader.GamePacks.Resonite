using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyLoader.Patching
{
    /// <summary>
    /// Represents an assembly
    /// </summary>
    public sealed class PrePatchTarget
    {
        private readonly HashSet<string> _types;

        /// <summary>
        /// Gets a special value for <see cref="EarlyMonkey{TMonkey}.PrePatchTargets"/>,
        /// that indicates wanting to patch all available <see cref="AssemblyDefinition"/>s.
        /// </summary>
        public static IEnumerable<PrePatchTarget> AllAvailable { get; } = Enumerable.Empty<PrePatchTarget>();

        /// <summary>
        /// Gets the name of the targeted assembly.
        /// </summary>
        public AssemblyName Assembly { get; }

        /// <summary>
        /// Gets the full names of the targeted types.
        /// </summary>
        public IEnumerable<string> Types => _types.AsSafeEnumerable();

        /// <summary>
        /// Creates a new pre-patch target, targeting the given assembly and optionally specific types.
        /// </summary>
        /// <param name="assembly">The name of the targeted assembly.</param>
        /// <param name="types">The full names fo the targeted types.</param>
        public PrePatchTarget(AssemblyName assembly, params string[] types)
            : this(assembly, (IEnumerable<string>)types)
        { }

        /// <summary>
        /// Creates a new pre-patch target, targeting the given assembly and optionally specific types.
        /// </summary>
        /// <param name="assembly">The name of the targeted assembly.</param>
        /// <param name="types">The full names fo the targeted types.</param>
        public PrePatchTarget(AssemblyName assembly, IEnumerable<string> types)
        {
            Assembly = assembly;
            _types = types.ToHashSet();
        }

        /// <summary>
        /// Gets whether this pre-patch target includes the given full name of a type.
        /// </summary>
        /// <param name="fullName">The full name to check for being targeted.</param>
        /// <returns>Whether the given full name is targeted.</returns>
        public bool TargetsType(string fullName) => _types.Contains(fullName);

        /// <summary>
        /// Gets whether this pre-patch target includes the given type definition.
        /// </summary>
        /// <param name="typeDefinition">The type definion to check for being targeted.</param>
        /// <returns>Whether the given type definition is targeted.</returns>
        public bool TargetsType(TypeDefinition typeDefinition) => TargetsType(typeDefinition.FullName);

        internal Dictionary<string, TypeDefinition> GetTypeDefinitions(AssemblyDefinition assembly)
            => assembly.GetTypes().Where(TargetsType).ToDictionary(type => type.FullName);
    }
}