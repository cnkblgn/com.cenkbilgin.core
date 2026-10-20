using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Effect
{
    using static CoreUtility;
        
    public sealed class EffectContainer
    {
        public event Action<EffectContext> OnChanged;

        private readonly SwapBackArray<EffectInstance> effects;

        public EffectContainer(uint capacity = 16) : this(new SwapBackArray<EffectInstance>(capacity)) { }
        public EffectContainer(EffectContainer container) : this(container.effects) { }
        public EffectContainer(SwapBackArray<EffectInstance> traits)
        {
            this.effects = new((uint)traits.Capacity);

            for (int i = 0; i < traits.Count; i++)
            {
                this.effects[i] = traits[i];
            }
        }

        public IReadOnlyList<EffectInstance> GetEffects() => effects;

        private void SetState(EffectState state, EffectInstance instance) => OnChanged?.Invoke(new(state, instance));

        public void Tick(GameObject actor, float deltaTime)
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

        public bool TryAddEffect(GameObject actor, EffectInstance instance)
        {
            if (!instance.ID.IsInCompatibleWith(GetEffects(), actor))
            {
                return false;
            }

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
        public bool TryRemoveEffect(GameObject actor, EffectID id)
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
        
        private void TickAction(GameObject actor, EffectDefinition definition, ref EffectInstance instance)
        {
            EffectAction[] actions = definition.Actions;

            for (int i = 0; i < actions.Length; i++)
            {
                actions[i].Tick(actor, ref instance);
            }
        }
        private void ApplyAction(GameObject actor, EffectDefinition definition, ref EffectInstance instance)
        {
            EffectAction[] actions = definition.Actions;

            for (int i = 0; i < actions.Length; i++)
            {
                actions[i].Apply(actor, ref instance);
            }
        }
        private void RemoveAction(GameObject actor, EffectDefinition definition, ref EffectInstance instance)
        {
            EffectAction[] actions = definition.Actions;

            for (int i = 0; i < actions.Length; i++)
            {
                actions[i].Removed(actor, ref instance);
            }
        }
        private void RemoveEffect(GameObject actor, int index, ref EffectInstance instance)
        {
            SetState(EffectState.REMOVED, instance);

            RemoveAction(actor, instance.ID.GetDefinition(), ref instance);

            effects.RemoveAt(index);
        }
    }
}
