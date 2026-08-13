using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Core.Trait
{
    [Serializable]
    public struct TraitID : IEquatable<TraitID>
    {
        public int Index
        {
            get
            {
                if (index < 0)
                {
                    index = TraitDatabase.GetIDIndex(key);
                }

                return index;
            }
        }
        public readonly string Key => key;
        public bool IsValid => !string.IsNullOrEmpty(key) && Index >= 0;

        [SerializeField, Required] private string key;
        [NonSerialized] private int index;

        public TraitID(string key, int index)
        {
            this.key = key;
            this.index = index;
        }

        public override string ToString() => $"Key: {key} << Index: {Index}";

        public readonly override int GetHashCode() => key?.GetHashCode() ?? 0;
        public readonly override bool Equals(object obj) => obj is TraitID other && Equals(other);
        public readonly bool Equals(TraitID other) => string.Equals(key, other.key, StringComparison.Ordinal);
        public static bool operator ==(TraitID left, TraitID right) => left.Equals(right);
        public static bool operator !=(TraitID left, TraitID right) => !left.Equals(right);

        public readonly TraitDefinition GetDefinition() => TraitDatabase.GetDefinition(this);
        public readonly TraitInstance CreateInstance() => TraitDatabase.CreateInstance(this);
        public readonly bool IsCompatibleWith(TraitID id)
        {
            TraitDefinition definition = GetDefinition();

            for (int i = 0; i < definition.IncompatibleIDs.Length; i++)
            {
                if (definition.IncompatibleIDs[i] == id)
                {
                    return false;
                }
            }

            return true;
        }
        public readonly bool IsIncompatibleWith(TraitID id)
        {
            TraitDefinition definition = GetDefinition();

            for (int i = 0; i < definition.IncompatibleIDs.Length; i++)
            {
                if (definition.IncompatibleIDs[i] == id)
                {
                    return true;
                }
            }

            return false;
        }
        public readonly void GetIncompatibleDesc(StringBuilder sb)
        {
            if (sb == null)
            {
                Debug.LogError("Get compatible races failed! string builder is null!?");
                return;
            }

            IReadOnlyList<TraitDefinition> database = TraitDatabase.GetDatabase();

            foreach (TraitDefinition trait in database)
            {
                if (!IsIncompatibleWith(trait.ID))
                {
                    continue;
                }

                sb.Append(" -> ".ToRed());
                sb.AppendLine(trait.NameID.Get().ToRed());
            }
        }
    }
}
