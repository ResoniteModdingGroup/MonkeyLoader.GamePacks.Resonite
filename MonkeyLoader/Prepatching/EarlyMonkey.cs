using MonkeyLoader.Configuration;
using MonkeyLoader.Logging;
using MonkeyLoader.Meta;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Prepatching
{
    /// <summary>
    /// Represents the base class for pre-patchers that modify a game's assemblies in memory before they get loaded.<br/>
    /// Game assemblies and their types must not be directly referenced from these.<br/>
    /// Must have a parameterless constructor. Relevant methods must be overridden by the deriving class and decorated with attributes.
    /// </summary>
    /// <remarks>
    /// For <see cref="patchAssembly">PatchAssembly</see> this is <see cref="TargetAssemblyAttribute">TargetAssembly</see> attributes,
    /// while for <see cref="PatchType">PatchType</see> it's <see cref="TargetTypeAttribute">TargetType</see> attributes.<br/>
    /// If <see cref="prepare">Prepare</see> or <see cref="cleanup">Cleanup</see> return <c>false</c>, patching won't be applied or will be discarded.<br/>
    /// If no call to <see cref="patchAssembly">PatchAssembly</see> or  <see cref="PatchType">PatchType</see>
    /// returns <c>true</c>, this patcher will be treated as having done nothing.
    /// </remarks>
    public abstract class EarlyMonkey
    {
        private Mod mod;

        /// <summary>
        /// Gets the <see cref="Configuration.Config"/> that this pre-patcher can use to load <see cref="ConfigSection"/>s.
        /// </summary>
        public Config Config => Mod.Config;

        /// <summary>
        /// Gets the <see cref="MonkeyLogger"/> that this pre-patcher can use to log messages to game-specific channels.
        /// </summary>
        public MonkeyLogger Logger { get; private set; }

        /// <summary>
        /// Gets the mod that this pre-patcher is a part of.
        /// </summary>
        public Mod Mod
        {
            get => mod;

            [MemberNotNull(nameof(mod), nameof(Logger))]
            internal set
            {
                if (value == mod)
                    return;

                mod = value;
                Logger = new MonkeyLogger(mod.Logger, Name);
            }
        }

        /// <summary>
        /// Gets the name of this pre-patcher's <see cref="Type"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the <see cref="AssemblyName"/> for the assembly this pre-patcher is defined in.
        /// </summary>
        public AssemblyName PrepatcherAssemblyName { get; internal set; }

        /// <summary>
        /// Gets the names of the assemblies and types therein which this pre-patcher targets.<br/>
        /// Must not reference the same assembly multiple times.<br/>
        /// <see cref="PatchJob"/>s will be given to methods in the same order.
        /// </summary>
        public abstract IEnumerable<PrePatchTarget> PrePatchTargets { get; }

        protected EarlyMonkey()
        {
            Name = GetType().Name;
        }

        /// <summary>
        /// Executes <see cref="prepare"/>, <see cref="patch"/> and <see cref="cleanup"/> for all possible <see cref="PrePatchTargets">PrePatchTargets</see>.
        /// </summary>
        /// <returns>Whether the patching was considered successful.</returns>
        /// <exception cref="InvalidOperationException">When the pre-patch targets reference the same Assembly multiple times.</exception>
        internal bool Apply()
        {
            Logger.Debug(() => $"Applying pre-patcher.");

            if (PrePatchTargets.Select(target => target.Assembly).Distinct().Count() < PrePatchTargets.Count())
                throw new InvalidOperationException($"{nameof(PrePatchTargets)} referenced the same Assembly multiple times!");

            var patchJobs = PrePatchTargets.TrySelect<PrePatchTarget, PatchJob>(tryFromPrePatchTarget).ToArray();

            // Not doing anything from prepare is success
            if (!prepare(patchJobs))
            {
                Logger.Debug(() => $"Skipping pre-patching as prepare failed!");
                return true;
            }

            foreach (var patchJob in patchJobs)
            {
                try
                {
                    Logger.Trace(() => $"Applying pre-patcher to {patchJob.Target.Assembly}.");

                    if (!patch(patchJob))
                    {
                        patchJob.Failed = true;
                        Logger.Warn(() => $"Pre-patcher failed on assembly [{patchJob.Target.Assembly}]!");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(() => ex.Format($"Pre-patcher threw an exception on assembly [{patchJob.Target.Assembly}]!"));
                    patchJob.Error = true;
                }
            }

            var success = cleanup(patchJobs);
            if (!success)
            {
                foreach (var patchJob in patchJobs)
                    patchJob.Failed = true;
            }

            foreach (var patchJob in patchJobs)
                patchJob.Finish();

            return success;
        }

        /// <summary>
        /// Lets the pre-patcher do any necessary cleanup.<br/>
        /// Return <c>true</c> to indicate that patching was successful enough and non-failed changes should be applied.
        /// </summary>
        /// <param name="patchJobs">All patch jobs of this pre-patcher.</param>
        /// <returns>Whether patching was successful enough.</returns>
        protected virtual bool cleanup(IEnumerable<PatchJob> patchJobs) => true;

        /// <summary>
        /// Receives the <see cref="PatchJob"/> for every <see cref="PrePatchTarget"/> to apply patches.
        /// Set <c>true</c> on <see cref="PatchJob.Changes"/> to indicate that any patching has happened.<br/>
        /// Return <c>true</c> to indicate that the patching was successful.<br/>
        /// Exceptions that make patching fail should be left to bubble up.
        /// </summary>
        /// <param name="patchJob">The patch job to apply.</param>
        /// <returns>Whether the patching was successful.</returns>
        protected abstract bool patch(PatchJob patchJob);

        /// <summary>
        /// Lets the pre-patcher make any necessary preparations and/or validate the available <see cref="PatchJob"/>s.
        /// There may be <see cref="PatchJob"/>s missing if they couldn't be created.<br/>
        /// Return <c>true</c> to indicate that patching can go ahead.
        /// Checks whether the number of <see cref="PatchJob"/>s matches the number of <see cref="PrePatchTargets">PrePatchTargets</see>.
        /// </summary>
        /// <param name="patchJobs">All patch jobs of this pre-patcher.</param>
        /// <returns>Whether patching can go ahead.</returns>
        protected virtual bool prepare(IEnumerable<PatchJob> patchJobs) => patchJobs.Count() == PrePatchTargets.Count();

        private bool tryFromPrePatchTarget(PrePatchTarget prePatchTarget, [NotNullWhen(true)] out PatchJob? patchJob)
        {
            if (!Mod.Loader.TryGetAssemblyDefinition(prePatchTarget.Assembly, out var assemblyPool, out var assemblyDefinition))
            {
                patchJob = null;
                return false;
            }

            patchJob = new PatchJob(prePatchTarget, assemblyPool, assemblyDefinition);
            return true;
        }

        /// <summary>
        /// Contains all the data necessary for patching a <see cref="PrePatchTarget"/>.
        /// </summary>
        protected sealed class PatchJob
        {
            private readonly AssemblyPool pool;
            private readonly TypeDefinition[] types;
            private AssemblyDefinition assembly;
            private bool error;

            /// <summary>
            /// Gets or sets the <see cref="AssemblyDefinition"/> that was targeted.<br/>
            /// Automatically sets <c><see cref="Changes">Changes</see> = true</c> when replacing the definition.
            /// </summary>
            public AssemblyDefinition Assembly
            {
                get => assembly;

                [MemberNotNull(nameof(assembly))]
                set
                {
                    if (assembly != value)
                        Changes = true;

                    assembly = value;
                }
            }

            /// <summary>
            /// Gets or sets whether any changes were made.
            /// </summary>
            public bool Changes { get; set; } = false;

            /// <summary>
            /// Gets whether this patch job failed hard (i.e. threw an Exception).<br/>
            /// Setting this to true sets <see cref="Failed">Failed</see> as well.
            /// </summary>
            public bool Error
            {
                get => error;
                internal set
                {
                    error = value;
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
            /// Gets the <see cref="TypeDefinition"/>s for the types specified in the target.
            /// </summary>
            public IEnumerable<TypeDefinition> Types
            {
                get
                {
                    foreach (var type in types)
                        yield return type;
                }
            }

            internal PatchJob(PrePatchTarget target, AssemblyPool pool, AssemblyDefinition assembly)
            {
                Target = target;
                this.pool = pool;
                Assembly = assembly;
                types = target.GetTypeDefinitions(assembly).ToArray();
            }

            internal void Finish()
            {
                if (Failed || !Changes)
                    pool.RestoreDefinition(Target.Assembly);
                else
                    pool.ReturnDefinition(Target.Assembly, Assembly);
            }
        }
    }
}