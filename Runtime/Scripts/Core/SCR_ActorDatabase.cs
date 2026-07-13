using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    using static CoreUtility;

    public static class ActorDatabase
    {
        internal static bool IsParsed => tagDatabase != null;

        private static string[] idKeys = Array.Empty<string>();
        private static string[] tagKeys = Array.Empty<string>();
        private static Dictionary<string, int> tagDatabase = null;
        private static readonly Dictionary<ActorID, List<ActorEntry>> actorDatabase = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnRuntimeInitialize() => actorDatabase.Clear();

        public static IReadOnlyList<string> GetTags() => tagKeys;
        public static IReadOnlyList<string> GetIDs() => idKeys;
        public static int GetTagIndex(string id)
        {
            if (!IsParsed)
            {
                Debug.LogError($"Actor database is not parsed! Please build via ActorDatabaseConfig");
                return -1;
            }

            if (tagDatabase.TryGetValue(id, out int a))
            {
                return a;
            }

            return -1;
        }

        internal static void Build(string[] ids, string[] tags)
        {
            if (tags == null)
            {
                return;
            }

            idKeys = new string[ids.Length];
            tagKeys = new string[tags.Length];
            tagDatabase = new(tags.Length);

            for (int i = 0; i < ids.Length; i++)
            {
                idKeys[i] = ids[i];
            }

            for (int i = 0; i < tags.Length; i++)
            {
                string key = tags[i];
                int index = i + 1;

                tagKeys[i] = key;
                tagDatabase[key] = index;
            }

            Debug.Log($"Actor database build successfull!");
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
                Debug.LogError($"id not found in database: [{id}]");
                return false;
            }

            if (entries.Count <= 0)
            {
                return false;
            }

            actor = entries[0].Actor;
            return true;
        }
        internal static bool TryGetAnyActor(ActorTag tag, out Actor actor)
        {
            actor = null;

            if (!tag.IsValid)
            {
                return false;
            }

            for (int i = 0; i < idKeys.Length; i++)
            {
                ActorID id = new(idKeys[i]);

                if (TryGetAllActors(id, out IReadOnlyList<ActorEntry> entries))
                {
                    for (int j = 0; j < entries.Count; j++)
                    {
                        Actor tempActor = entries[i].Actor;

                        if (tempActor.HasAny(tag))
                        {
                            actor = tempActor;
                            return true;
                        }
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
                Debug.LogError($"id not found in database: [{id}]");
                return false;
            }

            actors = entries;
            return true;
        }
        internal static bool TryGetAllActors(ActorTag tag, out List<Actor> actors)
        {
            actors = new();

            if (!tag.IsValid)
            {
                return false;
            }

            for (int i = 0; i < idKeys.Length; i++)
            {
                ActorID id = new(idKeys[i]);

                if (TryGetAllActors(id, out IReadOnlyList<ActorEntry> entries))
                {
                    for (int j = 0; j < entries.Count; j++)
                    {
                        Actor tempActor = entries[i].Actor;

                        if (tempActor.HasAny(tag))
                        {
                            actors.Add(tempActor);
                        }
                    }
                }
            }

            return true;
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

            actorDatabase[id].Add(new(actor));
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