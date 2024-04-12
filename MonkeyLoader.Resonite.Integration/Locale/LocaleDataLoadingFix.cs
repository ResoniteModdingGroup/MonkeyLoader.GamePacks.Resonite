using Elements.Assets;
using HarmonyLib;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.Locale
{
    [HarmonyPatch(typeof(LocaleResource), nameof(LocaleResource.LoadDataAdditively), new[] { typeof(LocaleData) })]
    internal sealed class LocaleDataLoadingFix : ResoniteMonkey<LocaleDataLoadingFix>
    {
        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => Enumerable.Empty<IFeaturePatch>();

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