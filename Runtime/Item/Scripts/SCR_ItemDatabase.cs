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
        private static readonly Dictionary<ItemID, ItemDefinition> database = new();
        private static ItemProcessor processor;
        private static Transform root;

        internal static void Build(string[] idCollection, string[] tagCollection, ItemEntry[] entries)
        {
            if (idCollection == null || tagCollection == null)
            {
                return;
            }

            database.Clear();
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
                database[entries[i].ID] = new(entries[i]);
            }

            Debug.Log($"Item database build successfull!");
        }

        public static IReadOnlyCollection<ItemDefinition> GetDatabase() => database.Values;
        public static SearchCollection<string> GetIDs() => idSearch;
        public static SearchCollection<string> GetTags() => tagSearch;
        public static int GetIDIndex(string id)
        {
            if (idLookup.TryGetValue(id, out int a))
            {
                return a;
            }

            return -1;
        }
        public static int GetTagIndex(string id)
        {
            if (tagLookup.TryGetValue(id, out int a))
            {
                return a;
            }

            return -1;
        }
        internal static string GetID(int index)
        {
            if (index < 0 || index >= idLookup.Count)
            {
                throw new IndexOutOfRangeException($"Item index [{index}] is not valid!");
            }

            return idSearch.Entries[index].Value;
        }
        public static ItemDefinition GetDefinition(int index) => GetDefinition(CreateID(index));
        public static ItemDefinition GetDefinition(ItemID id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentNullException($"Item id [{nameof(id)}] is not valid!");
            }

            if (!database.TryGetValue(id, out ItemDefinition definition))
            {
                Debug.LogError($"Item id not found in database? [{id.Key}]");
                return null;
            }

            return definition;
        }
        internal static ItemData CreateData(ItemID id)
        {
            if (!database.ContainsKey(id))
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

            foreach (ItemDefinition definition in database.Values)
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