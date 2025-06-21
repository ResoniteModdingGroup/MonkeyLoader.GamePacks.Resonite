using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EngineLocaleHelper = FrooxEngine.LocaleHelper;

namespace MonkeyLoader.Resonite.Locale
{
    [HarmonyPatch(typeof(EngineLocaleHelper))]
    [HarmonyPatchCategory(nameof(ModLocaleFallbackDriver))]
    internal sealed class ModLocaleFallbackDriver : ResoniteMonkey<ModLocaleFallbackDriver>
    {
        private static void AddFallbackMessage(IField<string> field, LocaleStringDriver localeDriver, string notFoundMessage, string fallbackMessage)
        {
            var booleanDriver = localeDriver.Slot.AttachComponent<BooleanValueDriver<string>>();
            localeDriver.Target.Target = booleanDriver.FalseValue;
            booleanDriver.TrueValue.Value = fallbackMessage;
            booleanDriver.TargetField.Target = field;

            var equalityDriver = localeDriver.Slot.AttachComponent<ValueEqualityDriver<string>>();
            equalityDriver.Reference.Value = notFoundMessage;
            equalityDriver.TargetValue.Target = booleanDriver.FalseValue;
            equalityDriver.Target.Target = booleanDriver.State;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(EngineLocaleHelper.DriveLocalized), [typeof(IField<string>), typeof(string), typeof(string), typeof(Dictionary<string, object>)])]
        private static void DriveLocalizedPostfix(IField<string> field, string key, string? format, Dictionary<string, object>? arguments, LocaleStringDriver __result)
        {
            if (arguments is not null && arguments.ContainsKey(LocaleExtensions.ModLocaleStringIndicatorArgumentName))
                AddFallbackMessage(field, __result, string.Format(format ?? "{0}", key), key.AsLocaleKey(format, arguments: arguments).FormatWithFallback()!);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(EngineLocaleHelper.DriveLocalized), [typeof(IField<string>), typeof(string), typeof(string), typeof((string, object)[])])]
        private static void DriveLocalizedPostfix(IField<string> field, string key, string? format, (string, object)[]? args, LocaleStringDriver __result)
        {
            if (args is not null && args.Any(arg => arg.Item1 == LocaleExtensions.ModLocaleStringIndicatorArgumentName))
                AddFallbackMessage(field, __result, string.Format(format ?? "{0}", key), key.AsLocaleKey(format, args).FormatWithFallback()!);
        }
    }
}