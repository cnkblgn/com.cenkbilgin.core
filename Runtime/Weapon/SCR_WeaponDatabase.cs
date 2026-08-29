using System;
using System.Collections.Generic;
using UnityEngine;
using Core;

namespace Core.Weapon
{
    public static class WeaponDatabase
    {
        private static readonly Dictionary<string, int> idLookup = new();
        private static readonly Dictionary<string, int> tagLookup = new();
        private static WeaponDefinition[] definitions = Array.Empty<WeaponDefinition>();
        private static WeaponTag[] tags = Array.Empty<WeaponTag>();

        internal static void Build(WeaponEntry[] entries, string[] _tags)
        {
            if (entries == null || _tags == null)
            {
                return;
            }

            tagLookup.Clear();
            idLookup.Clear();

            definitions = new WeaponDefinition[entries.Length];
            tags = new WeaponTag[_tags.Length];

            for (int i = 0; i < _tags.Length; i++)
            {
                string key = _tags[i];
                int index = i;

                if (string.IsNullOrEmpty(key))
                {
                    Debug.LogError("Weapon database tag key is invalid!?");
                    continue;
                }

                tagLookup[key] = index;
                tags[index] = new(key, index);
            }

            for (int i = 0; i < entries.Length; i++)
            {
                WeaponEntry entry = entries[i];
                string key = entry.ID.Key;

                idLookup[key] = i;
                definitions[i] = new(entry);
            }

            Debug.Log($"Weapon database build successfull!");
        }

        public static int GetIDIndex(string key) => idLookup.TryGetValue(key, out int index) ? index : -1;
        public static int GetTagIndex(string key) => tagLookup.TryGetValue(key, out int index) ? index : -1;
        public static WeaponTag GetTag(int index)
        {
            if (index >= definitions.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Weapon tag not found index out of range");
            }

            return tags[index];
        }
        public static IReadOnlyList<WeaponTag> GetTags() => tags;
        public static IReadOnlyList<WeaponDefinition> GetDefinitions() => definitions;
        public static WeaponDefinition GetDefinition(int index)
        {
            if (index >= definitions.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Weapon not found index out of range");
            }

            return definitions[index];
        }
        public static WeaponDefinition GetDefinition(WeaponID id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentException($"Weapon id [{nameof(id)}] is not valid!");
            }

            return GetDefinition(id.Index);
        }

        public static void GetWeaponsByTag(ref List<WeaponID> weapons, params WeaponTag[] tags) => GetWeaponsByTag(ref weapons, tags.CreateMask());
        public static void GetWeaponsByTag(ref List<WeaponID> weapons, ulong tags)
        {
            if (weapons == null)
            {
                return;
            }

            weapons.Clear();

            foreach (WeaponDefinition definition in definitions)
            {
                if (definition.Tags.HasAny(tags))
                {
                    weapons.Add(definition.ID);
                }
            }
        }
    }
}