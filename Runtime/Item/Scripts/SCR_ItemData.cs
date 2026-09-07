using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Item
{
    public sealed class ItemData : IEquatable<ItemData>
    {
        public readonly Dictionary<string, DataNode> Data;
        public readonly ItemID BaseID;
        public readonly Guid InstanceID;
        public readonly ulong Tags;

        private Vector2Int position;
        private int stack;
        private bool isRotated;

        public ItemData(ItemID id, Guid instanceID, Dictionary<string, DataNode> data, Vector2Int position, int stack, bool isRotated)
        {
            this.BaseID = id;
            this.InstanceID = instanceID;
            this.Data = data == null ? new() : new(data);
            this.position = position;
            this.stack = stack;
            this.isRotated = isRotated;
            this.Tags = BaseID.GetDefinition().Tags;
        }
        public ItemData(ItemID id) : this(id, Guid.NewGuid(), null, Vector2Int.zero, 1, false)
        {
            ItemDefinition definition = id.GetDefinition();

            definition.Component.GetDefaults(Data);

            stack = definition.Stack;
        }
        public ItemData(ItemData data) : this(data == null ? throw new ArgumentNullException(nameof(data)) : data.BaseID, data.InstanceID, data.Data, data.position, data.stack, data.isRotated) { }

        public bool Equals(ItemData other) => other is not null && InstanceID == other.InstanceID && BaseID == other.BaseID;
        public override bool Equals(object obj) => Equals(obj as ItemData);
        public override int GetHashCode() => HashCode.Combine(InstanceID, BaseID);
        public static bool operator ==(ItemData left, ItemData right) =>  left is null ? right is null : left.Equals(right);
        public static bool operator !=(ItemData left, ItemData right) => !(left == right);

        public Vector2Int GetScale() => GetScale(BaseID, isRotated);
        public Vector2Int GetScale(bool isRotated) => GetScale(BaseID, isRotated);
        public static Vector2Int GetScale(ItemID id, bool isRotated)
        {
            ItemDefinition definition = id.GetDefinition();
            int width = definition.Width;
            int height = definition.Height;

            return isRotated ? new(height, width) : new(width, height);
        }

        public Vector2Int GetPosition() => position;
        internal void SetPosition(Vector2Int value) => position = value;

        public bool GetRotation() => isRotated;
        internal void SetRotation(bool value) => isRotated = value;

        public float GetWeight() => GetWeight(stack);
        public float GetWeight(int stack) => BaseID.GetDefinition().Weight * stack;

        public bool IsStackable() => BaseID.GetDefinition().Stack > 1;
        public int GetStack() => stack;
        public void SetStack(int value) => stack = Mathf.Clamp(value, 0, BaseID.GetDefinition().Stack);
    }
}