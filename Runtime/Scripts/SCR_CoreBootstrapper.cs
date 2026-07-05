#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

namespace Core
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    internal static class CoreBootstrapper
    {
        public const string SCENE_NAME = "SCN_Bootstrap";

#if UNITY_EDITOR
        private static string sceneGuid = null;

        static CoreBootstrapper() => EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode)
            {
                return;
            }

            if (TryGetBootstrapScene(out SceneAsset scene))
            {
                EditorSceneManager.playModeStartScene = scene;
            }          
        }

        private static bool TryGetBootstrapScene(out SceneAsset asset)
        {
            asset = null;

            if (string.IsNullOrEmpty(sceneGuid))
            {
                string[] guids = AssetDatabase.FindAssets($"t:Scene {SCENE_NAME}");

                if (guids.Length == 0)
                {
                    Debug.LogError($"Could not find scene '{SCENE_NAME}'.");
                    return false;
                }

                sceneGuid = guids[0];
            }

            string path = AssetDatabase.GUIDToAssetPath(sceneGuid);

            if (string.IsNullOrEmpty(path))
            {
                sceneGuid = null;

                Debug.LogError($"Bootstrap scene '{SCENE_NAME}' is no longer valid.");
                return false;
            }

            SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);

            if (scene == null)
            {
                sceneGuid = null;

                Debug.LogError($"Failed to load bootstrap scene '{SCENE_NAME}'.");
                return false;
            }

            return scene;
        }
#endif
    }
}