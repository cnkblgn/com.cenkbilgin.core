using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace Core.Editor
{
    /// <summary>
    /// Finds every concrete (non-abstract, non-generic-definition) type assignable to a
    /// given base type/interface across all loaded assemblies, and caches the result as
    /// a SearchCollection&lt;Type&gt; so it's only built once per base type.
    /// Cache is cleared automatically before every assembly reload (script recompile),
    /// so it stays correct even with "Reload Domain" disabled on Enter Play Mode.
    /// </summary>
    [InitializeOnLoad]
    internal static class ReferenceDatabase
    {
        private static readonly Dictionary<Type, SearchCollection<Type>> database = new();
        static ReferenceDatabase() { AssemblyReloadEvents.beforeAssemblyReload += ClearCollection; }

        /// <summary> Manual escape hatch (eg. menu item) if results ever look stale. </summary>
        public static void ClearCollection() => database.Clear();
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
