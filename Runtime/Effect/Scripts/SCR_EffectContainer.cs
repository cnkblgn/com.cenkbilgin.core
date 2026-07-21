using System;
using Core.Actors;

namespace Core.Effect
{
    using static CoreUtility;
        
    public sealed class EffectContainer
    {
        public event Action<EffectContext> OnChanged;

        private readonly SwapBackArray<EffectInstance> effects;

        public EffectContainer(uint capacity = 16) => effects = new SwapBackArray<EffectInstance>(capacity);

        private void SetState(EffectState state, EffectInstance instance) => OnChanged?.Invoke(new(state, instance));

        public void Tick(Actor entity, float deltaTime)
        {
            for (int i = effects.Count - 1; i >= 0; i--)
            {
                ref EffectInstance registered = ref effects.GetRef(i);
                EffectDefinition definition = registered.ID.GetDefinition();

                registered.TimeRemaining -= deltaTime;

                if (registered.TickInterval > 0)
                {
                    registered.TickTimer += deltaTime;

                    while (registered.TickTimer >= registered.TickInterval)
                    {
                        registered.TickTimer -= registered.TickInterval;

                        registered.TickCount++;

                        definition.Action.Tick(entity, ref registered);
                    }
                }

                if (registered.TimeRemaining <= 0 && registered.Duration > 0)
                {
                    RemoveEffect(entity, i, registered);
                }
            }
        }

        public bool TryAddEffect(Actor entity, EffectInstance instance)
        {
            for (int i = 0; i < effects.Count; i++)
            {
                ref EffectInstance registered = ref effects.GetRef(i);

                if (registered.ID == instance.ID)
                {
                    return false;
                }
            }

            effects.Add(instance);

            ref EffectInstance added = ref effects.GetRef(effects.Count - 1);

            added.ID.GetDefinition().Action.Apply(entity, ref added);

            SetState(EffectState.ADDED, added);

            return true;
        }
        public bool TryRemoveEffect(Actor entity, EffectID id)
        {
            for (int i = 0; i < effects.Count; i++)
            {
                ref EffectInstance registered = ref effects.GetRef(i);

                if (registered.ID == id)
                {
                    RemoveEffect(entity, i, registered);
                    return true;
                }
            }

            return false;
        }

        private void RemoveEffect(Actor entity, int index, EffectInstance registered)
        {
            SetState(EffectState.REMOVED, registered);

            registered.ID.GetDefinition().Action.Removed(entity, ref registered);

            effects.RemoveAt(index);
        }
    }
}
