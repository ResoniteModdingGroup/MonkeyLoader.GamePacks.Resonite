using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Contains extension methods related to <see cref="DynamicVariableSpace"/>s.
    /// </summary>
    public static class DynamicVariableExtensions
    {
        //public static DynamicReference<T> CreateReferenceVariable<T>(this SyncRef<T> syncRef, string name, bool overrideOnLink = false, bool persistent = true)
        //    where T : class, IWorldElement
        //{
        //    var variable = syncRef.FindNearestParent<Slot>().AttachComponent<DynamicReference<T>>();
        //    variable.TargetReference.Target = syncRef;
        //    variable.VariableName.Value = name;
        //    variable.OverrideOnLink.Value = overrideOnLink;
        //    variable.Persistent = persistent;

        //    return variable;
        //}

        /// <summary>
        /// Attaches a <see cref="DynamicVariableSpace"/> component to this slot with
        /// the given <paramref name="spaceName"/> and optionally enables
        /// <see cref="DynamicVariableSpace.OnlyDirectBinding">OnlyDirectBinding</see>.
        /// </summary>
        /// <param name="slot">The slot to create the component on.</param>
        /// <param name="spaceName">The name for the created space.</param>
        /// <param name="onlyDirectBinding">Whether only variables with a matching space name should bind to the created space.</param>
        /// <returns>The created <see cref="DynamicVariableSpace"/> component.</returns>
        public static DynamicVariableSpace CreateSpace(this Slot slot, string? spaceName, bool onlyDirectBinding = false)
        {
            var space = slot.AttachComponent<DynamicVariableSpace>();
            space.SpaceName.Value = spaceName!;
            space.OnlyDirectBinding.Value = onlyDirectBinding;

            return space;
        }

        /// <summary>
        /// <see cref="DynamicVariableHelper.FindSpace">Searches</see> for
        /// a <see cref="DynamicVariableSpace"/> with the given <paramref name="spaceName"/>
        /// that covers this slot, or attaches one to it with the given parameters.
        /// </summary>
        /// <remarks>
        /// The found <see cref="DynamicVariableSpace"/>'s
        /// <see cref="DynamicVariableSpace.OnlyDirectBinding">OnlyDirectBinding</see>
        /// option may differ from the supplied value.
        /// </remarks>
        /// <returns>The found or attached <see cref="DynamicVariableSpace"/>.</returns>
        /// <inheritdoc cref="CreateSpace"/>
        public static DynamicVariableSpace FindOrCreateSpace(this Slot slot, string? spaceName, bool onlyDirectBinding = false)
            => slot.FindSpace(spaceName!) ?? CreateSpace(slot, spaceName, onlyDirectBinding);

        /// <summary>
        /// Gets the <see cref="DynamicVariableSpace"/> that this
        /// <see cref="IDynamicVariable">dynamic variable</see> is linked to.
        /// </summary>
        /// <param name="dynamicVariable">The dynamic variable of which to get the linked <see cref="DynamicVariableSpace"/>.</param>
        /// <returns>The <see cref="DynamicVariableSpace"/> that this <see cref="IDynamicVariable">dynamic variable</see> is linked to.</returns>
        /// <exception cref="NullReferenceException">
        /// When this <see cref="IDynamicVariable">dynamic variable</see> is not linked to a <see cref="DynamicVariableSpace"/>.
        /// </exception>
        public static DynamicVariableSpace GetLinkedSpace(this IDynamicVariable dynamicVariable)
        {
            if (dynamicVariable.TryGetLinkedSpace(out var space))
                return space;

            throw new NullReferenceException("Dynamic variable is not linked against a space!");
        }

        /// <inheritdoc cref="GetLinkedVariables"/>
        /// <typeparam name="T">The type of the values of the <see cref="IDynamicVariable{T}">dynamic variables</see>.</typeparam>
        public static IEnumerable<IDynamicVariable<T>> GetLinkedVariables<T>(this DynamicVariableSpace space,
            Predicate<IDynamicVariable<T>>? filter = null, bool includeLocal = false, bool excludeDisabled = false, Predicate<Slot>? slotFilter = null)
        {
            filter ??= Filter;

            return space.Slot.GetComponentsInChildren<IDynamicVariable<T>>(
                variable => variable.IsLinkedToSpace(space) && filter(variable),
                includeLocal, excludeDisabled, slotFilter!);
        }

        /// <summary>
        /// <see cref="Slot.GetComponentsInChildren{T}(Predicate{T}, bool, bool, Predicate{Slot})">Gets</see>
        /// all <see cref="IDynamicVariable{T}">dynamic variable</see> components linked to this
        /// <see cref="DynamicVariableSpace"/> that fulfill the given conditions.
        /// </summary>
        /// <param name="space">The dynamic variable space to get the linked variables of.</param>
        /// <param name="filter">A predicate to filter the linked variables returned; or <c>null</c> for all.</param>
        /// <param name="includeLocal">Whether to include local <see cref="Slot"/>s in the search.</param>
        /// <param name="excludeDisabled">Whether to exclude disabled components from the results.</param>
        /// <param name="slotFilter">
        /// A predicate to filter the <see cref="Slot"/>s included in the search; or <c>null</c> for all.<br/>
        /// The children of excluded <see cref="Slot"/>s are excluded as well.
        /// </param>
        /// <returns>
        /// The <see cref="IDynamicVariable{T}">dynamic variable</see> components linked to this
        /// <see cref="DynamicVariableSpace"/> that fulfill the given conditions.
        /// </returns>
        public static IEnumerable<IDynamicVariable> GetLinkedVariables(this DynamicVariableSpace space,
            Predicate<IDynamicVariable>? filter = null, bool includeLocal = false, bool excludeDisabled = false, Predicate<Slot>? slotFilter = null)
        {
            filter ??= Filter;

            return space.Slot.GetComponentsInChildren<IDynamicVariable>(
                variable => variable.IsLinkedToSpace(space) && filter(variable),
                includeLocal, excludeDisabled, slotFilter!);
        }

        /// <summary>
        /// Searches for a <see cref="DynamicField{T}"/> targetting this field
        /// and exposing it with the given <paramref name="variable"/> name on the nearest Slot,
        /// or attaches one with the given parameters to it.
        /// </summary>
        /// <typeparam name="T">The type of the field's value.</typeparam>
        /// <param name="field">The field targetted by the <see cref="DynamicField{T}"/>.</param>
        /// <param name="variable">The <see cref="DynamicVariableBase{T}.VariableName">VariableName</see> of the <see cref="DynamicField{T}"/>.</param>
        /// <param name="setupReset">Whether to also attach a <see cref="DynamicValueVariableReset{T}"/> with the current value for load, paste, and duplicate.</param>
        /// <param name="forceCurrentValue">Whether to force the <see cref="DynamicVariableSpace"/>'s value for the variable to the field's current value when attaching a <see cref="DynamicField{T}"/>.</param>
        /// <returns>The found or attached <see cref="DynamicField{T}"/>.</returns>
        public static DynamicField<T> GetSyncWithVariable<T>(this IField<T> field, string variable, bool setupReset = false, bool forceCurrentValue = false)
        {
            var slot = field.FindNearestParent<Slot>();

            if (slot.GetComponent<DynamicField<T>>(dynField => dynField.TargetField.Target == field && dynField.VariableName == variable) is DynamicField<T> foundField)
                return foundField;

            return field.SyncWithVariable(variable, setupReset, forceCurrentValue);
        }

        /// <summary>
        /// Searches for a <see cref="DynamicReference{T}"/> targetting this reference
        /// and exposing it with the given <paramref name="variable"/> name on the nearest Slot,
        /// or attaches one with the given parameters to it.
        /// </summary>
        /// <typeparam name="T">The type of the reference's target.</typeparam>
        /// <param name="reference">The reference targetted by the <see cref="DynamicReference{T}"/>.</param>
        /// <param name="variable">The <see cref="DynamicVariableBase{T}.VariableName">VariableName</see> of the <see cref="DynamicReference{T}"/>.</param>
        /// <param name="setupReset">Whether to also attach a <see cref="DynamicValueVariableReset{T}"/> with the current target for load, paste, and duplicate.</param>
        /// <param name="forceCurrentValue">Whether to force the <see cref="DynamicVariableSpace"/>'s target for the variable to the references's current target when attaching a <see cref="DynamicReference{T}"/>.</param>
        /// <returns>The found or attached <see cref="DynamicReference{T}"/>.</returns>
        public static DynamicReference<T> GetSyncWithVariable<T>(this SyncRef<T> reference, string variable, bool setupReset = false, bool forceCurrentValue = false)
            where T : class, IWorldElement
        {
            var slot = reference.FindNearestParent<Slot>();

            if (slot.GetComponent<DynamicReference<T>>(dynReference => dynReference.TargetReference.Target == reference && dynReference.VariableName == variable) is DynamicReference<T> foundReference)
                return foundReference;

            return reference.SyncWithVariable(variable, setupReset, forceCurrentValue);
        }

        /// <summary>
        /// Searches for a <see cref="DynamicTypeField"/> targetting this <see cref="Type"/> field
        /// and exposing it with the given <paramref name="variable"/> name on the nearest Slot,
        /// or attaches one with the given parameters to it.
        /// </summary>
        /// <param name="typeField">The <see cref="Type"/> field targetted by the <see cref="DynamicTypeField"/>.</param>
        /// <param name="variable">The <see cref="DynamicVariableBase{T}.VariableName">VariableName</see> of the <see cref="DynamicTypeField"/>.</param>
        /// <param name="setupReset">Whether to also attach a <see cref="DynamicValueVariableReset{T}"/> with the current <see cref="Type"/> for load, paste, and duplicate.</param>
        /// <param name="forceCurrentValue">Whether to force the <see cref="DynamicVariableSpace"/>'s <see cref="Type"/> for the variable to the references's current <see cref="Type"/> when attaching a <see cref="DynamicTypeField"/>.</param>
        /// <returns>The found or attached <see cref="DynamicTypeField"/>.</returns>
        public static DynamicTypeField GetSyncWithVariable(this SyncType typeField, string variable, bool setupReset = false, bool forceCurrentValue = false)
        {
            var slot = typeField.FindNearestParent<Slot>();

            if (slot.GetComponent<DynamicTypeField>(dynTypeField => dynTypeField.TargetField.Target == typeField && dynTypeField.VariableName == variable) is DynamicTypeField foundType)
                return foundType;

            return typeField.SyncWithVariable(variable, setupReset, forceCurrentValue);
        }

        //public static DynamicField<T>? CreateVariable<T>(this IField<T> field, string name, bool overrideOnLink = false, bool persistent = true)
        //{
        //    var variable = field.FindNearestParent<Slot>().AttachComponent<DynamicField<T>>();
        //    variable.TargetField.Target = field;
        //    variable.VariableName.Value = name;
        //    variable.OverrideOnLink.Value = overrideOnLink;
        //    variable.Persistent = persistent;

        //    return variable;
        //}

        //public static DynamicReferenceVariableDriver<T> DriveReferenceFromVariable<T>(this SyncRef<T> syncRef, string name, T? defaultTarget = default, bool persistent = true)
        //    where T : class, IWorldElement
        //{
        //    var driver = syncRef.FindNearestParent<Slot>().AttachComponent<DynamicReferenceVariableDriver<T>>();
        //    driver.Target.Target = syncRef;
        //    driver.VariableName.Value = name;
        //    driver.DefaultTarget.Target = defaultTarget!;
        //    driver.Persistent = persistent;

        //    return driver;
        //}

        //public static DynamicValueVariableDriver<T> DriveValueFromVariable<T>(this IField<T> field, string name, T? defaultValue = default, bool persistent = true)
        //{
        //    var driver = field.FindNearestParent<Slot>().AttachComponent<DynamicValueVariableDriver<T>>();
        //    driver.Target.Target = field;
        //    driver.VariableName.Value = name;
        //    driver.DefaultValue.Value = defaultValue!;
        //    driver.Persistent = persistent;

        //    return driver;
        //}

        /// <summary>
        /// Determines whether this <see cref="IDynamicVariable">dynamic variable</see>
        /// is linked against a <see cref="DynamicVariableSpace"/>.
        /// </summary>
        /// <param name="dynamicVariable">The dynamic variable for which to check for a linked <see cref="DynamicVariableSpace"/>.</param>
        /// <returns>
        /// <c>true</c> if this <see cref="IDynamicVariable">dynamic variable</see>
        /// is linked to a <see cref="DynamicVariableSpace"/>; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsLinkedToSpace(this IDynamicVariable dynamicVariable)
            => dynamicVariable.TryGetLinkedSpace(out _);

        /// <summary>
        /// Determines whether this <see cref="IDynamicVariable">dynamic variable</see>
        /// is linked against the given <see cref="DynamicVariableSpace"/>.
        /// </summary>
        /// <param name="dynamicVariable">The dynamic variable for which to check the linked <see cref="DynamicVariableSpace"/>.</param>
        /// <param name="space">The dynamic variable space to compare to the linked one.</param>
        /// <returns>
        /// <c>true</c> if this <see cref="IDynamicVariable">dynamic variable</see>
        /// is linked to the given <see cref="DynamicVariableSpace"/>; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsLinkedToSpace(this IDynamicVariable dynamicVariable, DynamicVariableSpace space)
            => dynamicVariable.TryGetLinkedSpace(out var linkedSpace) && linkedSpace == space;

        /// <summary>
        /// Tests whether this string is a <see cref="DynamicVariableHelper.IsValidName">valid dynamic variable name</see>.
        /// </summary>
        /// <param name="variableName">The string to test.</param>
        /// <returns><c>true</c> if this string is a valid name; otherwise, <c>false</c>.</returns>
        public static bool IsValidName(this string? variableName)
            => DynamicVariableHelper.IsValidName(variableName!);

        /// <summary>
        /// <see cref="DynamicVariableHelper.ParsePath">Parses</see> this string into
        /// <see cref="ProcessName">processed</see> dynamic variable space and path names, if possible.
        /// </summary>
        /// <param name="path">The string to parse.</param>
        /// <param name="spaceName">The parsed space name, or <c>null</c> if not applicable.</param>
        /// <param name="variableName">The parsed variable name, or <c>null</c> if not applicable.</param>
        public static void ParseAsPath(this string? path, out string? spaceName, out string? variableName)
            => DynamicVariableHelper.ParsePath(path!, out spaceName, out variableName);

        /// <summary>
        /// <see cref="DynamicVariableHelper.ProcessName">Processes</see>
        /// this string to a <see cref="string.Trim()">trimmed</see>
        /// and <see cref="IsValidName">valid</see> dynamic variable (space) name.
        /// </summary>
        /// <param name="variableName">The string to process.</param>
        /// <returns>The <see cref="string.Trim()">trimmed</see> name if <see cref="IsValidName">valid</see>; otherwise, <c>null</c>.</returns>
        public static string? ProcessName(this string? variableName)
            => DynamicVariableHelper.ProcessName(variableName!);

        /// <summary>
        /// Tries to get the <see cref="DynamicVariableSpace"/> that this
        /// <see cref="IDynamicVariable">dynamic variable</see> is linked to.
        /// </summary>
        /// <param name="dynamicVariable">The dynamic variable of which to get the linked <see cref="DynamicVariableSpace"/>.</param>
        /// <param name="linkedSpace">
        /// The <see cref="DynamicVariableSpace"/> that this
        /// <see cref="IDynamicVariable">dynamic variable</see> is linked to; otherwise, <c>null</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if this <see cref="IDynamicVariable">dynamic variable</see>
        /// is linked to a <see cref="DynamicVariableSpace"/>; otherwise, <c>false</c>.
        /// </returns>
        public static bool TryGetLinkedSpace(this IDynamicVariable dynamicVariable, [NotNullWhen(true)] out DynamicVariableSpace? linkedSpace)
        {
            linkedSpace = Traverse.Create(dynamicVariable)
                .Field(nameof(DynamicVariableBase<dummy>.handler))
                .Field(nameof(DynamicVariableHandler<dummy>._currentSpace))
                .GetValue<DynamicVariableSpace>();

            return linkedSpace is not null;
        }

        private static bool Filter<T>(T item) => true;
    }
}