using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Stat
{
    public sealed class StatContainer
    {
        public event Action<StatContext> OnChanged;

        private StatContext ctx;
        private StatState state;
        private readonly Dictionary<StatID, float> stats;
        private readonly List<StatModifier> modifiers;
        private readonly List<IStatHandler> handlers;

        public StatContainer(List<IStatHandler> handlers = null) : this(new(), new(), handlers) 
        {
            stats = new Dictionary<StatID, float>();

            foreach (StatDefinition definition in StatDatabase.GetDefinitions())
            {
                stats[definition.ID] = definition.Default;
            }
        }
        public StatContainer(StatContainer container, List<IStatHandler> handlers = null) : this(container == null ? throw new ArgumentNullException() : container.stats, container.modifiers, handlers) { }
        public StatContainer(Dictionary<StatID, float> stats, List<StatModifier> modifiers, List<IStatHandler> handlers = null)
        {
            if (stats == null) throw new ArgumentNullException();
            if (modifiers == null) throw new ArgumentNullException();

            this.handlers = handlers == null ? new() : new(handlers);
            this.stats = new(stats);
            this.modifiers = new(modifiers);
        }

        private void NotifyHandlers(in StatContext ctx)
        {
            for (int i = 0; i < handlers.Count; i++)
            {
                handlers[i]?.HandleStatChanged(in ctx);
            }
        }
        public void AddHandler(IStatHandler handler)
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
        public void RemoveHandler(IStatHandler handler)
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

        private void SetState(StatState state, StatModifier modifier)
        {
            this.state = state;

            ctx = new(this.state, modifier);
            NotifyHandlers(in ctx);
            OnChanged?.Invoke(ctx);
        }
        private void SetState(StatState state) => SetState(state, StatModifier.Empty);

        public StatContext GetContext() => ctx;
        public IReadOnlyCollection<StatID> GetStats() => stats.Keys;
        public float GetStat(StatID id)
        {
            StatDefinition definition = id.GetDefinition();
            float @base = stats.TryGetValue(id, out float v) ? v : 0;
            float flat = 0;
            float multiply = 1;

            foreach (StatModifier modifier in modifiers)
            {
                if (modifier.StatID != id)
                {
                    continue;
                }

                switch (modifier.Operation)
                {
                    case StatModifierOperation.FLAT:
                        flat += modifier.Value;
                        break;
                    case StatModifierOperation.MULTIPLY:
                        multiply *= modifier.Value;
                        break;
                    default:
                        break;
                }
            }

            float value = (@base + flat) * multiply;

            return Mathf.Clamp(value, definition.Min, definition.Max);
        }
        public void SetStat(StatID id, float value)
        {
            if (!id.IsValid)
            {
                Debug.LogError($"Set stat failed! {id} is not valid");
                return;
            }

            float current = stats[id];

            if (current == value)
            {
                return;
            }

            stats[id] = value;

            SetState(StatState.CHANGED);
        }

        public void AddModifier(in StatModifier mod)
        {
#if UNITY_EDITOR
            for (int i = 0; i < modifiers.Count; i++)
            {
                StatModifier curMod = modifiers[i];
                StatModifierSource curSrc = curMod.Source;
                StatModifierSource tarSrc = mod.Source;

                if (curSrc.TypeID == tarSrc.TypeID && curSrc.ContextID == tarSrc.ContextID && curMod.StatID == mod.StatID && curMod.Operation == mod.Operation)
                {
                    Debug.Log($"Duplicate modifier detected! [{curMod.StatID} | {curSrc.TypeID} | {curSrc.ContextID}]");
                    return;
                }
            }
#endif

            modifiers.Add(mod);
            SetState(StatState.MODIFIER_ADDED, mod);
            SetState(StatState.CHANGED);
        }
        public void RemoveModifier(in StatModifierSource source)
        {
            for (int i = modifiers.Count - 1; i >= 0; i--)
            {
                StatModifier modifier = modifiers[i];

                if (modifier.Source.TypeID == source.TypeID && modifier.Source.ContextID == source.ContextID)
                {
                    modifiers.RemoveAt(i);
                    SetState(StatState.MODIFIER_REMOVED, modifier);
                }
            }

            SetState(StatState.CHANGED);
        }
        public void RemoveModifier(uint typeID)
        {
            for (int i = modifiers.Count - 1; i >= 0; i--)
            {
                StatModifier modifier = modifiers[i];

                if (modifier.Source.TypeID == typeID)
                {
                    modifiers.RemoveAt(i);
                    SetState(StatState.MODIFIER_REMOVED, modifier);
                }
            }

            SetState(StatState.CHANGED);
        }
        public void ClearModifiers()
        {
            for (int i = modifiers.Count - 1; i >= 0; i--)
            {
                StatModifier modifier = modifiers[i];

                modifiers.RemoveAt(i);
                SetState(StatState.MODIFIER_REMOVED, modifier);
            }

            modifiers.Clear();
            SetState(StatState.CHANGED);
        }
    }
}