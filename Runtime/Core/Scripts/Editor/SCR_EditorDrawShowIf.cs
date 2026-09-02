using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
    [CustomPropertyDrawer(typeof(ShowIf), true)]
    public class EditorDrawShowIf : PropertyDrawer
    {
        private const BindingFlags MEMBER_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly Dictionary<(Type, string), MemberInfo> memberCache = new();
        private static readonly Dictionary<string, string[]> pathCache = new();

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
            object target = GetParentObject(property) ?? property.serializedObject.targetObject;
            bool result = Evaluate(target, attribute.ConditionMethod);

            return attribute.Inverse ? !result : result;
        }

        public static bool Evaluate(object target, string name)
        {
            if (target == null)
            {
                return true;
            }

            Type type = target.GetType();
            MemberInfo member = ResolveMember(type, name);

            if (member == null)
            {
                Debug.LogError($"Could not find bool field/property/method '{name}' on {type.Name}.");
                return true;
            }

            switch (member)
            {
                case MethodInfo method:
                    if (method.ReturnType != typeof(bool))
                    {
                        Debug.LogError($"{type.Name}.{name}() must return bool.");
                        return true;
                    }

                    return (bool)method.Invoke(target, null);

                case FieldInfo field:
                    if (field.FieldType != typeof(bool))
                    {
                        Debug.LogError($"{type.Name}.{name} must be bool.");
                        return true;
                    }

                    return (bool)field.GetValue(target);

                case PropertyInfo property:
                    if (property.PropertyType != typeof(bool))
                    {
                        Debug.LogError($"{type.Name}.{name} must be bool.");
                        return true;
                    }

                    return (bool)property.GetValue(target);

                default:
                    return true;
            }
        }

        private static MemberInfo ResolveMember(Type type, string name)
        {
            var key = (type, name);

            if (memberCache.TryGetValue(key, out MemberInfo cached))
            {
                return cached;
            }

            MemberInfo resolved = null;
            Type current = type;

            while (current != null && resolved == null)
            {
                resolved = (MemberInfo)current.GetMethod(name, MEMBER_FLAGS, null, Type.EmptyTypes, null) ?? (MemberInfo)current.GetField(name, MEMBER_FLAGS) ?? current.GetProperty(name, MEMBER_FLAGS);
                current = current.BaseType;
            }

            memberCache[key] = resolved;
            return resolved;
        }
        private static MemberInfo ResolveFieldOrProperty(Type type, string name)
        {
            var key = (type, name);

            if (memberCache.TryGetValue(key, out MemberInfo cached))
            {
                return cached;
            }

            MemberInfo resolved = null;
            Type current = type;

            while (current != null && resolved == null)
            {
                resolved = (MemberInfo)current.GetField(name, MEMBER_FLAGS) ?? current.GetProperty(name, MEMBER_FLAGS);
                current = current.BaseType;
            }

            memberCache[key] = resolved;
            return resolved;
        }

        private static object GetParentObject(SerializedProperty property)
        {
            string[] elements = GetPathSegments(property.propertyPath);
            object obj = property.serializedObject.targetObject;

            for (int i = 0; i < elements.Length - 1; i++)
            {
                string element = elements[i];
                int bracketIndex = element.IndexOf('[');

                if (bracketIndex >= 0)
                {
                    string elementName = element.Substring(0, bracketIndex);
                    int index = int.Parse(element.Substring(bracketIndex + 1, element.Length - bracketIndex - 2));
                    obj = GetFieldOrPropertyValue(obj, elementName);
                    obj = GetElementAtIndex(obj, index);
                }
                else
                {
                    obj = GetFieldOrPropertyValue(obj, element);
                }

                if (obj == null)
                {
                    return null;
                }
            }

            return obj;
        }
        private static string[] GetPathSegments(string propertyPath)
        {
            if (pathCache.TryGetValue(propertyPath, out string[] cached))
            {
                return cached;
            }

            string normalized = propertyPath.Replace(".Array.data[", "[");
            string[] segments = normalized.Split('.');
            pathCache[propertyPath] = segments;

            return segments;
        }
        private static object GetFieldOrPropertyValue(object source, string name)
        {
            if (source == null)
            {
                return null;
            }

            Type type = source.GetType();
            MemberInfo member = ResolveFieldOrProperty(type, name);

            return member switch
            {
                FieldInfo f => f.GetValue(source),
                PropertyInfo p => p.GetValue(source, null),
                _ => null
            };
        }
        private static object GetElementAtIndex(object source, int index)
        {
            if (source is IList list)
            {
                return (index >= 0 && index < list.Count) ? list[index] : null;
            }

            if (source is IEnumerable enumerable)
            {
                IEnumerator enumerator = enumerable.GetEnumerator();

                for (int i = 0; i <= index; i++)
                {
                    if (!enumerator.MoveNext())
                    {
                        return null;
                    }
                }

                return enumerator.Current;
            }

            return null;
        }
    }
}