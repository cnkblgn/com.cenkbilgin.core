using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
    [CustomPropertyDrawer(typeof(ShowIf), true)]
    public class EditorDrawShowIf : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!ShouldDraw(property))
            {
                return 0f;
            }

            return EditorGUI.GetPropertyHeight(property, label, true);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!ShouldDraw(property))
            {
                return;
            }

            EditorGUI.PropertyField(position, property, label, true);
        }

        private bool ShouldDraw(SerializedProperty property)
        {
            ShowIf attribute = (ShowIf)this.attribute;

            bool result = Evaluate(property.serializedObject.targetObject, attribute.ConditionMethod);

            return attribute.Inverse ? !result : result;
        }
        public static bool Evaluate(Object target, string name)
        {
            if (target == null)
            {
                return true;
            }

            var type = target.GetType();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            MethodInfo method = type.GetMethod(name, flags);

            if (method != null)
            {
                if (method.ReturnType != typeof(bool))
                {
                    Debug.LogError($"{type.Name}.{name}() must return bool.");
                    return true;
                }

                if (method.GetParameters().Length != 0)
                {
                    Debug.LogError($"{type.Name}.{name}() cannot have parameters.");
                    return true;
                }

                return (bool)method.Invoke(target, null);
            }

            FieldInfo field = type.GetField(name, flags);

            if (field != null)
            {
                if (field.FieldType != typeof(bool))
                {
                    Debug.LogError($"{type.Name}.{name} must be bool.");
                    return true;
                }

                return (bool)field.GetValue(target);
            }

            PropertyInfo property = type.GetProperty(name, flags);

            if (property != null)
            {
                if (property.PropertyType != typeof(bool))
                {
                    Debug.LogError($"{type.Name}.{name} must be bool.");
                    return true;
                }

                return (bool)property.GetValue(target);
            }

            Debug.LogError($"Could not find bool field/property/method '{name}' on {type.Name}.");
            return true;
        }
    }
}
