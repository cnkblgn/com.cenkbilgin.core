using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Item
{
    public static class ItemDatabase
    {
        private static readonly Dictionary<string, int> idLookup = new();
        private static readonly Dictionary<string, int> tagLookup = new();
        private static ItemDefinition[] definitions = Array.Empty<ItemDefinition>();
        private static ItemTag[] tags = Array.Empty<ItemTag>();
        private static ItemProcessor processor;
        private static Transform root;

        internal static void Build(ItemEntry[] entries, string[] _tags)
        {
            if (entries == null || _tags == null)
            {
                return;
            }

            tagLookup.Clear();
            idLookup.Clear();

            definitions = new ItemDefinition[entries.Length];
            tags = new ItemTag[_tags.Length + 1];
            tags[0] = ItemTag.GENERIC;
            tagLookup[ItemTag.GENERIC.Key] = 0;

            for (int i = 0; i < _tags.Length; i++)
            {
                string key = _tags[i];
                int index = i + 1;

                if (string.IsNullOrEmpty(key))
                {
                    Debug.LogError("Item database tag key is invalid!?");
                    continue;
                }

                tagLookup[key] = index;
                tags[index] = new(key, index);
            }

            for (int i = 0; i < entries.Length; i++)
            {
                ItemEntry entry = entries[i];
                string key = entry.ID.Key;

                idLookup[key] = i;
                definitions[i] = new(entry);
            }

            Debug.Log($"Item database build successfull!");
        }

        public static int GetIDIndex(string key) => idLookup.TryGetValue(key, out int index) ? index : -1;
        public static int GetTagIndex(string key) => tagLookup.TryGetValue(key, out int index) ? index : -1;
        public static ItemTag GetTag(int index)
        {
            if (index >= definitions.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Item tag not found index out of range");
            }

            return tags[index];
        }
        public static IReadOnlyList<ItemTag> GetTags() => tags;
        public static IReadOnlyList<ItemDefinition> GetDefinitions() => definitions;
        public static ItemDefinition GetDefinition(int index)
        {
            if (index >= definitions.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Item not found index out of range");
            }

            return definitions[index];
        }
        public static ItemDefinition GetDefinition(ItemID id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentException($"Item id [{nameof(id)}] is not valid!");
            }

            return GetDefinition(id.Index);
        }

        public static ItemEntity CreateEntity(ItemID id, Vector3 position, Quaternion rotation, Transform parent = null) => CreateEntity(new ItemData(id), position, rotation, parent);
        public static ItemEntity CreateEntity(ItemData data, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (data == null)
            {
                throw new ArgumentNullException($"Trying to create item entity with null data! {nameof(data)}");
            }

            GameObject spawned = data.BaseID.GetDefinition().EntityID.Spawn(position, rotation, parent == null ? root : parent);

            if (spawned == null)
            {
                return null;
            }

            if (!spawned.TryGetComponent(out ItemEntity entity))
            {
                throw new ArgumentNullException("Spawned entity does not have [ItemEntity] component?");
            }

            entity.ImportFrom(data);
            processor?.Invoke(entity);
            return entity;
        }

        public static void GetItemsByTag(ref List<ItemID> items, params ItemTag[] tags) => GetItemsByTag(ref items, tags.CreateMask());
        public static void GetItemsByTag(ref List<ItemID> items, ulong tags)
        {
            if (items == null)
            {
                return;
            }

            items.Clear();

            foreach (ItemDefinition definition in definitions)
            {
                if (definition.Tags.HasAny(tags))
                {
                    items.Add(definition.ID);
                }
            }
        }

        internal static ItemProcessor GetProcessor() => processor;
        public static void SetProcessor(ItemProcessor value) => processor = value;

        internal static Transform GetRoot() => root;
        public static void SetRoot(Transform transform) => root = transform;
    }
}