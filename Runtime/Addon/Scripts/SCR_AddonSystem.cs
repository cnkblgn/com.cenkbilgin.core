using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Core.Addon
{
    internal static class AddonSystem
    {
        private static readonly List<AddonLoaderEntry> entries = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void OnAfterAssembliesLoaded()
        {
            RegisterAll();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnBeforeSceneLoad()
        {
            LoadAll();
            BuildAll();
        }

        private static void RegisterAll()
        {
            entries.Clear();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int i = 0; i < assemblies.Length; i++)
            {
                Type[] types;

                try
                {
                    types = assemblies[i].GetTypes();
                }
                catch (ReflectionTypeLoadException exception)
                {
                    types = exception.Types;
                }

                for (int j = 0; j < types.Length; j++)
                {
                    Type type = types[j];

                    if (type == null || type.IsAbstract)
                    {
                        continue;
                    }

                    AddonLoader attribute = type.GetCustomAttribute<AddonLoader>();
                    bool hasAttribute = attribute != null;
                    bool implementsInterface = typeof(IAddonLoader).IsAssignableFrom(type);

                    if (hasAttribute && !implementsInterface)
                    {
                        Debug.LogError($"[ModLoader] is defined on '{type.FullName}', but the type does not implement IModLoader.");
                        continue;
                    }

                    if (!hasAttribute && implementsInterface)
                    {
                        Debug.LogError($"'{type.FullName}' implements IModLoader but is missing the [ModLoader] attribute.");
                        continue;
                    }

                    if (!hasAttribute)
                    {
                        continue;
                    }

                    if (Activator.CreateInstance(type) is not IAddonLoader loader)
                    {
                        Debug.LogError($"Failed to create mod loader '{type.FullName}'.");
                        continue;
                    }

                    entries.Add(new(type, loader, attribute.Priority));
                }
            }

            entries.Sort(static (a, b) =>
            {
                int priority = a.Priority.CompareTo(b.Priority);

                if (priority != 0)
                {
                    return priority;
                }

                return string.Compare(a.Type.FullName, b.Type.FullName, StringComparison.Ordinal);
            });
        }
        private static void LoadAll()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].Loader.Load();
            }
        }
        private static void BuildAll()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].Loader.Build();
            }
        }
    }
}