using System;
using UnityEngine;

namespace Core.Quest
{
    using static CoreUtility;

    [Serializable]
    public partial struct QuestID : IEquatable<QuestID>
    {
        public readonly string Key => key;
        public readonly int Index => index;
        public readonly bool IsValid => !string.IsNullOrEmpty(key) && index >= 0;

        [SerializeField, Required] private string key;
        [SerializeField, ReadOnly] private int index;

        public QuestID(string key, int index)
        {
            this.key = key;
            this.index = index;
        }

        public readonly override string ToString() => $"Key: {key} << Index: {index}";
        public readonly override int GetHashCode() => index;
        public readonly override bool Equals(object obj) => obj is QuestID other && Equals(other);
        public readonly bool Equals(QuestID other) => index == other.index;
        public static bool operator ==(QuestID left, QuestID right) => left.Equals(right);
        public static bool operator !=(QuestID left, QuestID right) => !left.Equals(right);

        public readonly QuestDefinition GetDefinition() => QuestDatabase.GetDefinition(this);
        public readonly QuestInstance CreateInstance() => QuestDatabase.CreateInstance(this);
    }
}