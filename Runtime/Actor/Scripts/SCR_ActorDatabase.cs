using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Actors
{
    public static class ActorDatabase
    {
        private static SearchCollection<string> idSearch = new(Array.Empty<SearchEntry<string>>());
        private static SearchCollection<string> tagSearch = new(Array.Empty<SearchEntry<string>>());
        private static readonly Dictionary<string, int> idLookup = new();
        private static readonly Dictionary<string, int> tagLookup = new();
        private static List<ActorEntry>[] database = Array.Empty<List<ActorEntry>>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnRuntimeInitialize() => database = Array.Empty<List<ActorEntry>>();

        internal static void Build(string[] idCollection, string[] tagCollection)
        {
            if (idCollection == null || tagCollection == null)
            {
                return;
            }

            database = new List<ActorEntry>[idCollection.Length];

            tagLookup.Clear();
            idLookup.Clear();
            idSearch = new(new SearchEntry<string>[idCollection.Length]);
            tagSearch = new(new SearchEntry<string>[tagCollection.Length + 1]);

            tagLookup["GENERIC"] = 0;
            tagSearch.Entries[0] = new("GENERIC", "GENERIC");

            for (int i = 0; i < idCollection.Length; i++)
            {
                string key = idCollection[i];
                int index = i;

                idLookup[key] = index;
                idSearch.Entries[i] = new(key, key);
                database[i] = new();
            }

            for (int i = 0; i < tagCollection.Length; i++)
            {
                string key = tagCollection[i];
                int index = i + 1;

                tagLookup[key] = index;
                tagSearch.Entries[index] = new(key, key);
            }

            Debug.Log($"Actor database build successfull!");
        }

        private static List<ActorEntry> GetEntries(int index)
        {
            if (index >= database.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Actor entries not found index out of range");
            }

            return database[index];
        }
        private static List<ActorEntry> GetEntries(ActorID id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentNullException(nameof(id), $"Actor id [{id.Key}] is not valid!");
            }

            return GetEntries(id.Index);
        }
        public static SearchCollection<string> GetIDs() => idSearch;
        public static SearchCollection<string> GetTags() => tagSearch;
        public static ActorID GetID(int index)
        {
            if (index >= database.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Actor id not found index out of range");
            }

            return new(idSearch.Entries[index].Value, index);
        }
        public static int GetIDIndex(string key) => idLookup.TryGetValue(key, out int index) ? index : 1;
        public static ActorID GetTag(int index)
        {
            if (index >= tagSearch.Entries.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Actor tag not found index out of range");
            }

            return new(tagSearch.Entries[index].Value, index);
        }
        public static int GetTagIndex(string key) => tagLookup.TryGetValue(key, out int index) ? index : -1;

        internal static bool TryGetAnyActor(ActorID id, out Actor actor)
        {
            actor = null;

            List<ActorEntry> entries = GetEntries(id);

            if (entries == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"id not found in database: [{id.Key}]");
#endif
                return false;
            }

            if (entries.Count <= 0)
            {
                return false;
            }

            actor = entries[0].Actor;
            return true;
        }
        internal static bool TryGetAnyActor(ActorTag tag, out Actor actor) => TryGetAnyActor(tag.Mask, out actor);
        internal static bool TryGetAnyActor(ActorTag[] tags, out Actor actor) => TryGetAnyActor(tags.CreateMask(), out actor);
        internal static bool TryGetAnyActor(ulong tags, out Actor actor)
        {
            actor = null;

            if (tags == 0)
            {
                return false;
            }

            foreach (List<ActorEntry> entries in database)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    Actor tempActor = entries[i].Actor;

                    if (tempActor.HasAny(tags))
                    {
                        actor = tempActor;
                        return true;
                    }
                }
            }

            return false;
        }
        internal static bool TryGetAllActors(ActorID id, out IReadOnlyList<ActorEntry> actors)
        {
            actors = GetEntries(id);

            if (actors == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"id not found in database: [{id.Key}]");
#endif
                return false;
            }

            return actors.Count > 0;
        }
        internal static bool TryGetAllActors(ActorTag tag, out List<Actor> actors) => TryGetAllActors(tag.Mask, out actors);
        internal static bool TryGetAllActors(ActorTag[] tags, out List<Actor> actors) => TryGetAllActors(tags.CreateMask(), out actors);
        internal static bool TryGetAllActors(ulong tags, out List<Actor> actors)
        {
            actors = new();

            if (tags == 0)
            {
                return false;
            }

            bool found = false;

            foreach (List<ActorEntry> entries in database)
            {
                if (entries == null)
                {
                    continue;
                }

                for (int i = 0; i < entries.Count; i++)
                {
                    Actor tempActor = entries[i].Actor;

                    if (tempActor.HasAny(tags))
                    {
                        actors.Add(tempActor);
                        found = true;
                    }
                }
            }

            return found;
        }

        internal static void RegisterActor(ActorID id, Actor actor)
        {
            if (actor == null)
            {
                throw new ArgumentNullException(nameof(actor));
            }

            List<ActorEntry> entries = GetEntries(id);

#if UNITY_EDITOR
            if (entries != null)
            {
                foreach (ActorEntry entry in entries)
                {
                    if (entry == null)
                    {
                        Debug.LogError("Invalid null actor detected!");
                        return;
                    }

                    if (entry.ID == actor.GetInstanceID())
                    {
                        Debug.LogError($"Actor register failed! Duplicate detected! [{entry.ID}]");
                        return;
                    }
                }
            }
#endif

            if (entries == null)
            {
                entries = new();
                database[id.Index] = entries;
            }

            entries.Add(new(actor));
        }
        internal static void RemoveActor(Actor actor)
        {
            if (actor == null)
            {
                throw new ArgumentNullException(nameof(actor));
            }

            List<ActorEntry> entries = GetEntries(actor.ID);

            if (entries == null)
            {
                Debug.LogError($"You are trying to remove invalid actor! [{actor.ID}]");
                return;
            }

            entries.Remove(new(actor));
        }
    }
}