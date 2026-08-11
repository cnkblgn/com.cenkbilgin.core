using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Item
{
    public static class ItemDatabase
    {
        private static SearchCollection<string> idSearch = new(Array.Empty<SearchEntry<string>>());
        private static SearchCollection<string> tagSearch = new(Array.Empty<SearchEntry<string>>());
        private static readonly Dictionary<string, int> idLookup = new();
        private static readonly Dictionary<string, int> tagLookup = new();
        private static ItemDefinition[] database = Array.Empty<ItemDefinition>();
        private static ItemProcessor processor;
        private static Transform root;

        internal static void Build(string[] idCollection, string[] tagCollection, ItemEntry[] entries)
        {
            if (idCollection == null || tagCollection == null)
            {
                return;
            }

            database = new ItemDefinition[entries.Length];
            tagLookup.Clear();
            idLookup.Clear();

            tagSearch = new(new SearchEntry<string>[tagCollection.Length + 1]);
            idSearch = new(new SearchEntry<string>[idCollection.Length]);

            tagLookup["GENERIC"] = 0;
            tagSearch.Entries[0] = new("GENERIC", "GENERIC");

            for (int i = 0; i < idCollection.Length; i++)
            {
                string key = idCollection[i];
                int index = i;

                idLookup[key] = index;
                idSearch.Entries[i] = new SearchEntry<string>(key, key);
            }

            for (int i = 0; i < tagCollection.Length; i++)
            {
                string key = tagCollection[i];
                int index = i + 1;

                tagLookup[key] = index;
                tagSearch.Entries[index] = new SearchEntry<string>(key, key);
            }

            for (int i = 0; i < entries.Length; i++)
            {
                database[i] = new(entries[i]);
            }

            Debug.Log($"Item database build successfull!");
        }

        public static IReadOnlyList<ItemDefinition> GetDatabase() => database;
        public static SearchCollection<string> GetIDs() => idSearch;
        public static SearchCollection<string> GetTags() => tagSearch;
        public static int GetIDIndex(string key) => idLookup.TryGetValue(key, out int index) ? index : -1;
        public static int GetTagIndex(string key) => tagLookup.TryGetValue(key, out int index) ? index : -1;
        public static ItemTag GetTag(int index)
        {
            if (index >= database.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Item tag not found index out of range");
            }

            return new(tagSearch.Entries[index].Value, index);
        }
        public static ItemDefinition GetDefinition(int index)
        {
            if (index >= database.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Item not found index out of range");
            }

            return database[index];
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

        public static List<ItemID> GetItemsByTag(params ItemTag[] tags) => GetItemsByTag(tags.CreateMask());
        public static List<ItemID> GetItemsByTag(ulong tags)
        {
            List<ItemID> items = new();

            foreach (ItemDefinition definition in database)
            {
                if (definition.Tags.HasAny(tags))
                {
                    items.Add(definition.ID);
                }
            }

            return items;
        }

        internal static ItemProcessor GetProcessor() => processor;
        public static void SetProcessor(ItemProcessor value) => processor = value;

        internal static Transform GetRoot() => root;
        public static void SetRoot(Transform transform) => root = transform;
    }
}