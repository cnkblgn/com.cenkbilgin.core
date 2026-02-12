using UnityEngine;
using UnityEditor;

namespace Core.Editor
{
    using static CoreUtility;

    public class EditorWindowObjectRenamer : EditorWindow
    {
        private string prefix = "PFB_";
        private string removeBefore = "";
        private string removeAfter = "";
        private string previewName = "";

        [MenuItem("Tools/Object Renamer")]
        public static void ShowWindow() => GetWindow<EditorWindowObjectRenamer>("Object Renamer");

        private void OnGUI()
        {
            GUILayout.Label("Apply Name to Selected Assets or GameObjects", EditorStyles.boldLabel);

            prefix = EditorGUILayout.TextField("Prefix", prefix);
            removeBefore = EditorGUILayout.TextField("Remove Before", removeBefore);
            removeAfter = EditorGUILayout.TextField("Remove After", removeAfter);

            UpdatePreview();

            EditorGUILayout.LabelField("Preview", previewName, EditorStyles.helpBox);

            if (GUILayout.Button("Rename Selected"))
            {
                Rename();
            }
        }

        private void UpdatePreview()
        {
            if (Selection.objects.Length == 0)
            {
                previewName = "(No selection)";
                return;
            }

            string name = Selection.objects[0].name;
            previewName = ComputeName(name);
        }
        
        private void Rename()
        {
            var selectedObjects = Selection.objects;

            if (selectedObjects.Length == 0)
            {
                Debug.LogWarning("No objects selected!");
                return;
            }

            foreach (var obj in selectedObjects)
            {
                string name = ComputeName(obj.name);
                string path = AssetDatabase.GetAssetPath(obj);

                if (!string.IsNullOrEmpty(path))
                {
                    AssetDatabase.RenameAsset(path, name);
                }
                else
                {
                    obj.name = name;
                }
            }

            AssetDatabase.SaveAssets();
            Debug.LogWarning("Selected objects renamed!");
        }
        private string ComputeName(string original)
        {
            string name = original;

            if (!string.IsNullOrEmpty(removeBefore))
            {
                int index = name.IndexOf(removeBefore);

                if (index >= 0)
                {
                    name = name[(index + removeBefore.Length)..];
                }
            }

            if (!string.IsNullOrEmpty(removeAfter))
            {
                int index = name.IndexOf(removeAfter);

                if (index >= 0)
                {
                    name = name[..index];
                }
            }

            name = name.Replace(" ", "");

            if (!string.IsNullOrEmpty(prefix))
            {
                name = prefix + name;
            }

            return string.IsNullOrEmpty(name) ? original : name;
        }
    }
}
