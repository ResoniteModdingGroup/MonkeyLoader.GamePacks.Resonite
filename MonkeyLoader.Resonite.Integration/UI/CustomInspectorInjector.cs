using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using MonkeyLoader.Patching;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace MonkeyLoader.Resonite.UI
{
    /// <summary>
    /// Handles injecting <see cref="CustomInspectorSegment"/>s provided by mods into
    /// the UI building process of <see cref="WorkerInspector"/>s.<br/>
    /// <b>Make sure to <see cref="RemoveSegment">remove</see> them during Shutdown.</b>
    /// </summary>
    [HarmonyPatchCategory(nameof(CustomInspectorInjector))]
    [HarmonyPatch(typeof(WorkerInspector), nameof(WorkerInspector.BuildUIForComponent))]
    public sealed class CustomInspectorInjector : Monkey<CustomInspectorInjector>
    {
        private static readonly SortedSet<ICustomInspectorSegment> _customInspectorSegments = new(CustomInspectorSegmentComparer);

        private static ICustomInspectorSegment _inspectorSegment;

        // one way ref
        //public static CustomInspectorSegment AddOneWayReferenceFieldTo<TWorker, TReference>(Func<TWorker, TReference> selector, string name)
        //    where TWorker : Worker
        //    where TReference : class, IWorldElement
        //{
        //    var segment = new CustomInspectorSegment(Is<TWorker>, (worker, ui, _) => BuildOneWayReferenceSegmentUI(selector((TWorker)worker), name, ui));

        //    _customInspectorSegments.Add(segment);
        //    return segment;
        //}

        /// <summary>
        /// Gets an <see cref="IComparer{T}"/> that compares <see cref="ICustomInspectorSegment"/>s
        /// based on their <see cref="ICustomInspectorSegment.Priority">priority</see>.
        /// </summary>
        public static IComparer<ICustomInspectorSegment> CustomInspectorSegmentComparer { get; } = new CustomInspectorSegmentComparerImpl();

        /// <summary>
        /// Gets all custom inspector segments which currently
        /// get queried during the UI building process, ordered by their
        /// <see cref="ICustomInspectorSegment.Priority"/>.
        /// </summary>
        public static IEnumerable<ICustomInspectorSegment> Segments => _customInspectorSegments.AsSafeEnumerable();

        public static ICustomInspectorSegment AddOneWayReferenceFieldTo<TReference>(Type baseType, Func<Worker, TReference> selector, string name)
                            where TReference : class, IWorldElement
        {
            var segment = new GenericWorkerDelegateInspectorSegment(baseType, (worker, ui, _) => BuildOneWayReferenceSegmentUI(selector(worker), name, ui));

            _customInspectorSegments.Add(segment);
            return segment;
        }

        //// two way ref
        //public static CustomInspectorSegment AddReferenceFieldTo<TWorker, TReference>(Func<Worker, ISyncRef<TReference>> selector, string name)
        //    where TWorker : Worker
        //    where TReference : class, IWorldElement
        //{
        //    return null;
        //}

        /// <summary>
        /// Adds the given <see cref="ICustomInspectorSegment"/>
        /// to the set of segments queried during the UI building process.<br/>
        /// <b>Make sure to <see cref="RemoveSegment">remove</see> it during Shutdown.</b>
        /// </summary>
        /// <param name="segment">The segment to add.</param>
        /// <returns><c>true</c> if the segment was added; <c>false</c> if it was already present.</returns>
        public static bool AddSegment(ICustomInspectorSegment segment)
            => _customInspectorSegments.Add(segment);

        public static void BuildOneWayReferenceSegmentUI<TReference>(TReference reference, string name, UIBuilder ui)
                    where TReference : class, IWorldElement
        {
            ui.PushStyle();
            ui.Style.MinHeight = 24f;
            var slot = ui.Panel().Slot;
            var referenceField = slot.AttachComponent<ReferenceField<TReference>>();
            referenceField.Reference.Target = reference;
            referenceField.Reference.DriveFrom(referenceField.Reference);

            slot = SyncMemberEditorBuilder.GenerateMemberField(referenceField.Reference, name, ui);
            slot.AttachComponent<RefEditor>().Setup(referenceField.Reference);

            ui.NestOut();
            ui.PopStyle();
        }

        /// <summary>
        /// Determines whether the set of <see cref="ICustomInspectorSegment"/>s
        /// queried during the UI building process contains the given one.
        /// </summary>
        /// <param name="segment">The segment to locate.</param>
        /// <returns><c>true</c> if the segment is present; otherwise, <c>false</c>.</returns>
        public static bool HasSegment(ICustomInspectorSegment segment)
            => _customInspectorSegments.Contains(segment);

        /// <summary>
        /// Removes the given <see cref="ICustomInspectorSegment"/>
        /// from the set of segments queried during the UI building process.
        /// </summary>
        /// <param name="segment">The segment to remove.</param>
        /// <returns><c>true</c> if the segment was removed; <c>false</c> if it could not be found.</returns>
        public static bool RemoveSegment(ICustomInspectorSegment segment)
            => _customInspectorSegments.Remove(segment);

        /// <inheritdoc/>
        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => Enumerable.Empty<IFeaturePatch>();

        protected override bool OnLoaded()
        {
            base.OnLoaded();
            _inspectorSegment = AddOneWayReferenceFieldTo(typeof(DynamicVariableBase<>), worker => (DynamicVariableSpace)Traverse.Create(worker).Field("handler").Field("_currentSpace").GetValue(), "LinkedSpace");
            _inspectorSegment = AddOneWayReferenceFieldTo(typeof(DynamicVariableBase<bool>), worker => (DynamicVariableSpace)Traverse.Create(worker).Field("handler").Field("_currentSpace").GetValue(), "BoolsSpace");
            return true;
        }

        protected override bool OnShutdown()
        {
            RemoveSegment(_inspectorSegment);
            return base.OnShutdown();
        }

        private static void BuildCustomInspectorUI(Worker worker, UIBuilder ui, Predicate<ISyncMember>? memberFilter)
        {
            memberFilter ??= Yes;

            foreach (var segment in _customInspectorSegments)
            {
                if (segment.AppliesTo(worker))
                    segment.BuildInspectorUI(worker, ui, memberFilter);
            }
        }

        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> BuildInspectorUITranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var customInspectorBuildMethod = AccessTools.Method(typeof(ICustomInspector), nameof(ICustomInspector.BuildInspectorUI));
            var workerInspectorBuildMethod = AccessTools.Method(typeof(WorkerInspector), nameof(WorkerInspector.BuildInspectorUI));

            var instructionList = new List<CodeInstruction>(instructions);
            var injectedCustomInspectorBuildMethod = AccessTools.Method(typeof(CustomInspectorInjector), nameof(BuildCustomInspectorUI));

            var customInspectorIndex = instructionList.FindLastIndex(instruction => instruction.Calls(customInspectorBuildMethod));

            Label? inspectorBuiltLabel = null;
            instructionList.FindIndex(customInspectorIndex, instruction => instruction.Branches(out inspectorBuiltLabel));

            var inspectorBuiltTargetIndex = instructionList.FindIndex(customInspectorIndex, instruction => instruction.labels.Contains(inspectorBuiltLabel!.Value));

            var workerInspectorIndex = instructionList.FindIndex(customInspectorIndex, instruction => instruction.Calls(workerInspectorBuildMethod));

            instructionList.InsertRange(inspectorBuiltTargetIndex, new[]
            {
                instructionList[workerInspectorIndex - 3].Clone(),
                instructionList[workerInspectorIndex - 2].Clone(),
                instructionList[workerInspectorIndex - 1].Clone(),
                new CodeInstruction(OpCodes.Call, injectedCustomInspectorBuildMethod),
            });

            instructionList[inspectorBuiltTargetIndex].MoveLabelsFrom(instructionList[inspectorBuiltTargetIndex + 4]);

            return instructionList;
        }

        private static bool Yes(ISyncMember _) => true;

        private sealed class CustomInspectorSegmentComparerImpl : IComparer<ICustomInspectorSegment>
        {
            public int Compare(ICustomInspectorSegment x, ICustomInspectorSegment y)
            {
                var priorityComparison = x.Priority.CompareTo(y.Priority);

                if (priorityComparison != 0)
                    return priorityComparison;

                return Comparer.DefaultInvariant.Compare(x, y);
            }
        }
    }
}