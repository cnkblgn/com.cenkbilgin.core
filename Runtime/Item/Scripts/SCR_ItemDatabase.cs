using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Item
{
    public static class ItemDatabase
    {
        private static SearchCollection<string> ids = new(Array.Empty<SearchEntry<string>>());
        private static SearchCollection<string> tags = new(Array.Empty<SearchEntry<string>>());
        private static readonly Dictionary<string, int> idDatabase = new();
        private static readonly Dictionary<string, int> tagDatabase = new();
        private static readonly Dictionary<ItemID, ItemDefinition> definitionDatabase = new();
        private static Transform entityRoot;

        internal static void Build(string[] idCollection, string[] tagCollection, ItemEntry[] entries)
        {
            if (idCollection == null || tagCollection == null)
            {
                return;
            }

            definitionDatabase.Clear();
            tagDatabase.Clear();
            idDatabase.Clear();

            tags = new(new SearchEntry<string>[tagCollection.Length]);
            ids = new(new SearchEntry<string>[idCollection.Length]);

            for (int i = 0; i < idCollection.Length; i++)
            {
                string key = idCollection[i];
                int index = i;

                idDatabase[key] = index;
                ids.Entries[i] = new SearchEntry<string>(key, key);
            }

            for (int i = 0; i < tagCollection.Length; i++)
            {
                string key = tagCollection[i];
                int index = i + 1;

                tagDatabase[key] = index;
                tags.Entries[i] = new SearchEntry<string>(key, key);
            }

            for (int i = 0; i < entries.Length; i++)
            {
                definitionDatabase[entries[i].ID] = new(entries[i]);
            }

            Debug.Log($"Item database build successfull!");
        }

        public static SearchCollection<string> GetIDs() => ids;
        public static SearchCollection<string> GetTags() => tags;
        public static int GetIDIndex(string id)
        {
            if (idDatabase.TryGetValue(id, out int a))
            {
                return a;
            }

            return -1;
        }
        public static int GetTagIndex(string id)
        {
            if (tagDatabase.TryGetValue(id, out int a))
            {
                return a;
            }

            return -1;
        }

        public static IReadOnlyCollection<ItemID> GetDatabase() => definitionDatabase.Keys;

        internal static string GetID(int index)
        {
            if (index < 0 || index >= idDatabase.Count)
            {
                throw new IndexOutOfRangeException($"Item index [{index}] is not valid!");
            }

            return ids.Entries[index].Value;
        }
        public static ItemDefinition GetDefinition(int index) => GetDefinition(new ItemID(GetID(index), index));
        public static ItemDefinition GetDefinition(ItemID id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentNullException($"Item id [{nameof(id)}] is not valid!");
            }

            if (!definitionDatabase.TryGetValue(id, out ItemDefinition definition))
            {
                Debug.LogError($"Item id not found in database? [{id.Key}]");
                return null;
            }

            return definition;
        }
        internal static ItemData CreateData(ItemID id)
        {
            if (!definitionDatabase.ContainsKey(id))
            {
                throw new ArgumentNullException($"item definition not found for [{id.Key}]");
            }

            return new(id);
        }
        internal static ItemID CreateID(int index) => new(GetID(index), index);

        public static ItemEntity CreateEntity(ItemID id, Vector3 position, Quaternion rotation, Transform parent = null) => CreateEntity(CreateData(id), position, rotation, parent);
        public static ItemEntity CreateEntity(ItemData data, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (data == null)
            {
                throw new ArgumentNullException($"Trying to create item entity with null data! {nameof(data)}");
            }

            GameObject spawned = data.BaseID.GetDefinition().EntityID.Spawn(position, rotation, parent == null ? entityRoot : parent);

            if (spawned == null)
            {
                return null;
            }

            if (!spawned.TryGetComponent(out ItemEntity entity))
            {
                throw new ArgumentNullException("Spawned entity does not have [ItemEntity] component?");
            }

            entity.ImportFrom(data);
            return entity;
        }

        public static List<ItemID> GetItemsByTag(params ItemTag[] tags) => GetItemsByTag(tags.CreateMask());
        public static List<ItemID> GetItemsByTag(ulong tags)
        {
            List<ItemID> items = new();

            foreach (ItemDefinition definition in definitionDatabase.Values)
            {
                if (definition.Tags.HasAny(tags))
                {
                    items.Add(definition.ID);
                }
            }

            return items;
        }

        internal static Transform GetRoot() => entityRoot;
        public static void SetRoot(Transform transform) => entityRoot = transform;
    }
}