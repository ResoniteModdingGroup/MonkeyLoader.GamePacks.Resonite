using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Prepatching
{
    /// <summary>
    /// Base class for any pre-patchers linking to a regular mod assembly. Must have a parameterless constructor.
    /// Relevant methods must be overridden by the deriving class and decorated with attributes.<br/>
    /// Gets access to the <see cref="LinkedAssembly"/> and it's relative <see cref="LinkedAssemblyFilename">path</see>
    /// in all called methods.
    /// </summary>
    /// <remarks>
    /// For <see cref="PrepatcherMonkey.PatchAssembly">PatchAssembly</see> this is <see cref="TargetAssemblyAttribute">TargetAssembly</see> attributes,
    /// while for <see cref="PrepatcherMonkey.PatchType">PatchType</see> it's <see cref="TargetTypeAttribute">TargetType</see> attributes.<br/>
    /// If <see cref="PrepatcherMonkey.Prepare">Prepare</see> or <see cref="PrepatcherMonkey.Cleanup">Cleanup</see> return <c>false</c>,
    /// patching won't be applied or will be discarded.<br/>
    /// If no call to <see cref="PrepatcherMonkey.PatchAssembly">PatchAssembly</see>, <see cref="PatchLinkedAssembly">PatchLinkedAssembly</see> or
    /// <see cref="PrepatcherMonkey.PatchType">PatchType</see> returns <c>true</c>, this patcher will be treated as having done nothing.
    /// </remarks>
    public abstract class LinkedPrePatcherMonkey : PrepatcherMonkey
    {
        /// <summary>
        /// Gets the <see cref="AssemblyDefinition"/> of the linked mod assembly.
        /// </summary>
        public AssemblyDefinition LinkedAssembly { get; internal set; }

        /// <summary>
        /// Gets the relative path to the linked mod assembly file.
        /// </summary>
        public string LinkedAssemblyFilename { get; internal set; } = string.Empty;

        /// <summary>
        /// Gets the <see cref="MonkeyLogger"/> that this pre-patcher can use to log messages to game-specific channels.
        /// </summary>
        public MonkeyLogger Logger { get; internal set; }

        /// <summary>
        /// Can patch the linked assembly before and after the other patch methods have been called.<br/>
        /// Return <c>true</c> to indicate that any patching has happened.
        /// </summary>
        /// <param name="beforePatching">Indicates whether this call is happening before or after the other patch methods.</param>
        /// <returns>Whether any patching has happened.</returns>
        public virtual bool PatchLinkedAssembly(bool beforePatching) => false;
    }
}