using System;

namespace Core.Item
{
    public sealed class ItemDefinition
    {
        public readonly ItemID ID;
        public readonly ulong Tags;
        public readonly string PrefabID;
        public readonly string IconID;
        public readonly string NameID;
        public readonly string DescID;
        public readonly int Stack;
        public readonly int Width;
        public readonly int Height;
        public readonly float Weight;
        public readonly ItemComponent Component;
        public readonly ItemAction[] Actions;

        internal ItemDefinition(ItemID id, ItemTag[] tags, int width, int height, int stack, float weight, ItemComponent component, ItemAction[] actions)
        {
            ID = id;
            Tags = tags == null ? 0 : tags.CreateMask();
            PrefabID = $"Item{ID.Key}";
            IconID = $"Icon{ID.Key}";
            NameID = $"item.{ID.Key}.name";
            DescID = $"item.{ID.Key}.desc";
            Width = width;
            Height = height;
            Stack = stack;
            Weight = weight;
            Component = component == null ? ItemComponent.NONE : component;
            Actions = actions ?? ItemAction.NONE;
        }
        internal ItemDefinition(ItemEntry entry) : this (entry.ID, entry.Tags, entry.Width, entry.Height, entry.Stack, entry.Weight, entry.Component, entry.Actions) { }
    }
}