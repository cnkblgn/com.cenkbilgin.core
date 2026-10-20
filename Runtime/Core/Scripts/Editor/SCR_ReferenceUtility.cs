using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Core.Editor
{
    public static class ReferenceUtility
    {
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