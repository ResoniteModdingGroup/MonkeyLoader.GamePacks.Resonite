using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Zio;

namespace MonkeyLoader.Resonite
{
    [HarmonyPatchCategory(nameof(LocaleDataInjector))]
    [HarmonyPatch(typeof(LocaleResource), nameof(LocaleResource.LoadTargetVariant))]
    internal class LocaleDataInjector : Monkey<LocaleDataInjector>
    {
        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => Enumerable.Empty<IFeaturePatch>();

        [HarmonyTranspiler]
        [HarmonyPatch(MethodType.Async)]
        private static IEnumerable<CodeInstruction> LoadTargetVariantMoveNextTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase targetMethod)
        {
            var onLoadStateChangeMethod = AccessTools.Method(typeof(Asset), nameof(Asset.OnLoadStateChanged));
            //var variantField = AccessTools.Field(targetMethod.DeclaringType, "variant");

            //var injectLocaleLoadingMethod = AccessTools.Method(typeof(LocaleDataInjector), nameof(InjectLocaleLoading));

            //var instructionList = new List<CodeInstruction>(instructions);

            /*
             * Instructions at this point - bool flag for whether to call on stack and target being added
             *
             X   IL_0233: ldloc.1
             X   IL_0234: call instance class [Elements.Assets]Elements.Assets.LocaleResource FrooxEngine.LocaleResource::get_Data()
             X   IL_0239: ldnull
             X   IL_023a: cgt.un
             *   IL_023c: ldloc.1
             *   IL_023d: ldarg.0
             *   IL_023e: ldfld class [Elements.Assets]Elements.Assets.LocaleResource FrooxEngine.LocaleResource/'<LoadTargetVariant>d__6'::'<resource>5__2'
             *   IL_0243: call instance void FrooxEngine.LocaleResource::set_Data(class [Elements.Assets]Elements.Assets.LocaleResource)
             X   IL_0248: brfalse.s IL_0250
             X   IL_024a: ldloc.1
             X   IL_024b: callvirt instance void FrooxEngine.Asset::OnLoadStateChanged()
             */

            // Only once, but at the end, so start there
            //var onLoadStateChangeIndex = instructionList.FindLastIndex(instruction => instruction.Calls(onLoadStateChangeMethod));

            //instructionList[onLoadStateChangeIndex] = new CodeInstruction(OpCodes.Call, injectLocaleLoadingMethod);
            //instructionList.InsertRange(onLoadStateChangeIndex, new[] { new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldfld, variantField) });
            //instructionList.RemoveAt(onLoadStateChangeIndex - 2);

            //instructionList.RemoveRange(onLoadStateChangeIndex - 10, 4);
            //instructionList.RemoveRange(onLoadStateChangeIndex - 2, 3); // 1, 2 works, as is fails

            foreach (var instruction in instructions)
            {
                if (instruction.Calls(onLoadStateChangeMethod))
                    yield return new CodeInstruction(OpCodes.Nop);
                else
                    yield return instruction;
            }
        }

        [HarmonyPostfix]
        private static async Task LoadTargetVariantPostfixAsync(Task __result, LocaleResource __instance, LocaleVariantDescriptor variant, bool __state)
        {
            await __result;

            var localeCodes = new List<string>();
            localeCodes.AddUnique(variant.LocaleCode);
            localeCodes.AddUnique(Elements.Assets.LocaleResource.GetMainLanguage(variant.LocaleCode));
            localeCodes.AddUnique("en");

            foreach (var localeCode in localeCodes)
            {
                var searchPath = (new UPath("Locale") / $"{localeCode}.json").ToRelative().ToString();

                foreach (var mod in Mod.Loader.Mods)
                {
                    var localeFilePaths = mod.ContentPaths.Where(path => path.ToString().EndsWith(searchPath, StringComparison.OrdinalIgnoreCase)).ToArray();

                    foreach (var localeFilePath in localeFilePaths)
                    {
                        try
                        {
                            using var localeFileStream = mod.FileSystem.OpenFile(localeFilePath, FileMode.Open, FileAccess.Read);

                            var localeData = await JsonSerializer.DeserializeAsync<Elements.Assets.LocaleData>(localeFileStream);

                            __instance.Data.LoadDataAdditively(localeData);
                        }
                        catch (Exception ex)
                        {
                            Warn(() => ex.Format($"Failed to deserialize file as LocaleData: {localeFilePath}"));
                        }
                    }
                }
            }

            if (__state)
                __instance.OnLoadStateChanged();
        }

        [HarmonyPrefix]
        private static void LoadTargetVariantPrefix(LocaleResource __instance, ref bool __state)
        {
            __state = __instance.Data != null;
        }
    }
}