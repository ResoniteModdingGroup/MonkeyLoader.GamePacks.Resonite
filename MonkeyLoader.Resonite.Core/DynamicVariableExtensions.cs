using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Contains extension methods related to <see cref="DynamicVariableSpace"/>s
    /// and their <see cref="IDynamicVariable">variables</see>.
    /// </summary>
    [TypeForwardedFrom("MonkeyLoader.Resonite.DynamicVariableExtensions, MonkeyLoader.Resonite.Integration")]
    public static class DynamicVariableExtensions
    {
        /// <summary>
        /// The name of the shared config slot in a <see cref="World"/>'s
        /// <see cref="World.AssetsSlot">Assets</see> <see cref="Slot"/>.<br/>
        /// The prefix for the names of Dynamic Variables that are part of the shared config.
        /// </summary>
        public const string SharedConfigIdentifier = "MonkeyLoader.SharedConfig";

        private const string SharedConfigVariableNamePrefix = $"World/{SharedConfigIdentifier}";

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
        /// Gets all <see cref="IDynamicVariable{T}">dynamic variable</see> components linked to this
        /// <see cref="DynamicVariableSpace"/> that optionally have to pass the given filter <paramref name="predicate"/>.
        /// </summary>
        /// <param name="space">The dynamic variable space to get the linked variables of.</param>
        /// <param name="predicate">Optional predicate to filter the potential linked variables with.</param>
        /// <returns>
        /// The <see cref="IDynamicVariable{T}">dynamic variable</see> components linked to this
        /// <see cref="DynamicVariableSpace"/> that optionally pass the given filter <paramref name="predicate"/>.
        /// </returns>
        public static IEnumerable<IDynamicVariable> GetAllLinkedVariables(this DynamicVariableSpace space, Func<IDynamicVariable, bool>? predicate = null)
        {
            predicate ??= Filter;

            return [.. space._dynamicValues.Values
                .SelectMany(manager =>
                    Traverse.Create(manager)
                    .Field("values")
                    .GetValue<IEnumerable<IDynamicVariable>>())
                .Where(predicate)];
        }

        /// <inheritdoc cref="GetAllLinkedVariables(DynamicVariableSpace, Func{IDynamicVariable, bool}?)"/>
        /// <typeparam name="T">The type of the values of the <see cref="IDynamicVariable{T}">dynamic variables</see>.</typeparam>
        public static IEnumerable<IDynamicVariable<T>> GetAllLinkedVariables<T>(this DynamicVariableSpace space, Func<IDynamicVariable<T>, bool>? predicate = null)
        {
            predicate ??= Filter;

            return [.. space._dynamicValues.Values
                .OfType<DynamicVariableSpace.ValueManager<T>>()
                .SelectMany(manager => manager.values)
                .Where(predicate)];
        }

        /// <summary>
        /// Gets all <see cref="IDynamicVariable{T}">dynamic variable</see> components linked to this
        /// <see cref="DynamicVariableSpace"/> that have the given <paramref name="type"/> and <paramref name="name"/>.
        /// </summary>
        /// <param name="space">The dynamic variable space to get the linked variables of.</param>
        /// <param name="type">The type of the values of the <see cref="IDynamicVariable{T}">dynamic variables</see>.</param>
        /// <param name="name">The name of the <see cref="IDynamicVariable{T}">dynamic variables</see>.</param>
        /// <returns>
        /// All <see cref="IDynamicVariable{T}">dynamic variable</see> components linked to this
        /// <see cref="DynamicVariableSpace"/> that have the given <paramref name="type"/> and <paramref name="name"/>.
        /// </returns>
        public static IEnumerable<IDynamicVariable> GetAllLinkedVariablesMatching(this DynamicVariableSpace space, Type type, string name)
        {
            name.ParseAsPath(out _, out var variableName);

            if (variableName is null || !space._dynamicValues.TryGetValue(new(type, variableName), out var valueManager))
                return [];

            return [.. Traverse.Create(valueManager).Field("values").GetValue<IEnumerable<IDynamicVariable>>()];
        }

        /// <summary>
        /// Gets all <see cref="IDynamicVariable{T}">dynamic variable</see> components linked to this
        /// <see cref="DynamicVariableSpace"/> that match the given <paramref name="identity"/> in all but the space.
        /// </summary>
        /// <param name="space">The dynamic variable space to get the linked variables of.</param>
        /// <param name="identity">The Dynamic Variable identity describing the <see cref="Type"/> and name of the <see cref="IDynamicVariable{T}">dynamic variables</see>.</param>
        /// <returns>
        /// All <see cref="IDynamicVariable{T}">dynamic variable</see> components linked to this
        /// <see cref="DynamicVariableSpace"/> that have the <see cref="Type"/> and name defined by the given <paramref name="identity"/>.
        /// </returns>
        public static IEnumerable<IDynamicVariable> GetAllLinkedVariablesMatching(this DynamicVariableSpace space, DynamicVariableIdentity identity)
            => space.GetAllLinkedVariablesMatching(identity.Type, identity.Name);

        /// <inheritdoc cref="GetAllLinkedVariablesMatching(DynamicVariableSpace, Type, string)"/>
        /// <typeparam name="T">The type of the values of the <see cref="IDynamicVariable{T}">dynamic variables</see>.</typeparam>
        public static IEnumerable<IDynamicVariable<T>> GetAllLinkedVariablesMatching<T>(this DynamicVariableSpace space, string name)
        {
            name.ParseAsPath(out _, out var variableName);

            if (variableName is null || !space._dynamicValues.TryGetValue(new(typeof(T), variableName), out var valueManager))
                return [];

            return [.. ((DynamicVariableSpace.ValueManager<T>)valueManager).values];
        }

        /// <summary>
        /// Gets all <see cref="DynamicVariableSpace"/>s that apply to this <see cref="Slot"/>.<br/>
        /// Optionally includes only those that pass through the <paramref name="filter"/>.
        /// </summary>
        /// <remarks>
        /// This function is different from a regular
        /// <c><see cref="Slot"/>.<see cref="Slot.GetComponentsInParents{T}(Predicate{T})">GetComponentsInParents</see>&lt;<see cref="DynamicVariableSpace"/>&gt;()</c> call,<br/>
        /// as it considers that <see cref="DynamicVariableSpace"/>s deeper in the hierarchy
        /// hide ones higher up with the same <see cref="DynamicVariableSpace.CurrentName">name</see>.<br/>
        /// If a <see cref="DynamicVariableSpace"/> deeper in the hierarchy doesn't pass the <paramref name="filter"/>,<br/>
        /// it will still hide any higher up ones with the same <see cref="DynamicVariableSpace.CurrentName">name</see>
        /// that might pass it, because they don't apply to this <see cref="Slot"/>.
        /// </remarks>
        /// <param name="slot">The <see cref="Slot"/> to find all applicable <see cref="DynamicVariableSpace"/>s for.</param>
        /// <param name="filter">The optional predicate to apply to the returned </param>
        /// <returns>All <see cref="DynamicVariableSpace"/> that apply to this <see cref="Slot"/> and optionally pass the <paramref name="filter"/>.</returns>
        public static IEnumerable<DynamicVariableSpace> GetAvailableSpaces(this Slot slot, Predicate<DynamicVariableSpace>? filter = null)
        {
            filter ??= Filter;
            var result = new List<DynamicVariableSpace>();
            var presentNames = Pool.BorrowHashSet<string>();

            while (slot is not null)
            {
                var currentSpaces = slot.GetComponents<DynamicVariableSpace>();

                foreach (var currentSpace in currentSpaces)
                {
                    // Use CurrentName because it includes the processing of the SpaceName
                    if (!presentNames.Add(currentSpace.CurrentName))
                        continue;

                    if (!filter(currentSpace))
                        continue;

                    result.Add(currentSpace);
                }

                slot = slot.Parent;
            }

            Pool.Return(ref presentNames);

            return [.. result];
        }

        /// <summary>
        /// Gets all <see cref="DynamicVariableIdentity">full Dynamic Variable
        /// identities</see> that apply to this <see cref="Slot"/>
        /// </summary>
        /// <returns>All full Dynamic Variable identities that apply to this <see cref="Slot"/>.</returns>
        /// <inheritdoc cref="GetAvailableVariableIdentities(Slot, Type, string)"/>
        public static IEnumerable<DynamicVariableIdentity> GetAvailableVariableIdentities(this Slot slot)
            => [.. slot.GetAvailableSpaces().SelectMany(GetVariableIdentities)];

        /// <summary>
        /// Gets all <see cref="DynamicVariableIdentity">full Dynamic Variable
        /// identities</see> that apply to this <see cref="Slot"/>,
        /// which can be assigned a value of type <typeparamref name="T"/>.
        /// </summary>
        /// <returns>All full Dynamic Variable identities that apply to this <see cref="Slot"/> that <typeparamref name="T"/> can be assigned to.</returns>
        /// <inheritdoc cref="GetAvailableVariableIdentities{T}(Slot, string)"/>
        public static IEnumerable<DynamicVariableIdentity> GetAvailableVariableIdentities<T>(this Slot slot)
            => [.. slot.GetAvailableSpaces().SelectMany(GetVariableIdentities<T>)];

        /// <summary>
        /// Gets all <see cref="DynamicVariableIdentity">full Dynamic Variable
        /// identities</see> that apply to this <see cref="Slot"/>,
        /// which can be assigned a value of the given <see cref="Type"/>.
        /// </summary>
        /// <returns>All full Dynamic Variable identities that apply to this <see cref="Slot"/> that the given <see cref="Type"/> can be assigned to.</returns>
        /// <inheritdoc cref="GetAvailableVariableIdentities(Slot, Type, string)"/>
        public static IEnumerable<DynamicVariableIdentity> GetAvailableVariableIdentities(this Slot slot, Type type)
            => [.. slot.GetAvailableSpaces().SelectMany(space => space.GetVariableIdentities(type))];

        /// <summary>
        /// Gets all <see cref="DynamicVariableIdentity">full Dynamic Variable
        /// identities</see> that apply to this <see cref="Slot"/>,
        /// which match the given <paramref name="name"/>.
        /// </summary>
        /// <returns>All full Dynamic Variable identities that apply to this <see cref="Slot"/> that have the given <paramref name="name"/>.</returns>
        /// <inheritdoc cref="GetAvailableVariableIdentities(Slot, Type, string)"/>
        public static IEnumerable<DynamicVariableIdentity> GetAvailableVariableIdentities(this Slot slot, string name)
            => [.. slot.GetAvailableSpaces().SelectMany(space => space.GetVariableIdentities(name))];

        /// <summary>
        /// Gets all <see cref="DynamicVariableIdentity">full Dynamic Variable
        /// identities</see> that apply to this <see cref="Slot"/>,
        /// which can be assigned a value of type <typeparamref name="T"/> and
        /// which match the given <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="T">The type that must be assignable to the variables.</typeparam>
        /// <returns>All full Dynamic Variable identities that apply to this <see cref="Slot"/>
        /// that <typeparamref name="T"/> can be assigned to and that have the given <paramref name="name"/>.
        /// </returns>
        /// <inheritdoc cref="GetAvailableVariableIdentities(Slot, Type, string)"/>
        public static IEnumerable<DynamicVariableIdentity> GetAvailableVariableIdentities<T>(this Slot slot, string name)
            => [.. slot.GetAvailableSpaces().SelectMany(GetVariableIdentities<T>).Where(id => id.Name == name)];

        /// <summary>
        /// Gets all <see cref="DynamicVariableIdentity">full Dynamic Variable
        /// identities</see> that apply to this <see cref="Slot"/>,
        /// which can be assigned a value of the given <see cref="Type"/> and
        /// which match the given <paramref name="name"/>.
        /// </summary>
        /// <remarks>
        /// This method uses
        /// <c><see cref="Slot">slot</see>.<see cref="GetAvailableSpaces(Slot, Predicate{DynamicVariableSpace}?)">GetAvailableSpaces</see>()</c>,<br/>
        /// which is different from a regular
        /// <c><see cref="Slot">slot</see>.<see cref="Slot.GetComponentInParents{T}(Predicate{T}, bool, bool)">GetComponentsInParents</see>&lt;<see cref="DynamicVariableSpace"/>&gt;()</c>
        /// call,<br/> as it considers that <see cref="DynamicVariableSpace"/>s deeper in the hierarchy
        /// hide ones higher up with the same <see cref="DynamicVariableSpace.SpaceName">name</see>.
        /// </remarks>
        /// <param name="slot">The <see cref="Slot"/> to find all applicable full Dynamic Variable identities for.</param>
        /// <param name="type">The type that must be assignable to the variables.</param>
        /// <param name="name">The name that the variable identities must have.</param>
        /// <returns>
        /// All full Dynamic Variable identities that apply to this <see cref="Slot"/>
        /// that the given <see cref="Type"/> can be assigned to and that have the given <paramref name="name"/>.
        /// </returns>
        public static IEnumerable<DynamicVariableIdentity> GetAvailableVariableIdentities(this Slot slot, Type type, string name)
            => [.. slot.GetAvailableSpaces().SelectMany(space => space.GetVariableIdentities(type)).Where(id => id.Name == name)];

        /// <inheritdoc cref="GetAvailableVariableIdentities(Slot)"/>
        public static IDictionary<DynamicVariableSpace, IEnumerable<DynamicVariableIdentity>> GetAvailableVariableIdentitiesBySpace(this Slot slot)
            => slot.GetAvailableSpaces().ToDictionary(Id, space => (IEnumerable<DynamicVariableIdentity>)[.. space.GetVariableIdentities()]);

        /// <inheritdoc cref="GetAvailableVariableIdentities{T}(Slot)"/>
        public static IDictionary<DynamicVariableSpace, IEnumerable<DynamicVariableIdentity>> GetAvailableVariableIdentitiesBySpace<T>(this Slot slot)
            => slot.GetAvailableSpaces().ToDictionary(Id, space => (IEnumerable<DynamicVariableIdentity>)[.. space.GetVariableIdentities<T>()]);

        /// <inheritdoc cref="GetAvailableVariableIdentities(Slot, Type)"/>
        public static IDictionary<DynamicVariableSpace, IEnumerable<DynamicVariableIdentity>> GetAvailableVariableIdentitiesBySpace(this Slot slot, Type type)
            => slot.GetAvailableSpaces().ToDictionary(Id, space => (IEnumerable<DynamicVariableIdentity>)[.. space.GetVariableIdentities(type)]);

        /// <inheritdoc cref="GetAvailableVariableIdentities(Slot, string)"/>
        public static IDictionary<DynamicVariableSpace, IEnumerable<DynamicVariableIdentity>> GetAvailableVariableIdentitiesBySpace(this Slot slot, string name)
            => slot.GetAvailableSpaces().ToDictionary(Id, space => (IEnumerable<DynamicVariableIdentity>)[.. space.GetVariableIdentities(name)]);

        /// <summary>
        /// Gets the <see cref="DynamicVariableHandler{T}"/> of this
        /// <see cref="IDynamicVariable{T}">dynamic variable</see>.
        /// </summary>
        /// <param name="dynamicVariable">The dynamic variable of which to get the <see cref="DynamicVariableHandler{T}"/>.</param>
        /// <returns>The <see cref="DynamicVariableHandler{T}"/> of this <see cref="IDynamicVariable{T}">dynamic variable</see>.</returns>
        /// <exception cref="NullReferenceException">
        /// When the <see cref="DynamicVariableHandler{T}"/> of this <see cref="IDynamicVariable{T}">dynamic variable</see> could not be found.
        /// </exception>
        public static DynamicVariableHandler<T> GetHandler<T>(this IDynamicVariable<T> dynamicVariable)
        {
            if (dynamicVariable.TryGetHandler(out var handler))
                return handler;

            throw new NullReferenceException("Dynamic variable handler could not be found!");
        }

        /// <summary>
        /// Gets the full <see cref="IDynamicVariable{T}">Dynamic Variable</see> identity of this dynamic variable component.
        /// </summary>
        /// <returns>The full identity of this dynamic variable.</returns>
        /// <exception cref="NullReferenceException">When this dynamic variable isn't <see cref="GetLinkedSpace">linked</see> to a <see cref="DynamicVariableSpace"/>.</exception>
        /// <inheritdoc cref="TryGetIdentity(IDynamicVariable, out DynamicVariableIdentity?)"/>
        public static DynamicVariableIdentity GetIdentity(this IDynamicVariable variable)
        {
            if (!variable.TryGetIdentity(out var identity))
                throw new NullReferenceException("Dynamic variable is not linked against a space or it could not be found!");

            return identity.Value;
        }

        /// <summary>
        /// Gets the <see cref="DynamicVariableSpace"/> that this
        /// <see cref="IDynamicVariable">dynamic variable</see> is linked to.
        /// </summary>
        /// <param name="dynamicVariable">The dynamic variable of which to get the linked <see cref="DynamicVariableSpace"/>.</param>
        /// <returns>The <see cref="DynamicVariableSpace"/> that this <see cref="IDynamicVariable">dynamic variable</see> is linked to.</returns>
        /// <exception cref="NullReferenceException">
        /// When this <see cref="IDynamicVariable">dynamic variable</see> is
        /// not linked to a <see cref="DynamicVariableSpace"/> or it could not be found.
        /// </exception>
        public static DynamicVariableSpace GetLinkedSpace(this IDynamicVariable dynamicVariable)
        {
            if (dynamicVariable.TryGetLinkedSpace(out var space))
                return space;

            throw new NullReferenceException("Dynamic variable is not linked against a space or it could not be found!");
        }

        /// <summary>
        /// <see cref="Slot.GetComponentsInChildren{T}(Predicate{T}, bool, bool, Predicate{Slot})">Gets</see>
        /// all <see cref="IDynamicVariable{T}">dynamic variable</see> components linked to this
        /// <see cref="DynamicVariableSpace"/> that fulfill the given conditions.
        /// </summary>
        /// <param name="space">The dynamic variable space to get the linked variables of.</param>
        /// <param name="filter">A predicate to filter the linked variables returned; or <c>null</c> for all.</param>
        /// <param name="excludeDisabled">Whether to exclude disabled components from the results.</param>
        /// <param name="includeLocal">Whether to include local <see cref="Slot"/>s in the search.</param>
        /// <param name="slotFilter">
        /// A predicate to filter the <see cref="Slot"/>s included in the search; or <c>null</c> for all.<br/>
        /// The children of excluded <see cref="Slot"/>s are excluded as well.
        /// </param>
        /// <returns>
        /// The <see cref="IDynamicVariable{T}">dynamic variable</see> components linked to this
        /// <see cref="DynamicVariableSpace"/> that fulfill the given conditions.
        /// </returns>
        public static IEnumerable<IDynamicVariable> GetLinkedVariables(this DynamicVariableSpace space,
            Predicate<IDynamicVariable>? filter = null, bool excludeDisabled = false, bool includeLocal = false, Predicate<Slot>? slotFilter = null)
        {
            filter ??= Filter;

            return space.Slot.GetComponentsInChildren<IDynamicVariable>(
                variable => variable.IsLinkedToSpace(space) && filter(variable),
                excludeDisabled, includeLocal, slotFilter!);
        }

        /// <inheritdoc cref="GetLinkedVariables(DynamicVariableSpace, Predicate{IDynamicVariable}?, bool, bool, Predicate{Slot}?)"/>
        /// <typeparam name="T">The type of the values of the <see cref="IDynamicVariable{T}">dynamic variables</see>.</typeparam>
        public static IEnumerable<IDynamicVariable<T>> GetLinkedVariables<T>(this DynamicVariableSpace space,
            Predicate<IDynamicVariable<T>>? filter = null, bool excludeDisabled = false, bool includeLocal = false, Predicate<Slot>? slotFilter = null)
        {
            filter ??= Filter;

            return space.Slot.GetComponentsInChildren<IDynamicVariable<T>>(
                variable => variable.IsLinkedToSpace(space) && filter(variable),
                excludeDisabled, includeLocal, slotFilter!);
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

        /// <summary>
        /// Gets all <see cref="DynamicVariableIdentity">full Dynamic Variable identities</see>
        /// that are associated with this <see cref="DynamicVariableSpace"/>.
        /// </summary>
        /// <param name="space">The <see cref="DynamicVariableSpace"/> to get all the variable identities of.</param>
        /// <returns>All full Dynamic Variable identities associated with this <paramref name="space"/>.</returns>
        public static IEnumerable<DynamicVariableIdentity> GetVariableIdentities(this DynamicVariableSpace space)
        {
            var i = 0;
            var identities = new DynamicVariableIdentity[space._dynamicValues.Keys.Count];

            foreach (var identity in space._dynamicValues.Keys)
                identities[i++] = new(space, identity);

            return identities;
        }

        /// <summary>
        /// Gets all <see cref="DynamicVariableIdentity">full Dynamic Variable identities</see>
        /// that are associated with this <see cref="DynamicVariableSpace"/>,
        /// which can be assigned a value of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type that must be assignable to the variables.</typeparam>
        /// <param name="space">The <see cref="DynamicVariableSpace"/> to get all the variable identities of.</param>
        /// <returns>All full Dynamic Variable identities associated with this <paramref name="space"/> that <typeparamref name="T"/> can be assigned to.</returns>
        public static IEnumerable<DynamicVariableIdentity> GetVariableIdentities<T>(this DynamicVariableSpace space)
            => space.GetVariableIdentities(typeof(T));

        /// <summary>
        /// Gets all <see cref="DynamicVariableIdentity">full Dynamic Variable identities</see>
        /// that are associated with this <see cref="DynamicVariableSpace"/>,
        /// which can be assigned a value of the given <see cref="Type"/>.
        /// </summary>
        /// <param name="space">The <see cref="DynamicVariableSpace"/> to get all the variable identities of.</param>
        /// <param name="type">The type that must be assignable to the variables.</param>
        /// <returns>All full Dynamic Variable identities associated with this <paramref name="space"/> that the given <see cref="Type"/> can be assigned to.</returns>
        public static IEnumerable<DynamicVariableIdentity> GetVariableIdentities(this DynamicVariableSpace space, Type type)
            => [.. space.GetVariableIdentities().Where(id => id.Type.IsAssignableFrom(type))];

        /// <summary>
        /// Gets all <see cref="DynamicVariableIdentity">full Dynamic Variable identities</see>
        /// that are associated with this <see cref="DynamicVariableSpace"/>,
        /// which match the given <paramref name="name"/>.
        /// </summary>
        /// <param name="space">The <see cref="DynamicVariableSpace"/> to get all the variable identities of.</param>
        /// <param name="name">The name that the variable identities must have.</param>
        /// <returns>All full Dynamic Variable identities associated with this <paramref name="space"/> that have the given <paramref name="name"/>.</returns>
        public static IEnumerable<DynamicVariableIdentity> GetVariableIdentities(this DynamicVariableSpace space, string name)
        {
            name.ParseAsPath(out _, out var variableName);

            return [.. space.GetVariableIdentities().Where(id => id.Name == variableName)];
        }

        /// <summary>
        /// Gets a <see cref="DynamicVariableIdentity">full Dynamic Variable identity</see>
        /// that is associated with this <see cref="DynamicVariableSpace"/> and
        /// has the given <paramref name="type"/> and <paramref name="name"/>.
        /// </summary>
        /// <remarks>
        /// <b>Note:</b> The Dynamic Variable identity created by this method
        /// does not necessarily <i>exist</i> on this <see cref="DynamicVariableSpace"/>.
        /// </remarks>
        /// <param name="space">The <see cref="DynamicVariableSpace"/> that the Dynamic Variable identity is a part of.</param>
        /// <param name="type">The <see cref="Type"/> of the Dynamic Variable identity.</param>
        /// <param name="name">The name of the Dynamic Variable identity.</param>
        /// <returns>
        /// A full Dynamic Variable identity associated with this <paramref name="space"/>
        /// that has the given <paramref name="type"/> and <paramref name="name"/>.
        /// </returns>
        public static DynamicVariableIdentity GetVariableIdentity(this DynamicVariableSpace space, Type type, string name)
            => new(space, type, name);

        /// <summary>
        /// Gets the concrete type of the value of this dynamic variable.
        /// </summary>
        /// <remarks>
        /// This method assumes that this dynamic variable also implements <see cref="IDynamicVariable{T}"/>.<br/>
        /// This should really always be the case.
        /// </remarks>
        /// <param name="dynamicVariable">The dynamic variable to get the concrete type of the value of.</param>
        /// <returns>The concrete type of the value of this dynamic variable.</returns>
        public static Type GetVariableType(this IDynamicVariable dynamicVariable)
            => dynamicVariable.GetType().GetGenericArgumentsFromInterface(typeof(IDynamicVariable<>))[0];

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
        /// Determines whether this <see cref="IDynamicVariable">dynamic variable</see>
        /// is linked against the given <see cref="DynamicVariableSpace"/> and
        /// returns the <paramref name="space"/> that it is actually linked to.
        /// </summary>
        /// <param name="dynamicVariable">The dynamic variable for which to check the linked <see cref="DynamicVariableSpace"/>.</param>
        /// <param name="space">
        /// The dynamic variable space to compare to the linked one.<br/>
        /// Will have the real space that the variable is linked to afterwards.
        /// This may be <see langword="null"/> if the variable isn't linked to a space.
        /// </param>
        /// <returns>
        /// <c>true</c> if this <see cref="IDynamicVariable">dynamic variable</see>
        /// is linked to the given <see cref="DynamicVariableSpace"/>; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsLinkedToSpace(this IDynamicVariable dynamicVariable, [NotNullWhen(true), MaybeNullWhen(false)] ref DynamicVariableSpace space)
        {
            var checkSpace = space;
            return dynamicVariable.TryGetLinkedSpace(out space) && space == checkSpace;
        }

        /// <summary>
        /// Determines whether this dynamic variable identity is part of
        /// the <see cref="SharedConfigIdentifier">shared config</see>.
        /// </summary>
        /// <param name="identity">The dynamic variable identity to check.</param>
        /// <returns>
        /// <see langword="true"/> if this dynamic variable identity is part of
        /// the <see cref="SharedConfigIdentifier">shared config</see>; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsSharedConfigVariable(this DynamicVariableIdentity identity)
            => identity.Space.CurrentName is "World" && identity.Name.StartsWith(SharedConfigIdentifier);

        /// <summary>
        /// Determines whether this dynamic variable is part of
        /// the <see cref="SharedConfigIdentifier">shared config</see>.
        /// </summary>
        /// <param name="dynamicVariable">The dynamic variable to check.</param>
        /// <returns>
        /// <see langword="true"/> if this dynamic variable is part of
        /// the <see cref="SharedConfigIdentifier">shared config</see>; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsSharedConfigVariable(this IDynamicVariable dynamicVariable)
            => dynamicVariable.VariableName.StartsWith(SharedConfigVariableNamePrefix) && dynamicVariable.GetLinkedSpace().CurrentName is "World";

        /// <summary>
        /// Tests whether this string is a <see cref="DynamicVariableHelper.IsValidName">valid dynamic variable name</see>.
        /// </summary>
        /// <param name="variableName">The string to test.</param>
        /// <returns><c>true</c> if this string is a valid name; otherwise, <c>false</c>.</returns>
        public static bool IsValidName(this string? variableName)
            => DynamicVariableHelper.IsValidName(variableName!);

        /// <summary>
        /// Determines whether <typeparamref name="T"/> values can be assigned to this dynamic variable.
        /// </summary>
        /// <typeparam name="T">The type of the values that should be assigned to this dynamic variable.</typeparam>
        /// <param name="dynamicVariable">The dynamic variable whose concrete type of the value to check.</param>
        /// <returns><see langword="true"/> if <typeparamref name="T"/> values could be assigned to this dynamic variable; otherwise, <see langword="false"/>.</returns>
        public static bool IsVariableOfCompatibleType<T>(this IDynamicVariable dynamicVariable)
            => dynamicVariable.IsVariableOfCompatibleType(typeof(T));

        /// <summary>
        /// Determines whether values of the given type can be assigned to this dynamic variable.
        /// </summary>
        /// <param name="dynamicVariable">The dynamic variable whose concrete type of the value to check.</param>
        /// <param name="valueType">The type of the values that should be assigned to this dynamic variable.</param>
        /// <returns><see langword="true"/> if values of the given type could be assigned to this dynamic variable; otherwise, <see langword="false"/>.</returns>
        public static bool IsVariableOfCompatibleType(this IDynamicVariable dynamicVariable, Type valueType)
            => dynamicVariable.GetVariableType().IsAssignableFrom(valueType);

        /// <summary>
        /// Determines whether the concrete type of the value of this dynamic variable is <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to compare the concrete type of the value of this dynamic variable against.</typeparam>
        /// <param name="dynamicVariable">The dynamic variable whose concrete type of the value to check.</param>
        /// <returns><see langword="true"/> if the concrete type of the value of this dynamic variable is <typeparamref name="T"/>; otherwise, <see langword="false"/>.</returns>
        public static bool IsVariableOfType<T>(this IDynamicVariable dynamicVariable)
            => dynamicVariable is IDynamicVariable<T>;

        /// <summary>
        /// Determines whether the concrete type of the value of this dynamic variable is of the given type.
        /// </summary>
        /// <param name="dynamicVariable">The dynamic variable whose concrete type of the value to check.</param>
        /// <param name="variableType">The type to compare the concrete type of the value of this dynamic variable against.</param>
        /// <returns><see langword="true"/> if the concrete type of the value of this dynamic variable is the given type; otherwise, <see langword="false"/>.</returns>
        public static bool IsVariableOfType(this IDynamicVariable dynamicVariable, Type variableType)
            => dynamicVariable.GetVariableType() == variableType;

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
        /// Tries to get the <see cref="DynamicVariableHandler{T}"/> of this
        /// <see cref="IDynamicVariable{T}">dynamic variable</see>.
        /// </summary>
        /// <param name="dynamicVariable">The dynamic variable of which to get the <see cref="DynamicVariableHandler{T}"/>.</param>
        /// <param name="handler">
        /// The <see cref="DynamicVariableHandler{T}"/> of this <see cref="IDynamicVariable">dynamic variable</see>; otherwise, <c>null</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the <see cref="DynamicVariableHandler{T}"/> of this
        /// <see cref="IDynamicVariable">dynamic variable</see> was found; otherwise, <c>false</c>.
        /// </returns>
        public static bool TryGetHandler<T>(this IDynamicVariable<T> dynamicVariable, [NotNullWhen(true)] out DynamicVariableHandler<T>? handler)
        {
            handler = Traverse.Create(dynamicVariable)
                .Field(nameof(DynamicVariableBase<dummy>.handler))
                .GetValue<DynamicVariableHandler<T>>();

            return handler is not null;
        }

        /// <summary>
        /// Tries to get the full <see cref="IDynamicVariable{T}">Dynamic Variable</see> identity of this dynamic variable component.
        /// </summary>
        /// <remarks>
        /// This requires this dynamic variable component to be <see cref="GetLinkedSpace">linked</see> to a <see cref="DynamicVariableSpace"/>.
        /// </remarks>
        /// <param name="variable">The variable to get the full identity for.</param>
        /// <param name="identity">The resulting full identity if this variable is <see cref="GetLinkedSpace">linked</see> to a <see cref="DynamicVariableSpace"/>; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the full identity was successfully created; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetIdentity(this IDynamicVariable variable, [NotNullWhen(true)] out DynamicVariableIdentity? identity)
        {
            if (!variable.TryGetLinkedSpace(out var linkedSpace))
            {
                identity = null;
                return false;
            }

            identity = new(linkedSpace, variable.GetVariableType(), variable.Name);
            return true;
        }

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
        //public static DynamicField<T>? CreateVariable<T>(this IField<T> field, string name, bool overrideOnLink = false, bool persistent = true)
        //{
        //    var variable = field.FindNearestParent<Slot>().AttachComponent<DynamicField<T>>();
        //    variable.TargetField.Target = field;
        //    variable.VariableName.Value = name;
        //    variable.OverrideOnLink.Value = overrideOnLink;
        //    variable.Persistent = persistent;
        //    return variable;
        //}

        /// <summary>
        /// Tries to get the <see cref="DynamicVariableSpace"/> that this
        /// <see cref="IDynamicVariable">dynamic variable</see> is linked to.
        /// </summary>
        /// <param name="dynamicVariable">The dynamic variable of which to get the linked <see cref="DynamicVariableSpace"/>.</param>
        /// <param name="linkedSpace">
        /// The <see cref="DynamicVariableSpace"/> that this <see cref="IDynamicVariable">dynamic
        /// variable</see> is linked to was found; otherwise, <c>null</c>.
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

        /// <summary>
        /// Excludes all dynamic variables identities from this sequence that are
        /// <see cref="IsSharedConfigVariable(DynamicVariableIdentity)">part of the shared config</see>.
        /// </summary>
        /// <param name="identities">The sequence to remove the identities of <see cref="SharedConfigIdentifier">shared config</see> variables from.</param>
        /// <returns>The sequence without any identities of <see cref="SharedConfigIdentifier">shared config</see> variables.</returns>
        public static IEnumerable<DynamicVariableIdentity> WithoutSharedConfigVariables(this IEnumerable<DynamicVariableIdentity> identities)
            => identities.Where(static identity => !identity.IsSharedConfigVariable());

        /// <summary>
        /// Excludes all dynamic variables from this sequence that are
        /// <see cref="IsSharedConfigVariable(IDynamicVariable)">part of the shared config</see>.
        /// </summary>
        /// <param name="dynamicVariables">The sequence to remove <see cref="SharedConfigIdentifier">shared config</see> variables from.</param>
        /// <returns>The sequence without any <see cref="SharedConfigIdentifier">shared config</see> variables.</returns>
        public static IEnumerable<IDynamicVariable> WithoutSharedConfigVariables(this IEnumerable<IDynamicVariable> dynamicVariables)
            => dynamicVariables.Where(static variable => !variable.IsSharedConfigVariable());

        private static bool Filter<T>(T item) => true;

        private static T Id<T>(this T item) => item;
    }
}