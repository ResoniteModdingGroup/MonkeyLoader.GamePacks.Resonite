using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace MonkeyLoader.Unity
{
    internal sealed class SceneLoadingHook : Monkey<SceneLoadingHook>
    {
        public override string Name { get; } = "Scene Loading Hook";

        private static IEnumerable<IUnityMonkeyInternal> UnityMonkeys
            => Mod.Loader.Monkeys.SelectCastable<IMonkey, IUnityMonkeyInternal>();

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => Enumerable.Empty<IFeaturePatch>();

        protected override bool OnLoaded()
        {
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            return true;
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            Info(() => "First Scene Loaded! Calling OnFirstSceneReady on UnityMonkeys!");

            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;

            foreach (var unityMonkey in UnityMonkeys)
                unityMonkey.FirstSceneReady(scene);
        }
    }
}
