using System;
using System.Collections.Generic;
using Core.Actors;

namespace Core.Trait
{
    public sealed class TraitContainer
    {
        public event Action<TraitContext> OnChanged = null;

        private readonly Dictionary<TraitID, TraitInstance> traits;

        public TraitContainer() : this(new Dictionary<TraitID, TraitInstance>()) { }
        public TraitContainer(TraitContainer container) : this(container == null ? throw new ArgumentNullException() : container.traits) { }
        public TraitContainer(Dictionary<TraitID, TraitInstance> traits)
        {
            if (traits == null) throw new ArgumentNullException();

            this.traits = new(traits);
        }

        private void SetState(TraitState state, TraitInstance instance) => OnChanged?.Invoke(new(state, instance));

        public bool HasTrait(TraitID id)
        {
            if (!id.IsValid)
            {
                return false;
            }

            return traits.ContainsKey(id);
        }
        public bool IsCompatibleWith(TraitID id)
        {
            foreach (TraitID registered in traits.Keys)
            {
                if (registered.IsCompatibleWith(id))
                {
                    return true;
                }
            }

            return false;
        }
        public bool IsIncompatibleWith(TraitID id)
        {
            foreach (TraitID registered in traits.Keys)
            {
                if (registered.IsIncompatibleWith(id))
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

            traits.Add(id, instance);

            id.GetDefinition().Action.Apply(actor, ref instance);

            SetState(TraitState.ADDED, instance);

            return true;
        }
        public bool TryRemoveTrait(TraitID id, Actor actor)
        {
            if (!traits.TryGetValue(id, out TraitInstance registered))
            {
                return false;
            }

            id.GetDefinition().Action.Remove(actor, ref registered);

            traits.Remove(id);

            SetState(TraitState.REMOVED, registered);

            return true;
        }
    }
}