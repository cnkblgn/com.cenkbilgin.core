using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
    using static CoreUtility;

    internal sealed class EditorWindowMaterialCreator : EditorWindow
    {
        [SerializeField] private Shader shader;
        [SerializeField] private string typePrefix = "TEX";
        [SerializeField] private string typeAlbedo = "C";
        [SerializeField] private string typeNormal = "N";
        [SerializeField] private string typeMask = "M";

        private SerializedProperty propertyShader = null;
        private SerializedProperty propertyTypePrefix = null;
        private SerializedProperty propertyTypeAlbedo = null;
        private SerializedProperty propertyTypeNormal = null;
        private SerializedProperty propertyTypeMask = null;
        private SerializedObject serializedObject = null;

        private void OnEnable()
        {
            serializedObject = new SerializedObject(this);
            propertyShader = serializedObject.FindProperty("shader");
            propertyTypePrefix = serializedObject.FindProperty("typePrefix");
            propertyTypeAlbedo = serializedObject.FindProperty("typeAlbedo");
            propertyTypeNormal = serializedObject.FindProperty("typeNormal");
            propertyTypeMask = serializedObject.FindProperty("typeMask");
        }

        [MenuItem("Tools/Material Creator")]
        public static void ShowTool() => GetWindow<EditorWindowMaterialCreator>("Material Creator");

        private void OnGUI()
        {
            serializedObject.Update();

            using (EditorGUILayout.VerticalScope vertical = new("helpbox", new GUILayoutOption[] { }))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(propertyShader, new GUIContent("Shader"));
                }

                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Naming Convention", EditorStyles.boldLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(propertyTypePrefix, new GUIContent("Prefix"));
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(propertyTypeAlbedo, new GUIContent("Albedo Suffix"));
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(propertyTypeNormal, new GUIContent("Normal Suffix"));
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(propertyTypeMask, new GUIContent("Mask Suffix"));
                }

                EditorGUILayout.Space(4);
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
                Debug.LogError("EditorWindowMaterialCreator.Create() shader == null");
                return;
            }

            bool anyCreated = false;

            foreach (var obj in Selection.objects)
            {
                if (obj.GetType() == typeof(Texture2D))
                {
                    Texture2D texture = (Texture2D)obj;

                    if (GetMaterial(texture, out string path, out string type))
                    {
                        CreateMaterial(texture, path, type);
                        anyCreated = true;
                    }
                    else
                    {
                        Debug.LogWarning($"EditorWindowMaterialCreator: '{texture.name}' isimlendirme kuralýyla eþleþmedi, atlandý.");
                    }
                }
            }

            if (anyCreated)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
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

            if (!string.IsNullOrEmpty(typePrefix) && format[0] == typePrefix)
            {
                format.RemoveAt(0);
            }

            bool recognized = type == typeAlbedo || type == typeNormal || type == typeMask;

            if (!recognized)
            {
                return false;
            }

            format.RemoveAt(format.Count - 1);

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

            if (type == typeAlbedo)
            {
                material.SetTexture("_BaseMap", texture);
            }
            else if (type == typeNormal)
            {
                material.SetTexture("_NormalMap", texture);
            }
            else if (type == typeMask)
            {
                material.SetTexture("_MaskMap", texture);
            }

            material.enableInstancing = true;

            UnityEditor.EditorUtility.SetDirty(material);
        }
    }
}