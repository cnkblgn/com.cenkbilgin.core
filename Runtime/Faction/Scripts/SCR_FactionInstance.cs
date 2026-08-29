using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Faction
{
    public sealed class FactionInstance
    {
        public readonly FactionID ID;
        private readonly FactionRelation[] relations;

        internal FactionInstance(FactionID id, FactionRelation[] source, int factionCount)
        {
            ID = id;

            relations = new FactionRelation[factionCount];

            if (source != null)
            {
                for (int i = 0; i < source.Length; i++)
                {
                    FactionRelation relation = source[i];

                    if (!relation.ID.IsValid || relation.ID.Index >= relations.Length)
                    {
                        continue;
                    }

                    relations[relation.ID.Index] = new(relation);
                }
            }
        }
        internal FactionInstance(FactionInstance instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            ID = instance.ID;
            relations = (FactionRelation[])instance.relations.Clone();
        }

        internal FactionAttitude GetAttitude(FactionID target) => FactionDatabase.EvaluateAttitude(GetRelation(target));
        internal int GetRelation(FactionID target)
        {
            if (!target.IsValid || target.Index >= relations.Length)
            {
                return 0;
            }

            return relations[target.Index].Relation;
        }
        internal IReadOnlyList<FactionRelation> GetRelations() => relations;

        internal void SetRelation(FactionInstance target, int value)
        {
            if (target == null)
            {
                Debug.LogError("Set relation failed! Target faction null!?");
                return;
            }

            if (ID == target.ID)
            {
                return;
            }

            value = Mathf.Clamp(value, -100, 100);

            relations[target.ID.Index].Relation = value;
            target.relations[ID.Index].Relation = value;
        }
        internal void AddRelation(FactionInstance target, int value)
        {
            if (target == null)
            {
                Debug.LogError("Add relation failed! Target faction null!?");
                return;
            }

            SetRelation(target, GetRelation(target.ID) + value);
        }
    }
}