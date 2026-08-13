using Core.Actors;
using System;
using System.ComponentModel;
using System.Linq;

namespace Core.Trait
{
    using static CoreUtility;

    public sealed class TraitContainer
    {
        public event Action<TraitContext> OnChanged = null;

        private readonly SwapBackArray<TraitInstance> traits;

        public TraitContainer(uint capacity = 16) : this(new SwapBackArray<TraitInstance>(capacity)) { }
        public TraitContainer(TraitContainer container) : this(container.traits) { }
        public TraitContainer(SwapBackArray<TraitInstance> traits)
        {
            this.traits = new((uint)traits.Capacity);

            for (int i = 0; i < traits.Count; i++)
            {
                this.traits[i] = traits[i];
            }
        }

        private void SetState(TraitState state, TraitInstance instance) => OnChanged?.Invoke(new(state, instance));

        public bool HasTrait(TraitID id)
        {
            if (!id.IsValid)
            {
                return false;
            }

            for (int i = 0; i < traits.Count; i++)
            {
                if (traits[i].ID == id)
                {
                    return true;
                }
            }

            return false;
        }
        public bool IsCompatibleWith(TraitID id)
        {
            for (int i = 0; i < traits.Count; i++)
            {
                if (traits[i].ID.IsCompatibleWith(id))
                {
                    return true;
                }
            }

            return false;
        }
        public bool IsIncompatibleWith(TraitID id)
        {
            for (int i = 0; i < traits.Count; i++)
            {
                if (traits[i].ID.IsIncompatibleWith(id))
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryAddTrait(TraitID id, Actor actor)
        {
            if (HasTrait(id))
            {
                return false;
            }

            if (!IsCompatibleWith(id))
            {
                return false;
            }

            TraitInstance instance = id.CreateInstance();

            traits.Add(instance);

            ref TraitInstance added = ref traits.GetRef(traits.Count - 1);

            ApplyAction(actor, ref added);

            SetState(TraitState.ADDED, added);

            return true;
        }
        public bool TryRemoveTrait(TraitID id, Actor actor)
        {
            for (int i = 0; i < traits.Count; i++)
            {
                ref TraitInstance registered = ref traits.GetRef(i);

                if (registered.ID == id)
                {
                    RemoveTrait(actor, i, ref registered);
                    return true;
                }
            }

            return false;
        }

        private void ApplyAction(Actor actor, ref TraitInstance instance)
        {
            TraitAction[] actions = instance.ID.GetDefinition().Actions;

            for (int i = 0; i < actions.Length; i++)
            {
                actions[i].Apply(actor, ref instance);
            }
        }
        private void RemoveAction(Actor actor, ref TraitInstance instance)
        {
            TraitAction[] actions = instance.ID.GetDefinition().Actions;

            for (int i = 0; i < actions.Length; i++)
            {
                actions[i].Remove(actor, ref instance);
            }
        }
        private void RemoveTrait(Actor actor, int index, ref TraitInstance instance)
        {
            SetState(TraitState.REMOVED, instance);

            RemoveAction(actor, ref instance);

            traits.RemoveAt(index);
        }
    }
}