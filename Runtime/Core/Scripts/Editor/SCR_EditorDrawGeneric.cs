using System.Reflection;
using UnityEditor;
using UnityEngine;
using Core;

namespace Core.Editor
{
    [CustomEditor(typeof(Object), true)]
    internal sealed class EditorDrawGeneric : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawShowIfHideIf();

            DrawClickable();

            serializedObject.ApplyModifiedProperties();
        }


        private void DrawShowIfHideIf()
        {
            SerializedProperty prop = serializedObject.GetIterator();
            bool enterChildren = true;

            while (prop.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (prop.name == "m_Script")
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.PropertyField(prop);
                    }

                    continue;
                }

                if (ShouldDraw(prop))
                {
                    EditorGUILayout.PropertyField(prop, true);
                }
            }
        }
        private void DrawClickable()
        {
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
        }

        private bool ShouldDraw(SerializedProperty property)
        {
            FieldInfo field = target.GetType().GetField(property.name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (field == null)
            {
                return true;
            }

            var showIf = field.GetCustomAttribute<ShowIf>();

            if (showIf == null)
            {
                return true;
            }

            bool result = EvaluateCondition(showIf.ConditionMethod);
            return showIf.Inverse ? !result : result;
        }
        private bool EvaluateCondition(string name)
        {
            var type = target.GetType();
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            MethodInfo method = type.GetMethod(name, flags);
            if (method != null && method.ReturnType == typeof(bool))
            {
                return (bool)method.Invoke(target, null);
            }

            FieldInfo field = type.GetField(name, flags);
            if (field != null && field.FieldType == typeof(bool))
            {
                return (bool)field.GetValue(target);
            }

            PropertyInfo propInfo = type.GetProperty(name, flags);
            if (propInfo != null && propInfo.PropertyType == typeof(bool))
            {
                return (bool)propInfo.GetValue(target);
            }

            Debug.LogWarning($"Editor draw failed '{name}' named method/field/property does not exitst!");
            return true;
        }        
    }
}