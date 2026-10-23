using System;
using UnityEngine;

namespace Core.Scenario
{
    [Serializable]
    public struct ScenarioID : IEquatable<ScenarioID>
    {
        public readonly string Key => key;
        public bool IsValid => !string.IsNullOrEmpty(key) && Index >= 0;
        public int Index
        {
            get
            {
                if (resolved)
                {
                    return index;
                }

                index = ScenarioDatabase.GetIDIndex(key);

                resolved = index >= 0;

                return index;
            }
        }

        [SerializeField, Required] private string key;
        [NonSerialized] private int index;
        [NonSerialized] private bool resolved;

        public ScenarioID(string key, int index) => (this.key, this.index, this.resolved) = (key, index, index >= 0);
        public ScenarioID(string key) : this(key, -1) { }

        public override string ToString() => $"Key: {key} << Index: {Index}";

        public readonly override int GetHashCode() => key?.GetHashCode() ?? 0;
        public readonly override bool Equals(object obj) => obj is ScenarioID other && Equals(other);
        public readonly bool Equals(ScenarioID other) => string.Equals(key, other.key, StringComparison.Ordinal);
        public static bool operator ==(ScenarioID left, ScenarioID right) => left.Equals(right);
        public static bool operator !=(ScenarioID left, ScenarioID right) => !left.Equals(right);

        public readonly ScenarioDefinition GetDefinition() => ScenarioDatabase.GetDefinition(this);
        public readonly bool IsCompatibleWith(object context)
        {
            if (context == null)
            {
                Debug.LogError("Scenario compatibility check failed! context is null!?");
                return false;
            }

            ScenarioCompatibility[] compatibilities = GetDefinition().Compatibilities;

            for (int i = 0; i < compatibilities.Length; i++)
            {
                if (!compatibilities[i].IsCompatibleWith(this, context))
                {
                    return false;
                }
            }

            return true;
        }
        public readonly bool IsCompatibleWith(int context)
        {
            ScenarioCompatibility[] compatibilities = GetDefinition().Compatibilities;

            for (int i = 0; i < compatibilities.Length; i++)
            {
                if (!compatibilities[i].IsCompatibleWith(this, context))
                {
                    return false;
                }
            }

            return true;
        }
        public readonly bool IsCompatibleWith(ulong context)
        {
            ScenarioCompatibility[] compatibilities = GetDefinition().Compatibilities;

            for (int i = 0; i < compatibilities.Length; i++)
            {
                if (!compatibilities[i].IsCompatibleWith(this, context))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
