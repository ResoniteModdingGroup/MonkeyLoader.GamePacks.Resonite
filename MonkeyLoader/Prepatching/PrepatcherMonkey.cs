using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Prepatching
{
    /// <summary>
    /// Base class for any pre-patchers. Must have a parameterless constructor.
    /// Relevant methods must be overridden by the deriving class and decorated with attributes.
    /// </summary>
    /// <remarks>
    /// For <see cref="PatchAssembly">PatchAssembly</see> this is <see cref="TargetAssemblyAttribute">TargetAssembly</see> attributes,
    /// while for <see cref="PatchType">PatchType</see> it's <see cref="TargetTypeAttribute">TargetType</see> attributes.<br/>
    /// If <see cref="Prepare">Prepare</see> or <see cref="Cleanup">Cleanup</see> return <c>false</c>, patching won't be applied or will be discarded.<br/>
    /// If no call to <see cref="PatchAssembly">PatchAssembly</see> or  <see cref="PatchType">PatchType</see>
    /// returns <c>true</c>, this patcher will be treated as having done nothing.
    /// </remarks>
    public abstract class PrepatcherMonkey
    {
        /// <summary>
        /// Gets the relative path to this assembly file.
        /// </summary>
        public string PrepatcherAssemblyFilename { get; internal set; } = string.Empty;

        /// <summary>
        /// Lets the pre-patcher do any necessary cleanup.<br/>
        /// Return <c>true</c> to indicate that patching was successful.
        /// </summary>
        /// <returns>Whether patching was successful.</returns>
        public virtual bool Cleanup() => true;

        /// <summary>
        /// Receives an entire <see cref="AssemblyDefinition"/> to patch.<br/>
        /// Return <c>true</c> to indicate that any patching has happened.
        /// </summary>
        /// <param name="assembly">The editable or even replaceable <see cref="AssemblyDefinition"/>.</param>
        /// <param name="filename">The relative path to the original assembly file.</param>
        /// <returns>Whether any patching has happened.</returns>
        public virtual bool PatchAssembly(ref AssemblyDefinition assembly, string filename) => false;

        /// <summary>
        /// Receives a <see cref="TypeDefinition"/> to patch.<br/>
        /// Return <c>true</c> to indicate that any patching has happened.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public virtual bool PatchType(TypeDefinition type, string filename) => false;

        /// <summary>
        /// Lets the pre-patcher make any necessary preparations.<br/>
        /// Return <c>true</c> to indicate that patching can go ahead.
        /// </summary>
        /// <returns>Whether patching can go ahead.</returns>
        public virtual bool Prepare() => true;
    }
}