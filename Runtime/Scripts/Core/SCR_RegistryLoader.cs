using UnityEngine;

namespace Core
{
    using static CoreUtility;

    internal static class RegistryLoader
    {
        private static Registry[] registries;

#if UNITY_EDITOR
        private static bool TryCache() => TryFindAssetsByType(out registries);

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnReload()
        {
            if (TryCache())
            {
                for (int i = 0; i < registries.Length; i++)
                {
                    registries[i].OnInitialize();
                }

                Debug.Log("Registries are reloaded!");
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnLoad()
        {
            if (registries == null)
            {
                TryCache();
            }

            for (int i = 0; i < registries.Length; i++)
            {
                registries[i].OnAwake();
            }

            Debug.Log("Registries are loaded!");
        }
#endif
    }
}