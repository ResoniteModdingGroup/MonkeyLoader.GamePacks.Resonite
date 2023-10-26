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

namespace MonkeyLoader.Patching
{
    /// <summary>
    /// Represents the base class for pre-patchers that modify a game's assemblies in memory before they get loaded.<br/>
    /// Game assemblies and their types must not be directly referenced from these.<br/>
    /// Must have a parameterless constructor. Relevant methods must be overridden by the deriving class and decorated with attributes.
    /// </summary>
    public abstract class EarlyMonkey<TMonkey> : MonkeyBase<TMonkey>, IEarlyMonkey
        where TMonkey : EarlyMonkey<TMonkey>, new()
    {
        /// <remarks>
        /// Must not reference the same assembly multiple times.<br/>
        /// <see cref="PatchJob"/>s will be given to methods in the same order.
        /// </remarks>
        /// <inheritdoc/>
        public abstract IEnumerable<PrePatchTarget> PrePatchTargets { get; }

        /// <summary>
        /// Allows creating only a single <typeparamref name="TMonkey"/> instance.
        /// </summary>
        protected EarlyMonkey()
        { }

        /// <summary>
        /// Executes <see cref="Prepare"/>, <see cref="Patch"/> and <see cref="Validate"/> for all possible <see cref="PrePatchTargets">PrePatchTargets</see>.
        /// </summary>
        /// <returns>Whether the patching was considered successful.</returns>
        /// <exception cref="InvalidOperationException">When the pre-patch targets reference the same Assembly multiple times.</exception>
        public override bool Run()
        {
            ThrowIfRan();
            Ran = true;

            Logger.Debug(() => $"Running pre-patcher.");

            if (PrePatchTargets.Select(target => target.Assembly).Distinct().Count() < PrePatchTargets.Count())
            {
                Failed = true;
                Logger.Error(() => $"{nameof(PrePatchTargets)} referenced the same Assembly multiple times! Aborting patching.");

                return false;
            }

            var patchJobs = PrePatchTargets.TrySelect<PrePatchTarget, PatchJob>(TryFromPrePatchTarget).ToArray();

            // Not doing anything from prepare is success
            if (!Prepare(patchJobs))
            {
                Logger.Debug(() => $"Skipping pre-patching as prepare failed!");
                return true;
            }

            foreach (var patchJob in patchJobs)
            {
                try
                {
                    Logger.Trace(() => $"Applying pre-patcher to {patchJob.Target.Assembly}.");

                    if (!Patch(patchJob))
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

            var success = Validate(patchJobs);
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
        /// Receives the <see cref="PatchJob"/> for every <see cref="PrePatchTarget"/> to apply patches.
        /// Set <c>true</c> on <see cref="PatchJob.Changes"/> to indicate that any patching has happened.<br/>
        /// Return <c>true</c> to indicate that the patching was successful.<br/>
        /// Exceptions that make patching fail should be left to bubble up.
        /// </summary>
        /// <param name="patchJob">The patch job to apply.</param>
        /// <returns>Whether the patching was successful.</returns>
        protected abstract bool Patch(PatchJob patchJob);

        /// <summary>
        /// Lets the pre-patcher make any necessary preparations and/or validate the available <see cref="PatchJob"/>s.
        /// There may be <see cref="PatchJob"/>s missing if they couldn't be created.<br/>
        /// Return <c>true</c> to indicate that patching can go ahead.
        /// Checks whether the number of <see cref="PatchJob"/>s matches the number of <see cref="PrePatchTargets">PrePatchTargets</see> by default.
        /// </summary>
        /// <param name="patchJobs">All patch jobs of this pre-patcher.</param>
        /// <returns>Whether patching can go ahead.</returns>
        protected virtual bool Prepare(IEnumerable<PatchJob> patchJobs) => patchJobs.Count() == PrePatchTargets.Count();

        /// <summary>
        /// Lets the pre-patcher do any necessary cleanup and validate the success of patching.
        /// Return <c>true</c> to indicate that patching was successful enough and non-failed changes should be applied.<br/>
        /// Checks whether all patch jobs were successful by default.
        /// </summary>
        /// <param name="patchJobs">All patch jobs of this pre-patcher.</param>
        /// <returns>Whether patching was successful enough.</returns>
        protected virtual bool Validate(IEnumerable<PatchJob> patchJobs) => !patchJobs.Any(job => job.Failed);

        private bool TryFromPrePatchTarget(PrePatchTarget prePatchTarget, [NotNullWhen(true)] out PatchJob? patchJob)
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
            private readonly AssemblyPool _pool;
            private readonly TypeDefinition[] _types;
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
            /// Setting this to true sets <see cref="Failed">Failed</see> as well.
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
            /// Gets the <see cref="TypeDefinition"/>s for the types specified in the target.
            /// </summary>
            public IEnumerable<TypeDefinition> Types
            {
                get
                {
                    foreach (var type in _types)
                        yield return type;
                }
            }

            internal PatchJob(PrePatchTarget target, AssemblyPool pool, AssemblyDefinition assembly)
            {
                Target = target;
                _pool = pool;
                Assembly = assembly;
                _types = target.GetTypeDefinitions(assembly).ToArray();
            }

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