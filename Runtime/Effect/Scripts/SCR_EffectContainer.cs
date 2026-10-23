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
        public EffectContainer(EffectContainer container) : this(container == null ? throw new ArgumentNullException(nameof(container)) : container.effects) { }
        public EffectContainer(SwapBackArray<EffectInstance> effects)
        {
            if (effects == null)
            {
                throw new ArgumentNullException(nameof(effects));
            }

            this.effects = new((uint)effects.Capacity);

            for (int i = 0; i < effects.Count; i++)
            {
                this.effects[i] = effects[i];
            }
        }

        public IReadOnlyList<EffectInstance> GetEffects() => effects;

        private void SetState(EffectState state, EffectInstance instance) => OnChanged?.Invoke(new(state, instance));

        public void Tick(float deltaTime, object context)
        {
            for (int i = effects.Count - 1; i >= 0; i--)
            {
                ref EffectInstance registered = ref effects.GetRef(i);
    
                registered.TimeRemaining -= deltaTime;

                if (registered.TickInterval > 0)
                {
                    registered.TickTimer += deltaTime;

                    while (registered.TickTimer >= registered.TickInterval)
                    {
                        registered.TickTimer -= registered.TickInterval;

                        registered.TickCount++;

                        TickAction(ref registered, context);
                    }
                }

                if (registered.TimeRemaining <= 0 && registered.Duration > 0)
                {
                    RemoveEffect(i, context);
                }
            }
        }

        public bool TryAddEffect(EffectInstance instance, object context)
        {
            if (!instance.ID.IsCompatibleWith(context))
            {
                return false;
            }

            if (!instance.ID.IsCompatibleWith(GetEffects()))
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

            ApplyAction(ref added, context);

            SetState(EffectState.ADDED, added);

            return true;
        }
        public bool TryRemoveEffect(EffectID id, object context)
        {
            if (!id.IsValid)
            {
                Debug.LogError($"Remove effect failed! {id} is not valid!");
                return false;
            }

            for (int i = 0; i < effects.Count; i++)
            {
                ref EffectInstance registered = ref effects.GetRef(i);

                if (registered.ID == id)
                {
                    RemoveEffect(i, context);
                    return true;
                }
            }

            return false;
        }
        
        private void TickAction(ref EffectInstance instance, object context)
        {
            EffectAction[] actions = instance.ID.GetDefinition().Actions;

            for (int i = 0; i < actions.Length; i++)
            {
                actions[i].Tick(ref instance, context);
            }
        }
        private void ApplyAction(ref EffectInstance instance, object context)
        {
            EffectAction[] actions = instance.ID.GetDefinition().Actions;

            for (int i = 0; i < actions.Length; i++)
            {
                actions[i].Apply(ref instance, context);
            }
        }
        private void RemoveAction(ref EffectInstance instance, object context)
        {
            EffectAction[] actions = instance.ID.GetDefinition().Actions;

            for (int i = 0; i < actions.Length; i++)
            {
                actions[i].Removed(ref instance, context);
            }
        }
        private void RemoveEffect(int index, object context)
        {
            EffectInstance removed = effects[index];

            RemoveAction(ref removed, context);
            effects.RemoveAt(index);

            SetState(EffectState.REMOVED, removed);
        }
    }
}
