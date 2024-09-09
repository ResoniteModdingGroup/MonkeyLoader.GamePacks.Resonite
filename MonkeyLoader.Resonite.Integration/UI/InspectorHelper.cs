using FrooxEngine;
using FrooxEngine.UIX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.UI
{
    /// <summary>
    /// Contains helper methods to generate common elements of <see cref="WorkerInspector"/>s.
    /// </summary>
    public static class InspectorHelper
    {
        /// <summary>
        /// Constructs a reference editor field's open target's parent <see cref="Slot"/> and
        /// open target('s parent) <see cref="Worker"/> buttons in the context of the inspector header.
        /// </summary>
        /// <typeparam name="TReference">The reference target type.</typeparam>
        /// <param name="ui">The <see cref="UIBuilder"/> used to construct the buttons.</param>
        /// <param name="reference">The reference who's parents to open.</param>
        /// <returns>The modifiable <see cref="ReferenceField{T}"/> constructed to hold the <paramref name="reference"/>.</returns>
        public static ReferenceField<TReference> BuildHeaderOpenParentButtons<TReference>(UIBuilder ui, TReference reference)
            where TReference : class, IWorldElement
        {
            ui.PushStyle();
            ui.Style.FlexibleWidth = -1;
            ui.Style.MinWidth = 40;

            var button = ui.Button("⤴");

            var backingField = button.Slot.AttachComponent<ReferenceField<TReference>>();
            backingField.Reference.Target = reference;

            var refEditor = button.Slot.AttachComponent<RefEditor>();
            refEditor._targetRef.Target = backingField.Reference;

            button.Pressed.Target = refEditor.OpenInspectorButton;
            ui.Button("↑").Pressed.Target = refEditor.OpenWorkerInspectorButton;

            ui.PopStyle();

            return backingField;
        }

        /// <summary>
        /// Constructs a reference editor field that shows the given <paramref name="reference"/>.
        /// </summary>
        /// <typeparam name="TReference">The reference target type.</typeparam>
        /// <param name="ui">The <see cref="UIBuilder"/> used to construct the editor field.</param>
        /// <param name="name">The name to label the field with.</param>
        /// <param name="reference">The reference to display.</param>
        /// <returns>The modifiable <see cref="ReferenceField{T}"/> constructed to hold the <paramref name="reference"/>.</returns>
        public static ReferenceField<TReference> BuildOneWayReference<TReference>(UIBuilder ui, string name, TReference reference)
            where TReference : class, IWorldElement
        {
            var refSlot = ui.Current.AddSlot($"{name}-Reference");

            var backingField = refSlot.AttachComponent<ReferenceField<TReference>>();
            backingField.Reference.Target = reference;

            var displayField = refSlot.AttachComponent<ReferenceField<TReference>>();
            displayField.Reference.DriveFromRef(backingField.Reference);

            SyncMemberEditorBuilder.Build(displayField.Reference, name,
                displayField.GetSyncMemberFieldInfo(displayField.IndexOfMember(displayField.Reference)), ui);

            return backingField;
        }

        /// <summary>
        /// Constructs a reference editor field that shows the given <see cref="ISyncRef{T}"/>'s target.
        /// </summary>
        /// <typeparam name="TReference">The reference target's type.</typeparam>
        /// <param name="ui">The <see cref="UIBuilder"/> used to construct the editor field.</param>
        /// <param name="name">The name to label the field with.</param>
        /// <param name="syncRef">The reference who's target to display.</param>
        public static void BuildOneWayReference<TReference>(UIBuilder ui, string name, ISyncRef<TReference> syncRef)
            where TReference : class, IWorldElement
        {
            var worker = syncRef.FindNearestParent<Worker>();
            var fieldInfo = worker.GetSyncMemberFieldInfo(worker.IndexOfMember(syncRef));

            var refSlot = ui.Current.AddSlot($"{name}-Reference");

            var referenceField = refSlot.AttachComponent<ReferenceField<TReference>>();
            referenceField.Reference.DriveFromRef(syncRef);

            SyncMemberEditorBuilder.Build(referenceField.Reference, name, fieldInfo, ui);
        }

        /// <summary>
        /// Constructs a value editor field that shows the given <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="ui">The <see cref="UIBuilder"/> used to construct the editor field.</param>
        /// <param name="name">The name to label the field with.</param>
        /// <param name="value">The value to display.</param>
        /// <returns>The modifiable <see cref="ValueField{T}"/> constructed to hold the <paramref name="value"/>.</returns>
        public static ValueField<TValue> BuildOneWayValue<TValue>(UIBuilder ui, string name, TValue value)
            where TValue : class, IWorldElement
        {
            var refSlot = ui.Current.AddSlot($"{name}-Value");

            var backingField = refSlot.AttachComponent<ValueField<TValue>>();
            backingField.Value.Value = value;

            var displayField = refSlot.AttachComponent<ValueField<TValue>>();
            displayField.Value.DriveFrom(backingField.Value);

            SyncMemberEditorBuilder.Build(displayField.Value, name,
                displayField.GetSyncMemberFieldInfo(displayField.IndexOfMember(displayField.Value)), ui);

            return backingField;
        }

        /// <summary>
        /// Constructs a value editor field that shows the given <see cref="IField{T}"/>'s value.
        /// </summary>
        /// <typeparam name="TValue">The value target.</typeparam>
        /// <param name="ui">The <see cref="UIBuilder"/> used to construct the editor field.</param>
        /// <param name="name">The name to label the field with.</param>
        /// <param name="field">The field who's value to display.</param>
        public static void BuildOneWayValue<TValue>(UIBuilder ui, string name, IField<TValue> field)
            where TValue : class, IWorldElement
        {
            var worker = field.FindNearestParent<Worker>();
            var fieldInfo = worker.GetSyncMemberFieldInfo(worker.IndexOfMember(field));

            var refSlot = ui.Current.AddSlot($"{name}-Value");

            var valueField = refSlot.AttachComponent<ValueField<TValue>>();
            valueField.Value.DriveFrom(field);

            SyncMemberEditorBuilder.Build(valueField.Value, name, fieldInfo, ui);
        }

        /// <summary>
        /// Constructs a reference editor field that is linked with the given <see cref="ISyncRef{T}"/>.
        /// </summary>
        /// <typeparam name="TReference">The reference target's type.</typeparam>
        /// <param name="ui">The <see cref="UIBuilder"/> used to construct the editor field.</param>
        /// <param name="name">The name to label the field with.</param>
        /// <param name="syncRef">The reference to link with.</param>
        public static void BuildTwoWayReference<TReference>(UIBuilder ui, string name, ISyncRef<TReference> syncRef)
            where TReference : class, IWorldElement
        {
            var worker = syncRef.FindNearestParent<Worker>();
            var fieldInfo = worker.GetSyncMemberFieldInfo(worker.IndexOfMember(syncRef));

            SyncMemberEditorBuilder.Build(syncRef, name, fieldInfo, ui);
        }

        /// <summary>
        /// Constructs a value editor field that is linked with the given <see cref="IField{T}"/>.
        /// </summary>
        /// <typeparam name="TValue">The value target.</typeparam>
        /// <param name="ui">The <see cref="UIBuilder"/> used to construct the editor field.</param>
        /// <param name="name">The name to label the field with.</param>
        /// <param name="field">The field to link with.</param>
        public static void BuildTwoWayValue<TValue>(UIBuilder ui, string name, IField<TValue> field)
            where TValue : class, IWorldElement
        {
            var worker = field.FindNearestParent<Worker>();
            var fieldInfo = worker.GetSyncMemberFieldInfo(worker.IndexOfMember(field));

            SyncMemberEditorBuilder.Build(field, name, fieldInfo, ui);
        }
    }
}