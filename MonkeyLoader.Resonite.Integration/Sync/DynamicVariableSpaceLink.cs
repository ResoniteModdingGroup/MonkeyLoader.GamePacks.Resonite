using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Sync;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MonkeyLoader.Resonite.Sync
{
    /// <summary>
    /// Hooks <see cref="DynamicVariableSpace"/>s to create the
    /// <see cref="DynamicVariableSpaceSyncObject{TSyncObject}">MonkeySync objects
    /// linked through them</see> when they have the right name.
    /// </summary>
    [HarmonyPatchCategory(nameof(DynamicVariableSpaceLink))]
    [HarmonyPatch(typeof(DynamicVariableSpace), nameof(DynamicVariableSpace.UpdateName))]
    public sealed class DynamicVariableSpaceLink : ResoniteMonkey<DynamicVariableSpaceLink>
    {
        /// <summary>
        /// The prefix SyncObject <see cref="DynamicVariableSpace"/>s use
        /// in their <see cref="DynamicVariableSpace.SpaceName">SpaceName</see>s.
        /// </summary>
        public const string SpaceNamePrefix = "MonkeySyncObject.";

        public static DynamicReferenceVariable<T> CreateLinkedReferenceVariable<T>(Slot slot, string variableName, IMonkeySyncValue<T> syncValue)
            where T : class, IWorldElement
        {
            var referenceVariable = slot.AttachComponent<DynamicReferenceVariable<T>>();
            referenceVariable.VariableName.Value = variableName;
            referenceVariable.Reference.Target = syncValue.Value;

            CreateReferenceLink(referenceVariable, syncValue);

            return referenceVariable;
        }

        public static DynamicValueVariable<T> CreateLinkedValueVariable<T>(Slot slot, string variableName, IMonkeySyncValue<T> syncValue)
        {
            var valueVariable = slot.AttachComponent<DynamicValueVariable<T>>();
            valueVariable.VariableName.Value = variableName;
            valueVariable.Value.Value = syncValue.Value;

            CreateValueLink(valueVariable, syncValue);

            return valueVariable;
        }

        public static void CreateReferenceLink<T>(DynamicReferenceVariable<T> referenceVariable, IMonkeySyncValue<T> syncValue)
            where T : class, IWorldElement
        {
            referenceVariable.Reference.OnTargetChange += field => syncValue.Value = field.Target;

            syncValue.Changed += (sender, args) => referenceVariable.RunSynchronously(() => referenceVariable.Reference.Target = args.NewValue!);
        }

        public static void CreateValueLink<T>(DynamicValueVariable<T> valueVariable, IMonkeySyncValue<T> syncValue)
        {
            valueVariable.Value.OnValueChange += field => syncValue.Value = field.Value;

            syncValue.Changed += (sender, args) => valueVariable.RunSynchronously(() => valueVariable.Value.Value = args.NewValue!);
        }

        private static void Prefix(DynamicVariableSpace __instance)
        {
            var name = DynamicVariableHelper.ProcessName(__instance.SpaceName);

            if ((__instance._lastNameSet && name == __instance._lastName) || !(name?.StartsWith(SpaceNamePrefix) ?? false) || MonkeySyncRegistry.HasLinkedSyncObject(__instance))
                return;

            __instance.RunInUpdates(32, () => TryCreateSyncObject(__instance, name, out _));
        }

        private static bool TryCreateSyncObject(DynamicVariableSpace space, string name, [NotNullWhen(true)] out ILinkedMonkeySyncObject<DynamicVariableSpace>? syncObject)
        {
            syncObject = null;

            if (space.FilterWorldElement() is null || space.SpaceName.Value != name)
                return false;

            var syncObjectName = name[SpaceNamePrefix.Length..];

            if (!MonkeySyncRegistry.TryGetSyncObjectRegistration<DynamicVariableSpace>(syncObjectName, out var registration))
                return false;

            var createdSyncObject = registration.CreateSyncObject();

            if (!createdSyncObject.LinkWith(space, true))
            {
                createdSyncObject.Dispose();
                return false;
            }

            syncObject = createdSyncObject;

            return true;
        }
    }
}