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
        private static readonly Dictionary<ActorID, List<ActorEntry>> database = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnRuntimeInitialize() => database.Clear();

        internal static void Build(string[] idCollection, string[] tagCollection)
        {
            if (idCollection == null || tagCollection == null)
            {
                return;
            }

            tagLookup.Clear();
            idLookup.Clear();
            tagSearch = new(new SearchEntry<string>[tagCollection.Length]);
            idSearch = new(new SearchEntry<string>[idCollection.Length]);

            for (int i = 0; i < idCollection.Length; i++)
            {
                string key = idCollection[i];
                int index = i + 1;

                idLookup[key] = index;
                idSearch.Entries[i] = new SearchEntry<string>(key, key);
            }

            for (int i = 0; i < tagCollection.Length; i++)
            {
                string key = tagCollection[i];
                int index = i + 1;

                tagLookup[key] = index;
                tagSearch.Entries[i] = new SearchEntry<string>(key, key);
            }

            Debug.Log($"Actor database build successfull!");
        }

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

        internal static bool TryGetAnyActor(ActorID id, out Actor actor)
        {
            actor = null;

            if (!id.IsValid)
            {
                return false;
            }

            if (!database.TryGetValue(id, out List<ActorEntry> entries))
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

            foreach (List<ActorEntry> entries in database.Values)
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
            actors = null;

            if (!id.IsValid)
            {
                return false;
            }

            if (!database.TryGetValue(id, out List<ActorEntry> entries))
            {
#if UNITY_EDITOR
                Debug.LogError($"id not found in database: [{id.Key}]");
#endif
                return false;
            }

            actors = entries;
            return true;
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

            foreach (List<ActorEntry> entries in database.Values)
            {
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

            if (!id.IsValid)
            {
                return;
            }

#if UNITY_EDITOR
            if (database.TryGetValue(id, out List<ActorEntry> entries))
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

            if (!database.TryGetValue(id, out entries))
            {
                entries = new();
                database.Add(id, entries);
            }

            entries.Add(new(actor));
        }
        internal static void RemoveActor(Actor actor)
        {
            if (actor == null)
            {
                throw new ArgumentNullException(nameof(actor));
            }

            if (!database.TryGetValue(actor.ID, out List<ActorEntry> entries))
            {
                Debug.LogError($"You are trying to remove invalid actor! [{actor.ID}]");
                return;
            };

            entries.Remove(new(actor));
        }
    }
}