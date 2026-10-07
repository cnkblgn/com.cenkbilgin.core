using System;
using Core.Localization;
using Core.Graphics;
using Core.Prefab;

namespace Core.Item
{
    public sealed class ItemDefinition
    {
        public readonly ItemID ID;
        public readonly ulong Tags;
        public readonly PrefabID PrefabID;
        public readonly MeshID MeshID;
        public readonly IconID IconID;
        public readonly LocalizedID NameID;
        public readonly LocalizedID DescID;

        public readonly int Width;
        public readonly int Height;
        public readonly int Stack;
        public readonly float Weight;

        public readonly IItemComponent Component;
        public readonly ItemAction[] Actions;

        internal ItemDefinition(ItemID id, ItemTag[] tags, PrefabID prefabID, MeshID meshID, IconID iconID, LocalizedID nameID, LocalizedID descID, int width, int height, int stack, float weight, IItemComponent component, ItemAction[] actions)
        {
            ID = id;
            Tags = tags == null ? 0 : tags.CreateMask();
            PrefabID = prefabID.IsValid ? prefabID : throw new ArgumentNullException($"Item entity id is not valid! [{prefabID.Key}]");
            MeshID = meshID;
            IconID = iconID;
            NameID = nameID;
            DescID = descID;
            Width = width;
            Height = height;
            Stack = stack;
            Weight = weight;
            Component = component ?? IItemComponent.DEFAULT;
            Actions = actions ?? Array.Empty<ItemAction>();
        }
        internal ItemDefinition(ItemEntry entry) : this
        (
            entry.ID,
            entry.Tags,
            entry.PrefabID,
            entry.MeshID,
            entry.IconID,
            entry.NameID,
            entry.DescID,
            entry.Width,
            entry.Height,
            entry.Stack,
            entry.Weight,
            entry.Component,
            entry.Actions
        ) { }
    }
}