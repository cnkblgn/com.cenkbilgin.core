using System;
using UnityEngine;

namespace Core.Faction
{
    [Serializable]
    public struct FactionRelation
    {
#if UNITY_EDITOR
        [HideInInspector] public string Name;
#endif
        public FactionID ID;
        [Range(-100, 100)] public int Relation;

        public FactionRelation(FactionRelation relation) : this(relation.ID, relation.Relation) { }
        public FactionRelation(FactionID id, int relation)
        {
            ID = id;
            Relation = relation;

            Name = ID.Key;
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            string operation = Relation >= 0 ? "+" : "";
            string relation = $"{operation}{Relation}";
            Name = $"{ID.Key} [{relation}]";
        }

        public static FactionRelation[] GetNormalized(FactionRelation[] current)
        {
            if (current == null)
            {
                Debug.LogError($"{nameof(FactionRelation)} normalized failed! current reference is missing!?");
                return null;
            }

            int count = FactionDatabase.GetDefinitions().Count;

            FactionRelation[] entries = new FactionRelation[count];

            for (int i = 0; i < current.Length; i++)
            {
                FactionRelation entry = current[i];

                if (!entry.ID.IsValid)
                {
                    continue;
                }

                int index = entry.ID.Index;

                if ((uint)index >= (uint)entries.Length)
                {
                    continue;
                }

                if (entries[index].ID.IsValid)
                {
                    Debug.LogError($"Duplicate {nameof(FactionRelation)} [{entry.ID}] detected!");
                    continue;
                }

                entries[index] = entry;
            }

            for (int i = 0; i < entries.Length; i++)
            {
                if (!entries[i].ID.IsValid)
                {
                    entries[i] = new(FactionDatabase.GetDefinition(i).ID, 0);
                }
            }

            return entries;
        }
#endif
    }
}
