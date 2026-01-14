using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
    using static CoreUtility;

    public class EditorWindowMaterialCreator : EditorWindow
    {
        [SerializeField] private Shader shader;

        private SerializedProperty propertyShader = null;
        private SerializedObject serializedObject = null;

        private void OnEnable() { serializedObject = new SerializedObject(this); propertyShader = serializedObject.FindProperty("shader"); }
        [MenuItem("Tools/Material Creator")] public static void ShowTool() => GetWindow<EditorWindowMaterialCreator>("Material Creator");
        private void OnGUI()
        {
            serializedObject.Update();

            using (EditorGUILayout.VerticalScope vertical = new("helpbox", new GUILayoutOption[] { }))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(propertyShader, new GUIContent(""));
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Create Material", new GUIStyle(EditorStyles.miniButton) { alignment = TextAnchor.MiddleCenter, richText = true, fontStyle = FontStyle.Bold }))
                    {
                        Create();
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
        private void Create()
        {
            if (shader == null)
            {
                LogError("EditorWindowMaterialCreator.Create() shader == null");
                return;
            }

            foreach (var obj in Selection.objects)
            {
                if (obj.GetType() == typeof(Texture2D))
                {
                    Texture2D texture = (Texture2D)obj;

                    if (GetMaterial(texture, out string path, out string type))
                    {
                        CreateMaterial(texture, path, type);
                    }
                }
            }
        }

        private string GetDirectory(Object obj)
        {
            string path = AssetDatabase.GetAssetPath(assetObject: obj);

            if (path.Contains(value: '/'))
            {
                path = path[..path.LastIndexOf(value: '/')];
            }

            return path;
        }
        private bool GetMaterial(Texture2D texture, out string path, out string type)
        {
            List<string> format = new(texture.name.Split('_'));
            type = format.Count > 0 ? format[^1] : STRING_EMPTY;
            path = STRING_EMPTY;

            if (format.Count <= 0)
            {
                return false;
            }

            if (format[0] == "TEX")
            {
                format.RemoveAt(0);
            }

            if (type == "ColorMap")
            {
                format.RemoveAt(format.Count - 1);
            }
            else if (type == "NormalMap")
            {
                format.RemoveAt(format.Count - 1);
            }
            else if (type == "MaskMap")
            {
                format.RemoveAt(format.Count - 1);
            }

            path = GetDirectory(texture) + "/" + ("MAT_" + string.Join("_", format)) + ".mat";

            return true;
        }
        private void CreateMaterial(Texture2D texture, string path, string type)
        {
            Material material;

            if (AssetDatabase.AssetPathExists(path))
            {
                material = AssetDatabase.LoadAssetAtPath(path, typeof(Material)) as Material;
            }
            else
            {
                material = new(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            if (type == "ColorMap")
            {
                material.SetTexture("_BaseMap", texture);
            }
            else if (type == "NormalMap")
            {
                material.SetTexture("_NormalMap", texture);
            }
            else if (type == "MaskMap")
            {
                material.SetTexture("_MaskMap", texture);
            }

            material.enableInstancing = true;
        }
    }
}