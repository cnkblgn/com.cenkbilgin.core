using UnityEngine;
using UnityEditor;

namespace Core.Editor
{
    using static CoreUtility;

    public class EditorWindowRenamer : EditorWindow
    {
        private string prefix = "PFB_";

        [MenuItem("Tools/Prefix Renamer")]
        public static void ShowWindow() => GetWindow<EditorWindowRenamer>("Prefix Renamer");

        private void OnGUI()
        {
            GUILayout.Label("Add Prefix to Selected Assets or GameObjects", EditorStyles.boldLabel);

            prefix = EditorGUILayout.TextField("Prefix", prefix);

            if (GUILayout.Button("Rename Selected"))
            {
                RenameSelected();
            }
        }

        private void RenameSelected()
        {
            // Asset ve GameObjectleri al
            var selectedObjects = Selection.objects;

            if (selectedObjects.Length == 0)
            {
                LogWarning("No objects selected!");
                return;
            }

            foreach (var obj in selectedObjects)
            {
                string newName = prefix + obj.name.Replace(" ", "");

                string path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path))
                {
                    AssetDatabase.RenameAsset(path, newName);
                }
                else
                {
                    obj.name = newName;
                }
            }

            AssetDatabase.SaveAssets();
            LogWarning("Selected objects renamed!");
        }
    }
}
