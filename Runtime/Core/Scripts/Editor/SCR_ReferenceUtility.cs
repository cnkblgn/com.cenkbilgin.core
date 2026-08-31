using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Core.Editor
{
    using static ReferenceDatabase;

    public static class ReferenceUtility
    {
        public static void FixReferences(UnityEngine.Object root)
        {
            if (root == null)
            {
                return;
            }

            Dictionary<string, int> sizes = LastRefSizes.GetOrCreateValue(root);

            foreach (FieldInfo arrayField in GetTrackedArrayFields(root.GetType()))
            {
                if (arrayField.GetValue(root) is not IList list)
                {
                    continue;
                }

                int currentSize = list.Count;

                if (!sizes.TryGetValue(arrayField.Name, out int previousSize))
                {
                    sizes[arrayField.Name] = currentSize;
                    continue;
                }

                if (currentSize > previousSize)
                {
                    for (int i = previousSize; i < currentSize; i++)
                    {
                        object element = list[i];
                        ClearReferenceFields(element);

                        if (element.GetType().IsValueType)
                        {
                            list[i] = element;
                        }
                    }
                }

                sizes[arrayField.Name] = currentSize;
            }
        }
        private static void ClearReferenceFields(object element)
        {
            foreach (FieldInfo refField in GetRefFields(element.GetType()))
            {
                refField.SetValue(element, null);
            }
        }
        private static Type GetElementType(Type fieldType)
        {
            if (fieldType.IsArray)
            {
                return fieldType.GetElementType();
            }

            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                return fieldType.GetGenericArguments()[0];
            }

            return null;
        }
        private static FieldInfo[] GetTrackedArrayFields(Type type)
        {
            if (ArrayFieldsCache.TryGetValue(type, out FieldInfo[] cached))
            {
                return cached;
            }

            List<FieldInfo> result = new();

            foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!field.IsPublic && !Attribute.IsDefined(field, typeof(SerializeField)))
                {
                    continue;
                }

                Type elementType = GetElementType(field.FieldType);

                if (elementType == null)
                {
                    continue;
                }

                if (GetRefFields(elementType).Length == 0)
                {
                    continue;
                }

                result.Add(field);
            }

            FieldInfo[] arr = result.ToArray();
            ArrayFieldsCache[type] = arr;
            return arr;
        }
        private static FieldInfo[] GetRefFields(Type type)
        {
            if (RefFieldsCache.TryGetValue(type, out FieldInfo[] cached))
            {
                return cached;
            }

            List<FieldInfo> result = new();

            foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (Attribute.IsDefined(field, typeof(SerializeReference)))
                {
                    result.Add(field);
                }
            }

            FieldInfo[] arr = result.ToArray();
            RefFieldsCache[type] = arr;
            return arr;
        }
    
        internal static Type GetBaseType(FieldInfo fieldInfo)
        {
            return GetBaseType(fieldInfo.FieldType);
        }
        internal static Type GetBaseType(Type fieldType)
        {
            if (fieldType.IsArray)
            {
                return fieldType.GetElementType();
            }

            if (fieldType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(fieldType))
            {
                Type[] args = fieldType.GetGenericArguments();

                if (args.Length == 1)
                {
                    return args[0];
                }
            }

            return fieldType;
        }
        internal static bool Validate(FieldInfo fieldInfo, out string error)
        {
            Type baseType = GetBaseType(fieldInfo);

            if (typeof(UnityEngine.Object).IsAssignableFrom(baseType))
            {
                error = $"[Reference] cannot be used with '{baseType.Name}'.\n" + "UnityEngine.Object types are not supported by SerializeReference.";

                return false;
            }

            error = null;
            return true;
        }
    }
}