using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using MonkeyLoader.Meta;

namespace MonkeyLoader.Patching
{
    public abstract partial class MonkeyBase : IMonkey
    {
        /// <summary>
        /// Contains all the data necessary for patching a <see cref="PrePatchTarget"/>.
        /// </summary>
        protected sealed class PatchJob
        {
            private readonly AssemblyPool _pool;
            private readonly Dictionary<string, TypeDefinition> _types;
            private AssemblyDefinition _assembly;
            private bool _error;

            /// <summary>
            /// Gets or sets the <see cref="AssemblyDefinition"/> that was targeted.<br/>
            /// Automatically sets <c><see cref="Changes">Changes</see> = true</c> when replacing the definition.
            /// </summary>
            public AssemblyDefinition Assembly
            {
                get => _assembly;

                [MemberNotNull(nameof(_assembly))]
                set
                {
                    if (_assembly != value)
                        Changes = true;

                    _assembly = value;
                }
            }

            /// <summary>
            /// Gets or sets whether any changes were made.
            /// </summary>
            public bool Changes { get; set; } = false;

            /// <summary>
            /// Gets whether this patch job failed hard (i.e. threw an Exception).<br/>
            /// When this is <c>true</c>, <see cref="Failed">Failed</see> will be as well.
            /// </summary>
            public bool Error
            {
                get => _error;
                internal set
                {
                    _error = value;
                    Failed |= value;
                }
            }

            /// <summary>
            /// Gets whether this patch job failed (softly).
            /// </summary>
            public bool Failed { get; internal set; }

            /// <summary>
            /// Gets the <see cref="PrePatchTarget"/> this <see cref="PatchJob"/> was created for.
            /// </summary>
            public PrePatchTarget Target { get; }

            /// <summary>
            /// Efficiently gets the <see cref="TypeDefinition"/> for the given full name.
            /// </summary>
            /// <param name="fullName">The full name of the type to get.</param>
            /// <returns>The type's definition.</returns>
            /// <exception cref="KeyNotFoundException">If the type's definition wasn't found.</exception>
            public TypeDefinition this[string fullName] => _types[fullName];

            /// <summary>
            /// Gets the <see cref="TypeDefinition"/>s for the types specified in the target.
            /// </summary>
            public IEnumerable<TypeDefinition> Types => _types.Values.AsSafeEnumerable();

            internal PatchJob(PrePatchTarget target, AssemblyPool pool, AssemblyDefinition assembly)
            {
                Target = target;
                _pool = pool;
                Assembly = assembly;
                _types = target.GetTypeDefinitions(assembly);
            }

            /// <summary>
            /// Efficiently tries to get the <see cref="TypeDefinition"/> for the given full name.
            /// </summary>
            /// <param name="fullName">The full name of the type to get.</param>
            /// <param name="typeDefinition">The type's definition, or <c>null</c> if it couldn't be found.</param>
            /// <returns>Whether the type's definition was found.</returns>
            public bool TryGetTypeDefinition(string fullName, [NotNullWhen(true)] out TypeDefinition? typeDefinition)
                => _types.TryGetValue(fullName, out typeDefinition);

            internal void Finish()
            {
                if (Failed || !Changes)
                    _pool.RestoreDefinition(Target.Assembly);
                else
                    _pool.ReturnDefinition(Target.Assembly, Assembly);
            }
        }
    }
}