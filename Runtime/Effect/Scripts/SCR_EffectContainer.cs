using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Effect
{
    using static CoreUtility;

    public sealed class EffectContainer
    {
        public event Action<EffectContext> OnChanged;

        private readonly IEffectUser user;
        private readonly SwapBackArray<EffectInstance> effects;

        public EffectContainer(uint capacity = 16, IEffectUser user = null) : this(new SwapBackArray<EffectInstance>(capacity), user) { }
        public EffectContainer(EffectContainer container) : this(container == null ? throw new ArgumentNullException(nameof(container)) : container.effects, container.user) { }
        public EffectContainer(SwapBackArray<EffectInstance> effects, IEffectUser user = null)
        {
            if (effects == null)
            {
                throw new ArgumentNullException(nameof(effects));
            }

            this.user = user;
            this.effects = new((uint)effects.Capacity);

            for (int i = 0; i < effects.Count; i++)
            {
                this.effects[i] = effects[i];
            }
        }

        public IReadOnlyList<EffectInstance> GetEffects() => effects;

        private void SetState(EffectState state, EffectInstance instance)
        {
            EffectContext ctx = new(state, instance);
            user?.HandleEffectStateChanged(ctx);
            OnChanged?.Invoke(ctx);
        }

        public bool HasEffect(EffectID id)
        {
            for (int i = 0; i < effects.Count; i++)
            {
                ref EffectInstance registered = ref effects.GetRef(i);

                if (registered.ID == id)
                {
                    return true;
                }
            }

            return false;
        }

        public void Tick<TContext>(float deltaTime, TContext context)
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
        public bool TryAddEffect<TContext>(EffectInstance instance, TContext context)
        {
            if (HasEffect(instance.ID))
            {
                return false;
            }

            AddEffect(instance, context);
            return true;
        }
        private void AddEffect<TContext>(EffectInstance instance, TContext context)
        {
            effects.Add(instance);

            ref EffectInstance added = ref effects.GetRef(effects.Count - 1);

            ApplyAction(ref added, context);

            SetState(EffectState.ADDED, added);
        }
        public bool TryRemoveEffect<TContext>(EffectID id, TContext context)
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
        private void RemoveEffect<TContext>(int index, TContext context)
        {
            ref EffectInstance removed = ref effects.GetRef(index);

            RemoveAction(ref removed, context);

            effects.RemoveAt(index);

            SetState(EffectState.REMOVED, removed);
        }
        private void TickAction<TContext>(ref EffectInstance instance, TContext context)
        {
            EffectAction[] actions = instance.ID.GetDefinition().Actions;

            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] is EffectAction<TContext> action)
                {
                    action.Tick(ref instance, context);
                }
            }
        }
        private void ApplyAction<TContext>(ref EffectInstance instance, TContext context)
        {
            EffectAction[] actions = instance.ID.GetDefinition().Actions;

            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] is EffectAction<TContext> action)
                {
                    action.Apply(ref instance, context);
                }
            }
        }
        private void RemoveAction<TContext>(ref EffectInstance instance, TContext context)
        {
            EffectAction[] actions = instance.ID.GetDefinition().Actions;

            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] is EffectAction<TContext> action)
                {
                    action.Remove(ref instance, context);
                }
            }
        }
    }
}
