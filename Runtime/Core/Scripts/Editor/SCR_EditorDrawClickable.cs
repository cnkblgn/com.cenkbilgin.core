using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
    [CustomEditor(typeof(Object), true)]
    internal sealed class EditorDrawClickable : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();

            MethodInfo[] methods = target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (MethodInfo method in methods)
            {
                Clickable attribute = method.GetCustomAttribute<Clickable>();

                if (attribute == null)
                {
                    continue;
                }

                string label = string.IsNullOrEmpty(attribute.Label) ? method.Name : attribute.Label;

                EditorGUILayout.Space(5);

                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    if (GUILayout.Button(label))
                    {
                        method.Invoke(target, null);

                        UnityEditor.EditorUtility.SetDirty(target);
                    }
                }
                GUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
