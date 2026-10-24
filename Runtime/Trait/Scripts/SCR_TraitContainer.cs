using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Trait
{
    using static CoreUtility;

    public sealed class TraitContainer
    {
        public event Action<TraitContext> OnChanged = null;

        private readonly SwapBackArray<TraitInstance> traits;
        private readonly ITraitUser user;

        public TraitContainer(uint capacity = 16, ITraitUser user = null) : this(new SwapBackArray<TraitInstance>(capacity), user) { }
        public TraitContainer(TraitContainer container) : this(container == null ? throw new ArgumentNullException(nameof(container)) : container.traits, container.user) { }
        public TraitContainer(SwapBackArray<TraitInstance> traits, ITraitUser user = null)
        {
            if (traits == null)
            {
                throw new ArgumentNullException(nameof(traits));
            }

            this.user = user;
            this.traits = new((uint)traits.Capacity);

            for (int i = 0; i < traits.Count; i++)
            {
                this.traits[i] = traits[i];
            }
        }

        private void SetState(TraitState state, TraitInstance instance)
        {
            TraitContext ctx = new(state, instance);
            user?.HandleTraitStateChanged(ctx);
            OnChanged?.Invoke(ctx);
        }

        public IReadOnlyList<TraitInstance> GetTraits() => traits;

        public bool HasTrait(TraitID id)
        {
            for (int i = 0; i < traits.Count; i++)
            {
                if (traits[i].ID == id)
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryAddTrait<TContext>(TraitID id, TContext context)
        {
            if (!id.IsValid)
            {
                Debug.LogError($"Trait add failed! {id} is not valid!");
                return false;
            }

            if (HasTrait(id))
            {
                return false;
            }

            AddTrait(id, context);
            return true;
        }
        private void AddTrait<TContext>(TraitID id, TContext context)
        {
            TraitInstance instance = id.CreateInstance();

            traits.Add(instance);

            ref TraitInstance added = ref traits.GetRef(traits.Count - 1);

            ApplyAction(ref added, context);

            SetState(TraitState.ADDED, added);
        }
        public bool TryRemoveTrait<TContext>(TraitID id, TContext context)
        {
            if (!id.IsValid)
            {
                Debug.LogError($"Remove trait failed! {id} is not valid!");
                return false;
            }

            for (int i = 0; i < traits.Count; i++)
            {
                ref TraitInstance registered = ref traits.GetRef(i);

                if (registered.ID == id)
                {
                    RemoveTrait(i, context);
                    return true;
                }
            }

            return false;
        }
        private void RemoveTrait<TContext>(int index, TContext context)
        {
            ref TraitInstance removed = ref traits.GetRef(index);

            RemoveAction(ref removed, context);

            traits.RemoveAt(index);

            SetState(TraitState.REMOVED, removed);
        }
        private void ApplyAction<TContext>(ref TraitInstance instance, TContext context)
        {
            TraitAction[] actions = instance.ID.GetDefinition().Actions;

            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] is TraitAction<TContext> action)
                {
                    action.Apply(ref instance, context);
                }
            }
        }
        private void RemoveAction<TContext>(ref TraitInstance instance, TContext context)
        {
            TraitAction[] actions = instance.ID.GetDefinition().Actions;

            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] is TraitAction<TContext> action)
                {
                    action.Remove(ref instance, context);
                }
            }
        }
    }
}