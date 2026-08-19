using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Faction
{
    public static class FactionDatabase
    {
        private static readonly Dictionary<string, int> idLookup = new();
        private static FactionDefinition[] definitions = Array.Empty<FactionDefinition>();
        private static FactionInstance[] instances = Array.Empty<FactionInstance>();

        internal static void Build(FactionEntry[] entries)
        {
            if (entries == null)
            {
                return;
            }

            idLookup.Clear();
            definitions = new FactionDefinition[entries.Length];
            instances = new FactionInstance[entries.Length];

            for (int i = 0; i < entries.Length; i++)
            {
                FactionEntry entry = entries[i];
                string key = entry.ID.Key;

                idLookup[key] = i;
                definitions[i] = new(entry); 
            }

            for (int i = 0; i < definitions.Length; i++)
            {
                instances[i] = new(definitions[i].ID, definitions[i].Relations, definitions.Length);
            } 

            Debug.Log($"Faction database build successfull!");
        }

        public static FactionInstance[] Export()
        {
            FactionInstance[] copy = new FactionInstance[instances.Length];

            for (int i = 0; i < instances.Length; i++)
            {
                copy[i] = new(instances[i]);
            }

            return copy;
        }
        public static void Import(FactionInstance[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            instances = new FactionInstance[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                instances[i] = new(data[i]);
            }
        }

        public static int GetIDIndex(string key) => idLookup.TryGetValue(key, out int index) ? index : -1;
        private static FactionInstance GetInstance(FactionID id)
        {
            if (!id.IsValid || id.Index >= instances.Length)
            {
                throw new NullReferenceException($"Faction [{id}] not found!?");
            }

            return instances[id.Index];
        }
        public static IReadOnlyList<FactionDefinition> GetDefinitions() => definitions;
        public static FactionDefinition GetDefinition(int index)
        {
            if (index >= definitions.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Faction not found index out of range");
            }

            return definitions[index];
        }
        public static FactionDefinition GetDefinition(FactionID id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentNullException($"Faction id [{nameof(id)}] is not valid!");
            }

            return GetDefinition(id.Index);
        }

        public static FactionAttitude EvaluateAttitude(int relation)
        {
            if (relation <= -50) return FactionAttitude.ENEMY;
            if (relation <= -10) return FactionAttitude.HOSTILE;
            if (relation < 10) return FactionAttitude.NEUTRAL;
            if (relation < 50) return FactionAttitude.FRIENDLY;

            return FactionAttitude.ALLY;
        }
        public static FactionAttitude GetAttitudeDefault(FactionID a, FactionID b) => GetDefinition(a).GetAttitude(b);
        public static FactionAttitude GetAttitude(FactionID a, FactionID b) => GetInstance(a).GetAttitude(b);
        public static int GetRelationDefault(FactionID a, FactionID b) => GetDefinition(a).GetRelation(b);
        public static int GetRelation(FactionID a, FactionID b) => GetInstance(a).GetRelation(b);
        public static void SetRelation(FactionID a, FactionID b, int value) => GetInstance(a).SetRelation(GetInstance(b), value);
        public static void AddRelation(FactionID a, FactionID b, int value) => GetInstance(a).AddRelation(GetInstance(b), value);
    }
}