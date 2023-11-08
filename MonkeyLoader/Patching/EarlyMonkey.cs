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
        private readonly Lazy<PrePatchTarget[]> _prePatchTargets;
        private PrePatchTarget[]? _executedPatches;

        /// <inheritdoc/>
        public IEnumerable<PrePatchTarget> ExecutedPatches => _executedPatches?.AsSafeEnumerable() ?? Enumerable.Empty<PrePatchTarget>();

        /// <remarks>
        /// <see cref="MonkeyBase.PatchJob"/>s will be given to the processing methods in the same order.
        /// </remarks>
        /// <inheritdoc/>
        public IEnumerable<PrePatchTarget> PrePatchTargets => _prePatchTargets.Value.AsSafeEnumerable();

        /// <inheritdoc/>
        public bool TargetsAllAssemblies => ReferenceEquals(PrePatchTargets, PrePatchTarget.AllAvailable);

        /// <summary>
        /// Allows creating only a single <typeparamref name="TMonkey"/> instance.
        /// </summary>
        protected EarlyMonkey()
        {
            _prePatchTargets = new Lazy<PrePatchTarget[]>(() => GetPrePatchTargets().ToArray());
        }

        /// <summary>
        /// Executes <see cref="Prepare()"/>, <see cref="Prepare(IEnumerable{PatchJob})"/>, <see cref="Patch"/> and
        /// <see cref="Validate"/> for all possible <see cref="PrePatchTargets">PrePatchTargets</see>.
        /// </summary>
        /// <returns>Whether the patching was considered successful.</returns>
        [MemberNotNull(nameof(_executedPatches))]
        public override sealed bool Run()
        {
            ThrowIfRan();
            Ran = true;

            if (!Prepare())
            {
                // Not doing anything from prepare is success
                _executedPatches = Array.Empty<PrePatchTarget>();
                Debug(() => "Skipping pre-patching as prepare failed!");
                return true;
            }

            Trace(() => "Running EarlyMonkey's processing methods!");
            var patchJobs = PrePatchTargets.TrySelect<PrePatchTarget, PatchJob>(TryFromPrePatchTarget).ToArray();

            if (!Prepare(patchJobs))
            {
                // Not doing anything from prepare is success
                _executedPatches = Array.Empty<PrePatchTarget>();
                Debug(() => "Skipping pre-patching as prepare with targets failed!");
                return true;
            }

            foreach (var patchJob in patchJobs)
                RunPatchJob(patchJob);

            var success = Validate(patchJobs);
            if (!success)
            {
                foreach (var patchJob in patchJobs)
                    patchJob.Failed = true;
            }

            foreach (var patchJob in patchJobs)
                patchJob.Finish();

            _executedPatches = patchJobs.Where(job => !job.Failed)
                .Select(job => job.Target)
                .ToArray();

            Failed = !success;
            return success;
        }

        /// <summary>
        /// Gets the names of the assemblies and types therein which this pre-patcher targets.
        /// </summary>
        protected abstract IEnumerable<PrePatchTarget> GetPrePatchTargets();

        /// <summary>
        /// Receives the <see cref="MonkeyBase.PatchJob"/> for every <see cref="PrePatchTarget"/> to apply patches.
        /// Set <c>true</c> on <see cref="MonkeyBase.PatchJob.Changes"/> to indicate that any patching has happened.<br/>
        /// Return <c>true</c> to indicate that the patching was successful.<br/>
        /// Exceptions that make patching fail should be left to bubble up.
        /// </summary>
        /// <param name="patchJob">The patch job to apply.</param>
        /// <returns>Whether the patching was successful.</returns>
        protected abstract bool Patch(PatchJob patchJob);

        /// <summary>
        /// Lets the pre-patcher make any necessary preparations before processing starts.<br/>
        /// Return <c>true</c> to indicate that patching can go ahead.
        /// </summary>
        /// <remarks>
        /// <i>By default:</i> Doesn't do anything except return <c>true</c>.
        /// </remarks>
        /// <returns>Whether patching can go ahead.</returns>
        protected virtual bool Prepare() => true;

        /// <summary>
        /// Lets the pre-patcher make any necessary preparations and/or validate the available <see cref="MonkeyBase.PatchJob"/>s.
        /// There may be <see cref="MonkeyBase.PatchJob"/>s missing if they couldn't be created.<br/>
        /// Return <c>true</c> to indicate that patching can go ahead.
        /// </summary>
        /// <remarks>
        /// <i>By default:</i> Checks whether the number of <see cref="MonkeyBase.PatchJob"/>s matches the number of <see cref="PrePatchTargets">PrePatchTargets</see>.
        /// Accepts any number when this pre-patcher <see cref="TargetsAllAssemblies">targets all assemblies</see>.
        /// </remarks>
        /// <param name="patchJobs">All patch jobs of this pre-patcher.</param>
        /// <returns>Whether patching can go ahead.</returns>
        protected virtual bool Prepare(IEnumerable<PatchJob> patchJobs) => TargetsAllAssemblies || patchJobs.Count() == PrePatchTargets.Count();

        /// <summary>
        /// Lets the pre-patcher do any necessary cleanup and validate the success of patching.
        /// Return <c>true</c> to indicate that patching was successful enough and non-failed changes should be applied.
        /// </summary>
        /// <remarks>
        /// <i>By default:</i> Checks whether all patch jobs were successful.
        /// </remarks>
        /// <param name="patchJobs">All patch jobs of this pre-patcher.</param>
        /// <returns>Whether patching was successful enough.</returns>
        protected virtual bool Validate(IEnumerable<PatchJob> patchJobs) => !patchJobs.Any(job => job.Failed);

        private void RunPatchJob(PatchJob patchJob)
        {
            try
            {
                Trace(() => $"Applying pre-patcher to {patchJob.Target.Assembly}.");

                if (!Patch(patchJob))
                {
                    patchJob.Failed = true;
                    Warn(() => $"Pre-patcher failed on assembly [{patchJob.Target.Assembly}]!");
                }
            }
            catch (Exception ex)
            {
                Error(() => ex.Format($"Pre-patcher threw an exception on assembly [{patchJob.Target.Assembly}]!"));
                patchJob.Error = true;
            }
        }

        private bool TryFromPrePatchTarget(PrePatchTarget prePatchTarget, [NotNullWhen(true)] out PatchJob? patchJob)
        {
            if (Mod.Loader.TryGetAssemblyDefinition(prePatchTarget.Assembly, out var assemblyPool, out var assemblyDefinition))
            {
                patchJob = new PatchJob(prePatchTarget, assemblyPool, assemblyDefinition);
                return true;
            }

            patchJob = null;
            return false;
        }
    }
}