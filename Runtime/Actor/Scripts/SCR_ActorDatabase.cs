using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Actors
{
    public static class ActorDatabase
    {
        private static readonly Dictionary<string, int> idLookup = new();
        private static readonly Dictionary<string, int> tagLookup = new();
        private static ActorTag[] tags = Array.Empty<ActorTag>();
        private static ActorGroup[] groups = Array.Empty<ActorGroup>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnRuntimeInitialize()
        {
            for (int i = 0; i < groups.Length; i++)
            {
                groups[i].Group.Clear();
            }
        }

        internal static void Build(string[] ids, string[] _tags)
        {
            if (ids == null || _tags == null)
            {
                return;
            }

            idLookup.Clear();
            tagLookup.Clear();
            tags = new ActorTag[_tags.Length + 1];
            groups = new ActorGroup[ids.Length];

            tagLookup["GENERIC"] = 0;
            tags[0] = new("GENERIC", 0);

            for (int i = 0; i < ids.Length; i++)
            {
                string key = ids[i];
                int index = i;

                if (string.IsNullOrEmpty(key))
                {
                    Debug.LogError("Actor database id key is invalid!?");
                    continue;
                }

                idLookup[key] = index;
                groups[i] = new(new(key, index));
            }

            for (int i = 0; i < _tags.Length; i++)
            {
                string key = _tags[i];
                int index = i + 1;

                if (string.IsNullOrEmpty(key))
                {
                    Debug.LogError("Actor database tag key is invalid!?");
                    continue;
                }

                tagLookup[key] = index;
                tags[index] = new(key, index);
            }

            Debug.Log($"Actor database build successfull!");
        }

        public static IReadOnlyList<ActorTag> GetTags() => tags;
        public static IReadOnlyList<ActorGroup> GetGroups() => groups;
        private static List<ActorEntry> GetEntries(int index)
        {
            if (index >= groups.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Actor entries not found index out of range");
            }

            return groups[index].Group;
        }
        private static List<ActorEntry> GetEntries(ActorID id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentNullException(nameof(id), $"Actor id [{id.Key}] is not valid!");
            }

            return GetEntries(id.Index);
        }
        public static ActorID GetID(int index)
        {
            if (index >= groups.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Actor id not found index out of range");
            }

            return groups[index].ID;
        }
        public static int GetIDIndex(string key) => idLookup.TryGetValue(key, out int index) ? index : 1;
        public static ActorTag GetTag(int index)
        {
            if (index >= tags.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Actor tag not found index out of range");
            }

            return tags[index];
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

            for (int i = 0; i < groups.Length; i++)
            {
                List<ActorEntry> entries = groups[i].Group;

                for (int j = 0; j < entries.Count; j++)
                {
                    Actor tempActor = entries[j].Actor;

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

            for (int i = 0; i < groups.Length; i++)
            {
                List<ActorEntry> entries = groups[i].Group;

                for (int j = 0; j < entries.Count; j++)
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
#endif

            entries.Add(new(actor));
        }
        internal static void RemoveActor(Actor actor)
        {
            if (actor == null)
            {
                throw new ArgumentNullException(nameof(actor));
            }

            List<ActorEntry> entries = GetEntries(actor.ID);

            if (entries.Count == 0)
            {
                Debug.LogError($"You are trying to remove invalid actor! [{actor.ID}]");
                return;
            }

            entries.Remove(new(actor));
        }
    }
}