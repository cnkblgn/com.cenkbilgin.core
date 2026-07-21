using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Stat
{
    public sealed class StatContainer
    {
        public event Action<StatContext> OnChanged;

        private readonly Dictionary<StatID, float> stats;
        private readonly List<StatModifier> modifiers;
        private StatState state;

        public StatContainer() : this(new(), new()) 
        {
            stats = new Dictionary<StatID, float>();

            foreach (StatDefinition definition in StatDatabase.GetDatabase())
            {
                stats[definition.ID] = definition.Default;
            }
        }
        public StatContainer(StatContainer container) : this(container == null ? throw new ArgumentNullException() : container.stats, container.modifiers) { }
        public StatContainer(Dictionary<StatID, float> stats, List<StatModifier> modifiers)
        {
            if (stats == null) throw new ArgumentNullException();
            if (modifiers == null) throw new ArgumentNullException();

            this.stats = new(stats);
            this.modifiers = new(modifiers);
        }

        private void SetState(StatState state, StatModifier modifier)
        {
            this.state = state;
            OnChanged?.Invoke(new(this.state, modifier));
        }
        private void SetState(StatState state) => SetState(state, StatModifier.Empty);

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