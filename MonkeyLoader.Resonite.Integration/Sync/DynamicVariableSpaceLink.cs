using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Sync;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MonkeyLoader.Resonite.Sync
{
    [HarmonyPatchCategory(nameof(DynamicVariableSpaceLink))]
    [HarmonyPatch(typeof(DynamicVariableSpace), nameof(DynamicVariableSpace.UpdateName))]
    public sealed class DynamicVariableSpaceLink : ResoniteMonkey<DynamicVariableSpaceLink>
    {
        /// <summary>
        /// The prefix SyncObject <see cref="DynamicVariableSpace"/>s use
        /// in their <see cref="DynamicVariableSpace.SpaceName">SpaceName</see>s.
        /// </summary>
        public const string SpaceNamePrefix = "MonkeySyncObject.";

        private static void Prefix(DynamicVariableSpace __instance)
        {
            var name = DynamicVariableHelper.ProcessName(__instance.SpaceName);

            if ((__instance._lastNameSet && name == __instance._lastName) || !(name?.StartsWith(SpaceNamePrefix) ?? false))
                return;

            __instance.RunInUpdates(32, () => TryCreateSyncObject(__instance, name, out _));
        }

        private static bool TryCreateSyncObject(DynamicVariableSpace space, string name, [NotNullWhen(true)] out ILinkedMonkeySyncObject<DynamicVariableSpace>? syncObject)
        {
            syncObject = null;

            if (space.FilterWorldElement() is null || space.SpaceName.Value != name)
                return false;

            var syncObjectName = name[SpaceNamePrefix.Length..];

            if (!MonkeySyncRegistry.TryGetRegisteredSyncObject<DynamicVariableSpace>(syncObjectName, out var registeredSyncObject))
                return false;

            var createdSyncObject = registeredSyncObject.CreateSyncObject();

            if (!createdSyncObject.LinkWith(space, true))
                return false;

            syncObject = createdSyncObject;

            return true;
        }
    }
}