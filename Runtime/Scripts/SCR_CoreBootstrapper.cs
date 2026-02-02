#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

namespace Core
{
    using static CoreUtility;

#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class CoreBootstrapper
    {
        public const string SCENE_NAME = "SCN_Bootstrap";
        public static string SCENE_GUID = STRING_NULL;

#if UNITY_EDITOR
        private static SceneAsset sceneAsset = null;

        static CoreBootstrapper() => EditorApplication.playModeStateChanged += OnPlay;
        private static void OnPlay(PlayModeStateChange state)
        {
            if (sceneAsset == null)
            {
                string[] guids = AssetDatabase.FindAssets($"t:Scene {SCENE_NAME}");

                if (guids.Length <= 0)
                {
                    Debug.LogError($"CoreBootstrapper.OnPlay() Could not find scene: {SCENE_NAME}");
                    return;
                }

                sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }

            EditorSceneManager.playModeStartScene = sceneAsset;
        }
#endif
    }
}