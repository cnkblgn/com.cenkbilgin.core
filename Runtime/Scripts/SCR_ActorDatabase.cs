using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public static class ActorDatabase
    {
        internal static bool IsParsed => tagDatabase != null;

        private static string[] idKeys = Array.Empty<string>();
        private static string[] tagKeys = Array.Empty<string>();
        private static Dictionary<string, int> tagDatabase = null;
        private static readonly Dictionary<ActorID, Actor> actorDatabase = new();

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

                tagKeys[i] = key;
                tagDatabase[key] = i;
            }

            Debug.Log($"Actor database build successfull!");
        }

        internal static bool TryGetActor(ActorID id, out Actor entity)
        {
            entity = null;

            if (!id.IsValid)
            {
                return false;
            }

            if (!actorDatabase.TryGetValue(id, out entity))
            {
                Debug.LogError($"id not found in database: [{id}]");
                return false;
            }

            return true;
        }
        internal static void RegisterActor(ActorID id, Actor entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (!id.IsValid)
            {
                return;
            }

#if UNITY_EDITOR
            if (actorDatabase.ContainsKey(id))
            {
                Debug.LogWarning($"you are trying to register duplicate entity with [{id.Key}] id", entity);
            }
#endif
            actorDatabase[id] = entity;
        }
        internal static void RemoveActor(ActorID id)
        {
            if (!id.IsValid)
            {
                return;
            }

            if (!actorDatabase.ContainsKey(id))
            {
                Debug.LogWarning($"you are trying to remove invalid [{id.Key}] id");
            }

            actorDatabase.Remove(id);
        }
    }
}