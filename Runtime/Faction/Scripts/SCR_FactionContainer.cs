using System;
using UnityEngine;

namespace Core.Faction
{
    public sealed class FactionContainer
    {
        public event Action<FactionContext> OnChanged;

        private FactionID id;
        private readonly FactionRelation[] relations;

        public FactionContainer() : this((FactionID)default) { }
        public FactionContainer(FactionContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            id = container.id;
            relations = (FactionRelation[])container.relations.Clone();
        }
        public FactionContainer(FactionID id) : this(id, id.IsValid ? id.GetDefinition().Relations : new FactionRelation[0]) { }
        public FactionContainer(FactionID id, FactionRelation[] relations)
        {
            this.id = id;
            this.relations = new FactionRelation[FactionDatabase.GetDefinitions().Count];

            if (relations == null)
            {
                return;
            }

            for (int i = 0; i < relations.Length; i++)
            {
                FactionRelation relation = relations[i];

                if (!relation.ID.IsValid || relation.ID.Index >= this.relations.Length)
                {
                    continue;
                }

                this.relations[relation.ID.Index] = new(relation);
            }
        }
          
        private void SetState() => OnChanged?.Invoke(new(id, relations));

        public FactionID GetFaction() => id;
        public bool TrySetFaction(FactionID id)
        {
            if (id == this.id)
            {
                return false;
            }

            this.id = id;
            Array.Clear(relations, 0, relations.Length);
            SetState();
            return true;
        }

        public FactionAttitude GetAttitude(FactionID id) => FactionDatabase.EvaluateAttitude(GetRelation(id));
        public int GetRelation(FactionID id)
        {
            if (!id.IsValid || id.Index >= relations.Length)
            {
                return 0;
            }

            FactionRelation relation = relations[id.Index];

            if (relation.ID.IsValid)
            {
                return relation.Relation;
            }

            return FactionDatabase.GetRelation(this.id, id);
        }
        public void SetRelation(FactionID id, int value)
        {
            if (!id.IsValid || id.Index >= relations.Length)
            {
                return;
            }

            relations[id.Index] = new(id, Mathf.Clamp(value, -100, 100));

            SetState();
        }
        public void SetRelations(FactionRelation[] relations)
        {
            if (relations == null)
            {
                throw new ArgumentNullException(nameof(relations), "Set faction relations failed! relations cannot be null!");
            }

            Array.Clear(this.relations, 0, this.relations.Length);

            for (int i = 0; i < relations.Length; i++)
            {
                FactionRelation relation = relations[i];

                if (!relation.ID.IsValid || relation.ID.Index >= this.relations.Length)
                {
                    continue;
                }

                this.relations[relation.ID.Index] = new(relation);
            }

            SetState();
        }
        public void AddRelation(FactionID id, int value) => SetRelation(id, GetRelation(id) + value);
    }
}