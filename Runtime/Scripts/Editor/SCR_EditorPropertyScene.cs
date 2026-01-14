using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Core;

namespace Core.Editor
{
    using static CoreUtility;
    using static EditorUtility;

    [CustomPropertyDrawer(typeof(Scene))]
    public class EditorPropertyScene : PropertyDrawer
    {
        private SerializedProperty scenePath = null;
        private SerializedProperty sceneName = null;
        private SerializedProperty sceneID = null;
        private SceneAsset sceneAsset = null;
        private Object sceneObject = null;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            scenePath = property.FindPropertyRelative("Path");
            sceneName = property.FindPropertyRelative("Name");
            sceneID = property.FindPropertyRelative("Index");

            sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath.stringValue);

            EditorGUI.BeginChangeCheck();

            string labelName = property.name;
            labelName = labelName[0].ToString().ToUpper() + labelName[1..];
            sceneObject = EditorGUI.ObjectField(position, labelName, sceneAsset, typeof(SceneAsset), false) as SceneAsset;

            if (EditorGUI.EndChangeCheck())
            {
                if (sceneObject == null)
                {
                    scenePath.stringValue = "";
                    sceneName.stringValue = "";
                    sceneID.intValue = -1;
                    return;
                }

                TryAdd(AssetDatabase.GetAssetPath(sceneObject));

                string path = AssetDatabase.GetAssetPath(sceneObject);
                scenePath.stringValue = path;
                sceneName.stringValue = AssetDatabase.LoadAssetAtPath<Object>(path).name;
                sceneID.intValue = SceneUtility.GetBuildIndexByScenePath(path);
                property.serializedObject.ApplyModifiedProperties();
            }
        }
        private void TryAdd(string scenePath)
        {
            int index = SceneUtility.GetBuildIndexByScenePath(scenePath);

            if (index >= 0)
            {
                return;
            }

            EditorBuildSettingsScene[] currentScenes = EditorBuildSettings.scenes;
            EditorBuildSettingsScene[] editedScenes = new EditorBuildSettingsScene[currentScenes.Length + 1];

            System.Array.Copy(currentScenes, editedScenes, currentScenes.Length);

            EditorBuildSettingsScene newScene = new(scenePath, true);
            editedScenes[^1] = newScene;

            EditorBuildSettings.scenes = editedScenes;
        }
    }
}