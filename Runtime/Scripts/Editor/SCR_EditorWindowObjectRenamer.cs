using UnityEngine;
using UnityEditor;

namespace Core.Editor
{
    using static CoreUtility;

    public class EditorWindowObjectRenamer : EditorWindow
    {
        private string prefix = "";
        private string suffix = "";
        private string removeBefore = "";
        private string removeAfter = "";
        private string previewName = "";
        private bool sequenceNumbering = false;

        [MenuItem("Tools/Object Renamer")]
        public static void ShowWindow() => GetWindow<EditorWindowObjectRenamer>("Object Renamer");

        private void OnGUI()
        {
            GUILayout.Label("Apply Name to Selected Assets or GameObjects", EditorStyles.boldLabel);

            prefix = EditorGUILayout.TextField("Prefix", prefix);
            suffix = EditorGUILayout.TextField("Suffix", suffix);
            removeBefore = EditorGUILayout.TextField("Remove Before", removeBefore);
            removeAfter = EditorGUILayout.TextField("Remove After", removeAfter);
            sequenceNumbering = EditorGUILayout.Toggle("Sequence Numbering", sequenceNumbering);

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

            previewName = ComputeName(Selection.objects[0].name);
        }
        
        private void Rename()
        {
            var selectedObjects = Selection.objects;

            if (selectedObjects.Length == 0)
            {
                Debug.LogWarning("No objects selected!");
                return;
            }

            for (int i = 0; i < selectedObjects.Length; i++)
            {
                Object obj = selectedObjects[i];

                string name = ComputeName(obj.name);
                string path = AssetDatabase.GetAssetPath(obj);

                if (sequenceNumbering)
                {
                    name = $"{name}_{i + 1:00}";
                }

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

            if (!string.IsNullOrEmpty(suffix))
            {
                name += suffix;
            }

            return string.IsNullOrEmpty(name) ? original : name;
        }
    }
}
