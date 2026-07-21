using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Actors
{
    public static class ActorDatabase
    {
        private static SearchCollection<string> ids = new(Array.Empty<SearchEntry<string>>());
        private static SearchCollection<string> tags = new(Array.Empty<SearchEntry<string>>());
        private static readonly Dictionary<string, int> tagDatabase = new();
        private static readonly Dictionary<ActorID, List<ActorEntry>> actorDatabase = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnRuntimeInitialize() => actorDatabase.Clear();

        internal static void Build(string[] idCollection, string[] tagCollection)
        {
            if (idCollection == null || tagCollection == null)
            {
                return;
            }

            tagDatabase.Clear();
            tags = new(new SearchEntry<string>[tagCollection.Length]);
            ids = new(new SearchEntry<string>[idCollection.Length]);

            for (int i = 0; i < idCollection.Length; i++)
            {
                ids.Entries[i] = new SearchEntry<string>(idCollection[i], idCollection[i]);
            }

            for (int i = 0; i < tagCollection.Length; i++)
            {
                string key = tagCollection[i];
                int index = i + 1;

                tagDatabase[key] = index;
                tags.Entries[i] = new SearchEntry<string>(key, key);
            }

            Debug.Log($"Actor database build successfull!");
        }

        public static SearchCollection<string> GetIDs() => ids;
        public static SearchCollection<string> GetTags() => tags;
        public static int GetTagIndex(string id)
        {
            if (tagDatabase.TryGetValue(id, out int a))
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

            if (!actorDatabase.TryGetValue(id, out List<ActorEntry> entries))
            {
#if UNITY_EDITOR
                Debug.LogError($"id not found in database: [{id}]");
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

            foreach (List<ActorEntry> entries in actorDatabase.Values)
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

            if (!actorDatabase.TryGetValue(id, out List<ActorEntry> entries))
            {
#if UNITY_EDITOR
                Debug.LogError($"id not found in database: [{id}]");
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

            foreach (List<ActorEntry> entries in actorDatabase.Values)
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
            if (actorDatabase.TryGetValue(id, out List<ActorEntry> entries))
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

            if (!actorDatabase.TryGetValue(id, out entries))
            {
                entries = new();
                actorDatabase.Add(id, entries);
            }

            entries.Add(new(actor));
        }
        internal static void RemoveActor(Actor actor)
        {
            if (actor == null)
            {
                throw new ArgumentNullException(nameof(actor));
            }

            if (!actorDatabase.TryGetValue(actor.ID, out List<ActorEntry> entries))
            {
                Debug.LogError($"You are trying to remove invalid actor! [{actor.ID}]");
                return;
            };

            entries.Remove(new(actor));
        }
    }
}