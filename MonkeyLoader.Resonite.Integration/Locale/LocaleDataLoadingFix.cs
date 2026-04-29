using Elements.Assets;
using EnumerableToolkit;
using HarmonyLib;
using System.Globalization;

namespace MonkeyLoader.Resonite.Locale
{
    [HarmonyPatchCategory(nameof(LocaleDataLoadingFix))]
    [HarmonyPatch(typeof(LocaleResource), nameof(LocaleResource.LoadDataAdditively), [typeof(LocaleData)])]
    internal sealed class LocaleDataLoadingFix : ResoniteMonkey<LocaleDataLoadingFix>
    {
        public override Sequence<string> SubgroupPath => SubgroupDefinitions.Locale;

        [HarmonyPostfix]
        private static void LoadDataAdditivelyPostfix(LocaleResource __instance)
        {
            foreach (var author in __instance._authors)
            {
                var localeCodes = author.Value.Distinct().ToArray();

                author.Value.Clear();
                author.Value.AddRange(localeCodes);
            }
        }

        [HarmonyPrefix]
        private static void LoadDataAdditivelyPrefix(LocaleData data)
        {
            var localeCode = data.LocaleCode;
            var index = -1;

            do
            {
                try
                {
                    if (CultureInfo.GetCultureInfo(localeCode) is not null)
                    {
                        data.LocaleCode = localeCode;
                        break;
                    }
                }
                catch (Exception)
                {
                    index = localeCode.LastIndexOf('-');
                }
            }
            while (index > 0 && (localeCode = localeCode.Remove(index)).Length > 0);
        }
    }
}