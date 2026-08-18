using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Core.Trait
{
    using static CoreUtility;

    [Serializable]
    public struct TraitID : IEquatable<TraitID>
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

                index = TraitDatabase.GetIDIndex(key);

                resolved = index >= 0;

                return index;
            }
        }

        [SerializeField, Required] private string key;
        [NonSerialized] private int index;
        [NonSerialized] private bool resolved;

        public TraitID(string key, int index) => (this.key, this.index, this.resolved) = (key, index, index >= 0);
        public TraitID(string key) : this(key, -1) { }

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
        public readonly bool TryGetIncompatibleDesc(StringBuilder sb)
        {
            if (sb == null)
            {
                Debug.LogError("Get incompatible races desc failed! string builder is null!?");
                return false;
            }

            IReadOnlyList<TraitDefinition> database = TraitDatabase.GetDefinitions();
            bool found = false;

            foreach (TraitDefinition trait in database)
            {
                if (!IsIncompatibleWith(trait.ID))
                {
                    continue;
                }

                sb.Append(" -> ".ToRed());
                sb.AppendLine(trait.NameID.Get().ToRed());
                found = true;
            }

            return found;
        }
        public readonly bool TryGetCompatibleDesc(StringBuilder sb)
        {
            if (sb == null)
            {
                Debug.LogError("Get compatible races desc failed! string builder is null!?");
                return false;
            }

            IReadOnlyList<TraitDefinition> database = TraitDatabase.GetDefinitions();
            bool found = false;

            foreach (TraitDefinition trait in database)
            {
                if (!IsCompatibleWith(trait.ID))
                {
                    continue;
                }

                sb.Append(" -> ".ToGreen());
                sb.AppendLine(trait.NameID.Get().ToGreen());
                found = true;
            }

            return found;
        }
    }
}
