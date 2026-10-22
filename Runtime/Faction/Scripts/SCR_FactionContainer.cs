using System;
using System.Collections.Generic;
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

            TrySetRelations(relations);
        }

        private void SetState() => OnChanged?.Invoke(new(id, relations));

        public FactionID GetFaction() => id;
        public bool TrySetFaction(FactionID id, object actor)
        {
            if (id == this.id)
            {
                return false;
            }

            if (!id.IsValid)
            {
                Debug.LogError($"Faction set failed! {id} is not valid!");
                return false;
            }

            if (actor == null)
            {
                Debug.LogError("Faction set failed! actor is null!?");
                return false;
            }

            if (!id.IsCompatibleWith(actor))
            {
                return false;
            }

            RemoveAction(this.id, actor);

            this.id = id;

            ClearRelations();

            SetState();

            ApplyAction(id, actor);

            return true;
        }

        private void ApplyAction(FactionID id, object actor)
        {
            FactionAction[] actions = id.GetDefinition().Actions;

            for (int i = 0; i < actions.Length; i++)
            {
                actions[i].Apply(id, actor);
            }
        }
        private void RemoveAction(FactionID id, object actor)
        {
            FactionAction[] actions = id.GetDefinition().Actions;

            for (int i = 0; i < actions.Length; i++)
            {
                actions[i].Remove(id, actor);
            }
        }

        public FactionAttitude GetAttitude(FactionID id) => FactionDatabase.EvaluateAttitude(GetRelation(id));
        public IReadOnlyList<FactionRelation> GetRelations() => relations;
        public int GetRelation(FactionID id)
        {
            if (!id.IsValid)
            {
                Debug.LogError($"Faction get relation failed! {id} is not valid!");
                return 0;
            }

            if (id.Index >= relations.Length)
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
            if (!TrySetRelation(id, value))
            {
                return;
            }

            SetState();
        }
        private bool TrySetRelation(FactionID id, int value)
        {
            if (!id.IsValid)
            {
                Debug.LogError($"Faction set relation failed! {id} is not valid!");
                return false;
            }

            if (id.Index >= relations.Length)
            {
                return false;
            }

            relations[id.Index] = new(id, Mathf.Clamp(value, -100, 100));
            return true;
        }
        private bool TrySetRelation(FactionRelation relation) => TrySetRelation(relation.ID, relation.Relation);
        private bool TrySetRelations(FactionRelation[] relations)
        {
            if (relations == null)
            {
                throw new ArgumentNullException(nameof(relations), "Set faction relations failed! relations cannot be null!");
            }

            ClearRelations();
            bool success = false;

            for (int i = 0; i < relations.Length; i++)
            {
                if (TrySetRelation(relations[i]))
                {
                    success = true;
                }
            }

            return success;
        }
        public void AddRelation(FactionID id, int value) => SetRelation(id, GetRelation(id) + value);
        public void SetRelations(FactionRelation[] relations)
        {
            if (relations == null)
            {
                throw new ArgumentNullException(nameof(relations), "Set faction relations failed! relations cannot be null!");
            }

            TrySetRelations(relations);

            SetState();
        }
        public void ClearRelations() => Array.Clear(relations, 0, relations.Length);
    }
}