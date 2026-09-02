using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Reflection;
using UnityEditor;

namespace Core.Editor
{
    [InitializeOnLoad]
    internal static class ReferenceDatabase
    {
        private static readonly Dictionary<Type, SearchCollection<Type>> database = new();

        internal static readonly ConditionalWeakTable<object, Dictionary<string, int>> LastRefSizes = new();
        internal static readonly Dictionary<Type, FieldInfo[]> ArrayFieldsCache = new();
        internal static readonly Dictionary<Type, FieldInfo[]> RefFieldsCache = new();

        static ReferenceDatabase() { AssemblyReloadEvents.beforeAssemblyReload += ClearAll; }

        public static void ClearAll()
        {
            database.Clear();
            ArrayFieldsCache.Clear();
            RefFieldsCache.Clear();
        }

        public static SearchCollection<Type> GetCollection(Type baseType)
        {
            if (database.TryGetValue(baseType, out SearchCollection<Type> collection))
            {
                return collection;
            }

            collection = BuildCollection(baseType);
            database[baseType] = collection;

            return collection;
        }
        private static SearchCollection<Type> BuildCollection(Type baseType)
        {
            List<SearchEntry<Type>> entries = new();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                Type[] types;

                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types;
                }

                if (types == null)
                {
                    continue;
                }

                foreach (Type type in types)
                {
                    if (type == null || type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition)
                    {
                        continue;
                    }

                    if (!baseType.IsAssignableFrom(type))
                    {
                        continue;
                    }

                    string label = ObjectNames.NicifyVariableName(type.Name);

                    entries.Add(new SearchEntry<Type>(label, type));
                }
            }

            entries.Sort((a, b) => string.Compare(a.Label, b.Label, StringComparison.Ordinal));

            return new SearchCollection<Type>(entries.ToArray());
        }
    }
}
