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

        public void Tick(Actor actor, float deltaTime)
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

                        TickAction(actor, definition, ref registered);
                    }
                }

                if (registered.TimeRemaining <= 0 && registered.Duration > 0)
                {
                    RemoveEffect(actor, i, ref registered);
                }
            }
        }

        public bool TryAddEffect(Actor actor, EffectInstance instance)
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

            ApplyAction(actor, added.ID.GetDefinition(), ref added);

            SetState(EffectState.ADDED, added);

            return true;
        }
        public bool TryRemoveEffect(Actor actor, EffectID id)
        {
            for (int i = 0; i < effects.Count; i++)
            {
                ref EffectInstance registered = ref effects.GetRef(i);

                if (registered.ID == id)
                {
                    RemoveEffect(actor, i, ref registered);
                    return true;
                }
            }

            return false;
        }
        
        private void TickAction(Actor actor, EffectDefinition definition, ref EffectInstance instance)
        {
            EffectAction[] actions = definition.Actions;

            for (int i = 0; i < actions.Length; i++)
            {
                actions[i].Tick(actor, ref instance);
            }
        }
        private void ApplyAction(Actor actor, EffectDefinition definition, ref EffectInstance instance)
        {
            EffectAction[] actions = definition.Actions;

            for (int i = 0; i < actions.Length; i++)
            {
                actions[i].Apply(actor, ref instance);
            }
        }
        private void RemoveAction(Actor actor, EffectDefinition definition, ref EffectInstance instance)
        {
            EffectAction[] actions = definition.Actions;

            for (int i = 0; i < actions.Length; i++)
            {
                actions[i].Removed(actor, ref instance);
            }
        }
        private void RemoveEffect(Actor actor, int index, ref EffectInstance instance)
        {
            SetState(EffectState.REMOVED, instance);

            RemoveAction(actor, instance.ID.GetDefinition(), ref instance);

            effects.RemoveAt(index);
        }
    }
}
