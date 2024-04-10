using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using MonkeyLoader.Patching;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyLoader.Resonite.UI
{
    /// <summary>
    /// Handles injecting <see cref="ICustomInspectorSegment"/>s provided by mods into
    /// the UI building process of <see cref="WorkerInspector"/>s.
    /// </summary>
    /// <remarks>
    /// <b>Make sure to <see cref="RemoveSegment">remove</see> them during Shutdown.</b>
    /// </remarks>
    [HarmonyPatchCategory(nameof(CustomInspectorInjector))]
    [HarmonyPatch(typeof(WorkerInspector), nameof(WorkerInspector.BuildUIForComponent))]
    public sealed class CustomInspectorInjector : ResoniteMonkey<CustomInspectorInjector>
    {
        private static readonly SortedCollection<ICustomInspectorSegment> _customInspectorSegments = new(InspectorHelper.CustomInspectorSegmentComparer);

        private static ICustomInspectorHeader _inspectorSegment;

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
        /// Gets all custom inspector segments, which add segments to the body
        /// and currently get queried during the UI building process,
        /// ordered by their <see cref="ICustomInspectorSegment.Priority"/>.
        /// </summary>
        public static IEnumerable<ICustomInspectorBody> CustomBodies => _customInspectorSegments.SelectCastable<ICustomInspectorSegment, ICustomInspectorBody>();

        /// <summary>
        /// Gets all custom inspector segments, which add segments to the header
        /// and currently get queried during the UI building process,
        /// ordered by their <see cref="ICustomInspectorSegment.Priority"/>.
        /// </summary>
        public static IEnumerable<ICustomInspectorHeader> CustomHeaders => _customInspectorSegments.SelectCastable<ICustomInspectorSegment, ICustomInspectorHeader>();

        /// <summary>
        /// Gets all custom inspector segments, which currently
        /// get queried during the UI building process, ordered by their
        /// <see cref="ICustomInspectorSegment.Priority"/>.
        /// </summary>
        public static IEnumerable<ICustomInspectorSegment> CustomSegments => _customInspectorSegments.AsSafeEnumerable();

        public static ICustomInspectorBody AddOneWayReferenceFieldTo<TReference>(Type baseType, Func<Worker, TReference> selector, string name)
                            where TReference : class, IWorldElement
        {
            var segment = new LambdaCustomInspectorBody(baseType, (ui, worker, _, _, _) => InspectorHelper.BuildOneWayReferenceSegmentUI(ui, name, selector(worker)));

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
        /// Adds the given <see cref="ICustomInspectorHeader"/>
        /// to the set of segments queried during the UI building process.<br/>
        /// <b>Make sure to <see cref="RemoveSegment">remove</see> it during Shutdown.</b>
        /// </summary>
        /// <param name="customHeader">The custom header to add.</param>
        public static void AddSegment(ICustomInspectorHeader customHeader)
            => _customInspectorSegments.Add(customHeader ?? throw new ArgumentNullException(nameof(customHeader)));

        /// <summary>
        /// Adds the given <see cref="ICustomInspectorBody"/>
        /// to the set of segments queried during the UI building process.<br/>
        /// <b>Make sure to <see cref="RemoveSegment">remove</see> it during Shutdown.</b>
        /// </summary>
        /// <param name="customBody">The custom body to add.</param>
        public static void AddSegment(ICustomInspectorBody customBody)
            => _customInspectorSegments.Add(customBody ?? throw new ArgumentNullException(nameof(customBody)));

        /// <summary>
        /// Adds the given <see cref="ICustomInspector"/>
        /// to the set of segments queried during the UI building process.<br/>
        /// <b>Make sure to <see cref="RemoveSegment">remove</see> it during Shutdown.</b>
        /// </summary>
        /// <param name="customInspector">The custom inspector to add.</param>
        public static void AddSegment(ICustomInspector customInspector)
            => _customInspectorSegments.Add(customInspector ?? throw new ArgumentNullException(nameof(customInspector)));

        /// <summary>
        /// Determines whether the set of <see cref="ICustomInspectorBody"/>s
        /// queried during the UI building process contains the given one.
        /// </summary>
        /// <param name="segment">The segment to locate.</param>
        /// <returns><c>true</c> if the segment is present; otherwise, <c>false</c>.</returns>
        public static bool HasSegment(ICustomInspectorSegment? segment)
            => segment is not null && _customInspectorSegments.Contains(segment);

        /// <summary>
        /// Removes the given <see cref="ICustomInspectorBody"/>
        /// from the set of segments queried during the UI building process.
        /// </summary>
        /// <param name="segment">The segment to remove.</param>
        /// <returns><c>true</c> if the segment was removed; <c>false</c> if it could not be found.</returns>
        public static bool RemoveSegment(ICustomInspectorSegment? segment)
            => segment is not null && _customInspectorSegments.Remove(segment);

        /// <inheritdoc/>
        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => Enumerable.Empty<IFeaturePatch>();

        protected override bool OnLoaded()
        {
            base.OnLoaded();

            _inspectorSegment = new LambdaCustomInspectorHeader(typeof(DynamicVariableBase<>), (headerPosition, ui, worker, _, _, _) =>
            {
                if (headerPosition != InspectorHeaderPosition.Start
                || Traverse.Create(worker).Field("handler").Field("_currentSpace").GetValue() is not DynamicVariableSpace space)
                    return;

                ui.PushStyle();
                ui.Style.FlexibleWidth = -1;
                ui.Style.MinWidth = 40;

                var button = ui.Button("⤴");

                var refField = button.Slot.AttachComponent<ReferenceField<DynamicVariableSpace>>();
                refField.Reference.Target = space;

                var refEditor = button.Slot.AttachComponent<RefEditor>();
                refEditor._targetRef.Target = refField.Reference;

                button.Pressed.Target = refEditor.OpenInspectorButton;
                ui.Button("↑").Pressed.Target = refEditor.OpenWorkerInspectorButton;

                ui.PopStyle();
            });

            AddSegment(_inspectorSegment);

            return true;
        }

        /// <inheritdoc/>
        protected override bool OnShutdown(bool applicationExiting)
        {
            if (!applicationExiting)
                RemoveSegment(_inspectorSegment);

            return base.OnShutdown(applicationExiting);
        }

        private static void BuildCustomInspectorBodyUI(UIBuilder ui, Worker worker, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter)
        {
            foreach (var customBody in CustomBodies)
            {
                if (customBody.AppliesTo(worker))
                    customBody.BuildInspectorBodyUI(ui, worker, allowDuplicate, allowDestroy, memberFilter);
            }
        }

        private static void BuildCustomInspectorHeaderUI(IEnumerable<ICustomInspectorHeader> applicableCustomHeaders,
            InspectorHeaderPosition headerPosition, UIBuilder ui,
            Worker worker, bool allowDuplicate, bool allowDestroy, Predicate<ISyncMember> memberFilter)
        {
            foreach (var customHeader in applicableCustomHeaders)
            {
                customHeader.BuildInspectorHeaderUI(headerPosition, ui,
                    worker, allowDuplicate, allowDestroy, memberFilter);
            }
        }

        [HarmonyPrefix]
        private static bool BuildUIForComponentPrefix(WorkerInspector __instance, Worker worker, bool allowRemove, bool allowDuplicate, Predicate<ISyncMember> memberFilter)
        {
            memberFilter ??= Yes;
            var applicableCustomHeaders = CustomHeaders.Where(customHeader => customHeader.AppliesTo(worker)).ToArray();

            var ui = new UIBuilder(__instance.Slot);
            RadiantUI_Constants.SetupEditorStyle(ui);
            ui.VerticalLayout(6f);

            if (worker is not Slot)
            {
                ui.Style.MinHeight = 32f;
                ui.HorizontalLayout(4f);
                ui.Style.MinHeight = 24f;
                ui.Style.FlexibleWidth = 1000f;

                BuildCustomInspectorHeaderUI(applicableCustomHeaders, InspectorHeaderPosition.Start,
                    ui, worker, allowDuplicate, allowRemove, memberFilter);

                LocaleString text = $"<b>{worker.GetType().GetNiceName()}</b>";
                colorX? tint = RadiantUI_Constants.BUTTON_COLOR;

                var button = ui.ButtonRef(in text, in tint, __instance.OnWorkerTypePressed, worker);
                button.Label.Color.Value = RadiantUI_Constants.LABEL_COLOR;

                BuildCustomInspectorHeaderUI(applicableCustomHeaders, InspectorHeaderPosition.AfterName,
                    ui, worker, allowDuplicate, allowRemove, memberFilter);

                if (allowDuplicate || allowRemove)
                {
                    ui.Style.FlexibleWidth = 0f;
                    ui.Style.MinWidth = 40f;

                    if (allowDuplicate)
                    {
                        var duplicateIcon = OfficialAssets.Graphics.Icons.Inspector.Duplicate;
                        tint = RadiantUI_Constants.Sub.GREEN;

                        ButtonRefRelay<Worker> duplicateRelay = ui.Button(duplicateIcon, in tint).Slot.AttachComponent<ButtonRefRelay<Worker>>();
                        duplicateRelay.Argument.Target = worker;
                        duplicateRelay.ButtonPressed.Target = __instance.OnDuplicateComponentPressed;
                    }

                    if (allowRemove)
                    {
                        var destroyIcon = OfficialAssets.Graphics.Icons.Inspector.Destroy;
                        tint = RadiantUI_Constants.Sub.RED;

                        ButtonRefRelay<Worker> destroyRelay = ui.Button(destroyIcon, in tint).Slot.AttachComponent<ButtonRefRelay<Worker>>();
                        destroyRelay.Argument.Target = worker;
                        destroyRelay.ButtonPressed.Target = __instance.OnRemoveComponentPressed;
                    }
                }

                button.Slot.AttachComponent<ReferenceProxySource>().Reference.Target = worker;
                BuildCustomInspectorHeaderUI(applicableCustomHeaders, InspectorHeaderPosition.End,
                    ui, worker, allowDuplicate, allowRemove, memberFilter);

                ui.NestOut();
            }

            if (worker is FrooxEngine.ICustomInspector customInspector)
            {
                ui.Style.MinHeight = 24f;
                customInspector.BuildInspectorUI(ui);
            }
            else
            {
                WorkerInspector.BuildInspectorUI(worker, ui, memberFilter);
            }

            BuildCustomInspectorBodyUI(ui, worker, allowDuplicate, allowRemove, memberFilter);

            ui.Style.MinHeight = 8f;
            ui.Panel();
            ui.NestOut();

            return false;
        }

        private static bool Yes(ISyncMember _) => true;
    }
}