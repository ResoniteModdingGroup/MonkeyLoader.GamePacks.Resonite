using FrooxEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite
{
    public static class DynamicVariableExtensions
    {
        public static DynamicReference<T> CreateReferenceVariable<T>(this SyncRef<T> syncRef, string name, bool overrideOnLink = false, bool persistent = true)
            where T : class, IWorldElement
        {
            var variable = syncRef.FindNearestParent<Slot>().AttachComponent<DynamicReference<T>>();
            variable.TargetReference.Target = syncRef;
            variable.VariableName.Value = name;
            variable.OverrideOnLink.Value = overrideOnLink;
            variable.Persistent = persistent;

            return variable;
        }

        public static DynamicVariableSpace CreateSpace(this Slot slot, string? spaceName, bool onlyDirectBinding = false)
        {
            var space = slot.AttachComponent<DynamicVariableSpace>();
            space.SpaceName.Value = spaceName;
            space.OnlyDirectBinding.Value = onlyDirectBinding;

            return space;
        }

        public static DynamicField<T>? CreateVariable<T>(this IField<T> field, string name, bool overrideOnLink = false, bool persistent = true)
        {
            var variable = field.FindNearestParent<Slot>().AttachComponent<DynamicField<T>>();
            variable.TargetField.Target = field;
            variable.VariableName.Value = name;
            variable.OverrideOnLink.Value = overrideOnLink;
            variable.Persistent = persistent;

            return variable;
        }

        public static DynamicReferenceVariableDriver<T> DriveReferenceFromVariable<T>(this SyncRef<T> syncRef, string name, T? defaultTarget = default, bool persistent = true)
            where T : class, IWorldElement
        {
            var driver = syncRef.FindNearestParent<Slot>().AttachComponent<DynamicReferenceVariableDriver<T>>();
            driver.Target.Target = syncRef;
            driver.VariableName.Value = name;
            driver.DefaultTarget.Target = defaultTarget!;
            driver.Persistent = persistent;

            return driver;
        }

        public static DynamicValueVariableDriver<T> DriveValueFromVariable<T>(this IField<T> field, string name, T? defaultValue = default, bool persistent = true)
        {
            var driver = field.FindNearestParent<Slot>().AttachComponent<DynamicValueVariableDriver<T>>();
            driver.Target.Target = field;
            driver.VariableName.Value = name;
            driver.DefaultValue.Value = defaultValue!;
            driver.Persistent = persistent;

            return driver;
        }

        public static DynamicVariableSpace FindOrCreateSpace(this Slot slot, string? spaceName, bool onlyDirectBinding = false)
            => slot.FindSpace(spaceName) ?? CreateSpace(slot, spaceName, onlyDirectBinding);

        public static bool IsValidName(this string? variableName)
            => DynamicVariableHelper.IsValidName(variableName);

        public static void ParseAsPath(this string? path, out string? spaceName, out string? variableName)
            => DynamicVariableHelper.ParsePath(path, out spaceName, out variableName);

        public static string? ProcessName(this string? variableName)
            => DynamicVariableHelper.ProcessName(variableName);
    }
}