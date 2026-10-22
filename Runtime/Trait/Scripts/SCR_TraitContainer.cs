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

        public TraitContainer(uint capacity = 16) : this(new SwapBackArray<TraitInstance>(capacity)) { }
        public TraitContainer(TraitContainer container) : this(container == null ? throw new ArgumentNullException(nameof(container)) : container.traits) { }
        public TraitContainer(SwapBackArray<TraitInstance> traits)
        {
            if (traits == null)
            {
                throw new ArgumentNullException(nameof(traits));
            }

            this.traits = new((uint)traits.Capacity);

            for (int i = 0; i < traits.Count; i++)
            {
                this.traits[i] = traits[i];
            }
        }

        private void SetState(TraitState state, TraitInstance instance) => OnChanged?.Invoke(new(state, instance));

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

        public bool TryAddTrait(TraitID id, object actor)
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

            if (!id.IsCompatibleWith(GetTraits(), actor))
            {
                return false;
            }

            TraitInstance instance = id.CreateInstance();

            traits.Add(instance);

            ref TraitInstance added = ref traits.GetRef(traits.Count - 1);

            ApplyAction(ref added, actor);

            SetState(TraitState.ADDED, added);

            return true;
        }
        public bool TryRemoveTrait(TraitID id, object actor)
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
                    RemoveTrait(i, actor);
                    return true;
                }
            }

            return false;
        }

        private void ApplyAction(ref TraitInstance instance, object actor)
        {
            TraitAction[] actions = instance.ID.GetDefinition().Actions;

            for (int i = 0; i < actions.Length; i++)
            {
                actions[i].Apply(ref instance, actor);
            }
        }
        private void RemoveAction(ref TraitInstance instance, object actor)
        {
            TraitAction[] actions = instance.ID.GetDefinition().Actions;

            for (int i = 0; i < actions.Length; i++)
            {
                actions[i].Remove(ref instance, actor);
            }
        }
        private void RemoveTrait(int index, object actor)
        {
            TraitInstance removed = traits[index];

            RemoveAction(ref removed, actor);

            traits.RemoveAt(index);

            SetState(TraitState.REMOVED, removed);
        }
    }
}