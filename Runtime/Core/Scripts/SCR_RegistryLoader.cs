using UnityEngine;

namespace Core
{
    using static CoreUtility;

    internal static class RegistryLoader
    {
        private static Registry[] registries;

#if UNITY_EDITOR
        private static bool TryCache() => TryFindAssetsByType(out registries);

        [UnityEditor.Callbacks.DidReloadScripts(1)]
        private static void OnAssemblyReload()
        {
            if (TryCache())
            {
                for (int i = 0; i < registries.Length; i++)
                {
                    registries[i].OnAssemblyReload();
                }

                Debug.Log("Registry assemblies are reloaded!");
            }
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnBeforeSceneLoad()
        {
            if (registries == null)
            {
#if UNITY_EDITOR
                TryCache();
#else
                return;
#endif
            }

            for (int i = 0; i < registries.Length; i++)
            {
                registries[i].OnBeforeSceneLoad();
            }
        }
    }
}