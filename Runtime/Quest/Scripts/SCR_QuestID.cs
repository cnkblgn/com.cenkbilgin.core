using System;
using UnityEngine;

namespace Core.Quest
{
    using static CoreUtility;

    [Serializable]
    public partial struct QuestID : IEquatable<QuestID>
    {
        public static readonly QuestID NONE = new(STRING_EMPTY);

        public readonly string Key => key;
        public readonly bool IsValid => !string.IsNullOrEmpty(key);

        [SerializeField, Required] private string key;

        public QuestID(string key) => this.key = key;

        public readonly override int GetHashCode() => key != null ? key.GetHashCode() : 0;
        public readonly override bool Equals(object obj) => obj is QuestID other && Equals(other);
        public readonly bool Equals(QuestID other) => key == other.key;
        public static bool operator ==(QuestID left, QuestID right) => left.Equals(right);
        public static bool operator !=(QuestID left, QuestID right) => !left.Equals(right);

        public readonly QuestDefinition GetDefinition() => QuestDatabase.GetDefinition(this);
        public readonly QuestInstance CreateInstance() => QuestDatabase.CreateInstance(this);
    }
}