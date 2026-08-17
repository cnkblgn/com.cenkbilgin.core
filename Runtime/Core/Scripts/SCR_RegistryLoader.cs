using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core
{
    public static class RegistryLoader
    {
        public const string FOLDER = "Registry";
        public const string PATH = "Assets/Resources/" + FOLDER;

        private static Registry[] registries;

#if UNITY_EDITOR
        private static bool TryCacheEditor()
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

            registries = registries.OfType<Registry>().OrderBy(x => x.Priority).ToArray();
            return found;
        }

        [UnityEditor.MenuItem("Tools/Reload Registries", priority = -10)]
        private static void TryReloadEditor()
        {
            TryCacheEditor();

            for (int i = 0; i < registries.Length; i++)
            {
                registries[i].OnAfterScriptLoad();
            }

            Debug.Log("Registries reloaded!");
        }

        [UnityEditor.Callbacks.DidReloadScripts(1)]
        private static void OnAfterScriptLoad()
        {
            if (!TryCacheEditor())
            {
                return;
            }

            for (int i = 0; i < registries.Length; i++)
            {
                registries[i].OnAfterScriptLoad();
            }
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void AfterAssembliesLoaded()
        {
            registries = Resources.LoadAll<Registry>(FOLDER).OrderBy(x => x.Priority).ToArray();

            for (int i = 0; i < registries.Length; i++)
            {
                registries[i].OnAfterAssembliesLoaded();
            }

#if UNITY_EDITOR
            if (registries.Length == 0)
            {
                Debug.LogWarning($"Registry resources not found at path [{PATH}]. Ignore if its intented");
            }
#endif
        }

        public static bool TryGetRegistry<T>(out T registry) where T : Registry
        {
            registry = null;

            if (registries == null)
            {
                return false;
            }

            for (int i = 0; i < registries.Length; i++)
            {
                if (registries[i] is T typedRegistry)
                {
                    registry = typedRegistry;
                    return true;
                }
            }

            return false;
        }
        public static IReadOnlyList<Registry> GetRegistries() => registries;
    }
}