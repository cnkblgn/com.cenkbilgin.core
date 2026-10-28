using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Faction
{
    public sealed class FactionContainer
    {
        public event Action<FactionContext> OnChanged;

        private FactionID id;
        private readonly List<IFactionHandler> handlers;
        private readonly FactionRelation[] relations;

        public FactionContainer(FactionContainer container, List<IFactionHandler> handlers = null) : this(container == null ? throw new ArgumentNullException(nameof(container)) : container.id, (FactionRelation[])container.relations.Clone(), handlers) { }
        public FactionContainer(FactionID id, List<IFactionHandler> handlers = null) : this(id, id.IsValid ? id.GetDefinition().Relations : new FactionRelation[0], handlers) { }
        public FactionContainer(FactionID id, FactionRelation[] relations, List<IFactionHandler> handlers = null)
        {
            Debug.Log("FACTION CTOR: " + id);

            this.id = id;
            this.relations = new FactionRelation[FactionDatabase.GetDefinitions().Count];
            this.handlers = handlers == null ? new() : new(handlers);

            if (relations == null)
            {
                return;
            }

            TrySetRelations(relations);
        }

        private void NotifyHandlers(in FactionContext ctx)
        {
            for (int i = 0; i < handlers.Count; i++)
            {
                handlers[i]?.HandleFactionChanged(in ctx);
            }
        }
        public void AddHandler(IFactionHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (handlers.Contains(handler))
            {
                return;
            }

            handlers.Add(handler);
        }
        public void RemoveHandler(IFactionHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (!handlers.Contains(handler))
            {
                return;
            }

            handlers.Remove(handler);
        }
        private void SetState()
        {
            FactionContext ctx = new(id, relations);
            NotifyHandlers(in ctx);
            OnChanged?.Invoke(ctx);
        }

        public FactionID GetFaction() => id;
        public bool TrySetFaction<TContext>(FactionID id, TContext context)
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

            RemoveAction(this.id, context);

            this.id = id;

            ClearRelations();

            SetState();

            ApplyAction(id, context);

            return true;
        }

        private void ApplyAction<TContext>(FactionID id, TContext context)
        {
            FactionAction[] actions = id.GetDefinition().Actions;

            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] is FactionAction<TContext> action)
                {
                    action.Apply(id, context);
                }
            }
        }
        private void RemoveAction<TContext>(FactionID id, TContext context)
        {
            FactionAction[] actions = id.GetDefinition().Actions;

            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] is FactionAction<TContext> action)
                {
                    action.Remove(id, context);
                }
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
                Debug.Log("not vlaid!!'^1!: " + id);

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