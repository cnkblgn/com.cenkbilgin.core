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
        public readonly int Index => index;
        public readonly bool IsValid => !string.IsNullOrEmpty(key) && index >= 0;

        [SerializeField, Required] private string key;
        [SerializeField, ReadOnly] private int index;

        public TraitID(string key, int index)
        {
            this.key = key;
            this.index = index;
        }

        public readonly override string ToString() => $"Key: {key} << Index: {index}";
        public readonly override int GetHashCode() => index;
        public readonly override bool Equals(object obj) => obj is TraitID other && Equals(other);
        public readonly bool Equals(TraitID other) => index == other.index;
        public static bool operator ==(TraitID left, TraitID right) => left.Equals(right);
        public static bool operator !=(TraitID left, TraitID right) => !left.Equals(right);

        public readonly TraitDefinition GetDefinition() => TraitDatabase.GetDefinition(this);
        public readonly TraitInstance CreateInstance() => TraitDatabase.CreateInstance(this);
        public readonly bool IsCompatibleWith(TraitID id)
        {
            TraitDefinition definition = GetDefinition();

            for (int i = 0; i < definition.IncompatibleIDs.Length; i++)
            {
                TraitID incompatibleID = definition.IncompatibleIDs[i];

                if (incompatibleID == id)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
