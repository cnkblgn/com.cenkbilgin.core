using System;
using UnityEngine;

namespace Core.Quest
{
    [Serializable]
    public struct QuestID : IEquatable<QuestID>
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

                index = QuestDatabase.GetIDIndex(key);

                resolved = index >= 0;

                return index;
            }
        }

        [SerializeField, Required] private string key;
        [NonSerialized] private int index;
        [NonSerialized] private bool resolved;

        public QuestID(string key, int index)
        {
            this.key = key;
            this.index = index;
            this.resolved = true;
        }

        public override string ToString() => $"Key: {key} << Index: {Index}";

        public readonly override int GetHashCode() => key?.GetHashCode() ?? 0;
        public readonly override bool Equals(object obj) => obj is QuestID other && Equals(other);
        public readonly bool Equals(QuestID other) => string.Equals(key, other.key, StringComparison.Ordinal);
        public static bool operator ==(QuestID left, QuestID right) => left.Equals(right);
        public static bool operator !=(QuestID left, QuestID right) => !left.Equals(right);

        public readonly QuestDefinition GetDefinition() => QuestDatabase.GetDefinition(this);
        public readonly QuestInstance CreateInstance() => QuestDatabase.CreateInstance(this);
    }
}