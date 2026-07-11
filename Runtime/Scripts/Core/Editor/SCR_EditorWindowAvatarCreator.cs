using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
    public class EditorWindowAvatarCreator : EditorWindow
    {
        [SerializeField] private GameObject gameObject;
        [SerializeField] private string rootNodeName = "";

        private SerializedProperty propertyGameObject = null;
        private SerializedProperty propertyRootNodeName = null;
        private SerializedObject serializedObject = null;

        [MenuItem("Tools/Avatar Creator")] public static void ShowTool() => GetWindow<EditorWindowAvatarCreator>("Avatar Creator");
        private void OnEnable() 
        { 
            serializedObject = new SerializedObject(this); 

            propertyGameObject = serializedObject.FindProperty("gameObject");
            propertyRootNodeName = serializedObject.FindProperty("rootNodeName");
        }
        private void OnGUI()
        {
            serializedObject.Update();

            using (EditorGUILayout.VerticalScope vertical = new("helpbox", new GUILayoutOption[] { }))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(propertyGameObject, new GUIContent(""));
                    EditorGUILayout.PropertyField(propertyRootNodeName, new GUIContent("Root Node"));
                }

                using (new EditorGUILayout.VerticalScope())
                {
                    if (GUILayout.Button(new GUIContent($"Create Mask"), new GUIStyle("minibutton") { alignment = TextAnchor.MiddleCenter, richText = true, fontStyle = FontStyle.Bold }))
                    {
                        MakeMask();
                    }
                    if (GUILayout.Button(new GUIContent($"Create Avatar"), new GUIStyle("minibutton") { alignment = TextAnchor.MiddleCenter, richText = true, fontStyle = FontStyle.Bold }))
                    {
                        MakeAvatar();
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
        private void MakeMask()
        {
            if (gameObject != null)
            {
                AvatarMask avatarMask = new();
                avatarMask.AddTransformPath(gameObject.transform);
                string path = string.Format("Assets/{0}.mask", gameObject.name.Replace(':', '_'));
                AssetDatabase.CreateAsset(avatarMask, path);
            }
        }
        private void MakeAvatar()
        {
            if (gameObject != null)
            {
                Avatar avatar = AvatarBuilder.BuildGenericAvatar(gameObject, rootNodeName);
                avatar.name = gameObject.name;
                string path = string.Format("Assets/{0}.ht", avatar.name.Replace(':', '_'));
                AssetDatabase.CreateAsset(avatar, path);
            }
        }
    }
}