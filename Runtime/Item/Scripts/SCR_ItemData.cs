using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Item
{
    public sealed class ItemData : IEquatable<ItemData>
    {
        public readonly ItemID BaseID;
        public readonly Guid InstanceID;
        public readonly ulong Tags;

        public Vector2Int Position;
        public bool IsRotated;

        private int stack;

        public readonly Dictionary<string, DataNode> Data;

        public ItemData(ItemID id, Guid instanceID, Dictionary<string, DataNode> data, Vector2Int position, int stack, bool isRotated)
        {
            this.BaseID = id;
            this.InstanceID = instanceID;
            this.Data = data == null ? new() : new(data);
            this.Position = position;
            this.stack = stack;
            this.IsRotated = isRotated;
            this.Tags = BaseID.GetDefinition().Tags;
        }
        public ItemData(ItemID id) : this(id, Guid.NewGuid(), null, Vector2Int.zero, 1, false)
        {
            ItemDefinition definition = id.GetDefinition();

            definition.Component.GetDefaults(Data);

            stack = definition.Stack;
        }
        public ItemData(ItemData data) : this(data == null ? throw new ArgumentNullException(nameof(data)) : data.BaseID, data.InstanceID, data.Data, data.Position, data.stack, data.IsRotated) { }
        public ItemData(ItemData data, Vector2Int position) : this(data) { Position = position; }

        public bool Equals(ItemData other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return InstanceID.Equals(other.InstanceID) && BaseID.Equals(other.BaseID);
        }
        public override bool Equals(object obj) => Equals(obj as ItemData);
        public override int GetHashCode() => HashCode.Combine(InstanceID, BaseID);
        public static bool operator ==(ItemData left, ItemData right) =>  left is null ? right is null : left.Equals(right);
        public static bool operator !=(ItemData left, ItemData right) => !(left == right);

        public Vector2Int GetScale()
        {
            ItemDefinition definition = BaseID.GetDefinition();
            int width = definition.Width;
            int height = definition.Height;

            return IsRotated ? new(height, width) : new(width, height);
        }

        public float GetWeight() => BaseID.GetDefinition().Weight * stack;

        public int GetStack() => stack;
        public void SetStack(int value) => stack = Mathf.Clamp(value, 0, BaseID.GetDefinition().Stack);
    }
}