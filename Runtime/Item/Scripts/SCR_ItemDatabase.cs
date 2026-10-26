using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Item
{
    public static class ItemDatabase
    {
        public static int Version => version; private static int version;

        private static InventoryEntity orphanInventory = null;
        private static readonly Dictionary<string, int> idLookup = new();
        private static readonly Dictionary<string, int> tagLookup = new();
        private static ItemDefinition[] definitions = Array.Empty<ItemDefinition>();
        private static ItemTag[] tags = Array.Empty<ItemTag>();
        private static ItemEntitySpawner spawner;

        internal static void Build(ItemEntry[] entries, string[] _tags)
        {
            if (entries == null || _tags == null)
            {
                return;
            }

            if (_tags.Length > 63)
            {
                throw new InvalidOperationException("Item database supports a maximum of 63 non-generic tags.");
            }

            tagLookup.Clear();
            idLookup.Clear();

            definitions = new ItemDefinition[entries.Length];
            tags = new ItemTag[_tags.Length];

            for (int i = 0; i < _tags.Length; i++)
            {
                string key = _tags[i];
                int index = i;

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

            version++;
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
                throw new ArgumentNullException(nameof(id), $"Item id [{id}] is not valid!");
            }

            return GetDefinition(id.Index);
        }

        public static ItemEntity CreateEntity(ItemData data, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (data == null)
            {
                throw new ArgumentNullException($"Trying to create item entity with null data! {nameof(data)}");
            }

            if (spawner == null)
            {
                throw new ArgumentNullException(nameof(spawner), "Item database entity spawner missing!?");
            }

            CreateOrphanInventory();

            GameObject spawned = spawner(data.BaseID.GetDefinition().PrefabID, position, rotation, parent);

            if (spawned == null)
            {
                return null;
            }

            if (!spawned.TryGetComponent(out ItemEntity entity))
            {
                throw new ArgumentNullException($"[{nameof(ItemEntity)}] component not found in [{spawned.name}]");
            }

            entity.ImportFrom(data);
            return entity;
        }

        public static void GetItemsByTag(ref List<ItemID> items, params ItemTag[] tags) => GetItemsByTag(ref items, tags.CreateMask());
        public static void GetItemsByTag(ref List<ItemID> items, ItemMask mask)
        {
            if (items == null)
            {
                return;
            }

            items.Clear();

            foreach (ItemDefinition definition in definitions)
            {
                if (definition.Mask.HasAny(mask))
                {
                    items.Add(definition.ID);
                }
            }
        }

        public static void SetSpawner(ItemEntitySpawner value) => spawner = value;

        public static InventoryEntity GetOrphanInventory()
        {
            if (orphanInventory == null)
            {
                CreateOrphanInventory();
            }

            return orphanInventory;
        }
        private static void CreateOrphanInventory()
        {
            if (orphanInventory != null)
            {
                return;
            }

            orphanInventory = InventoryEntity.Create(10, 10, 1000);
        }
    }
}