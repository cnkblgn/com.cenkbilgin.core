using UnityEditor;
using UnityEngine;

namespace Core
{
    internal static class RegistryLoader
    {
        private static Registry[] registries;

#if UNITY_EDITOR
        private static bool TryCache()
        {
            string folder = "Assets";
            string filter = "t:" + typeof(Registry).Name;
            UnityEditor.GUID[] guids = UnityEditor.AssetDatabase.FindAssetGUIDs(filter, new[] { folder });

            if (guids.Length == 0)
            {
                return false;
            }

            bool found = false;
            registries = new Registry[guids.Length];

            for (int i = 0; i < guids.Length; i++)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
                Registry asset = UnityEditor.AssetDatabase.LoadAssetAtPath<Registry>(path);

                if (asset != null)
                {
                    registries[i] = asset;
                    found = true;
                }
            }

            return found;
        }

        [MenuItem("Tools/Reload Registries", priority = -10)]
        private static void TryReload()
        {
            TryCache();

            for (int i = 0; i < registries.Length; i++)
            {
                registries[i].OnAfterScriptLoad();
            }

            Debug.Log("Registries reloaded!");
        }

        [UnityEditor.Callbacks.DidReloadScripts(1)]
        private static void OnAfterScriptLoad()
        {
            if (!TryCache())
            {
                return;
            }

            for (int i = 0; i < registries.Length; i++)
            {
                registries[i].OnAfterScriptLoad();
            }
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnBeforeSceneLoad()
        {
            Registry[] registries = Resources.LoadAll<Registry>("Registry");

            for (int i = 0; i < registries.Length; i++)
            {
                registries[i].OnBeforeSceneLoad();
            }

#if UNITY_EDITOR
            if (registries.Length == 0)
            {
                Debug.LogWarning($"Registry resources not found at path [{"Assets/Resources/Registry"}]. Ignore if its intented");
            }
#endif
        }
    }
}