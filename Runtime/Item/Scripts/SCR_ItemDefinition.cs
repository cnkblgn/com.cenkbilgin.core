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
        public readonly PrefabID EntityID;
        public readonly PrefabID EquipableID;
        public readonly PrefabID ModelID;

        public readonly IconID IconID;
        public readonly LocalizedID NameID;
        public readonly LocalizedID DescID;

        public readonly int Width;
        public readonly int Height;
        public readonly int Stack;
        public readonly float Weight;

        public readonly ItemComponent Component;

        internal ItemDefinition(ItemID id, ItemTag[] tags, PrefabID entityID, PrefabID equipableID, PrefabID modelID, IconID iconID,  LocalizedID nameID, LocalizedID descID, int width, int height, int stack, float weight, ItemComponent component)
        {
            ID = id;
            Tags = tags.CreateMask();
            EntityID = entityID.IsValid ? entityID : throw new ArgumentNullException($"Item entity id is not valid! [{entityID.Key}]");
            EquipableID = equipableID;
            ModelID = modelID;
            IconID = iconID;
            NameID = nameID;
            DescID = descID;
            Width = width;
            Height = height;
            Stack = stack;
            Weight = weight;
            Component = component ?? ItemComponent.DEFAULT;
        }
        internal ItemDefinition(ItemEntry entry) : this
        (
            entry.ID,
            entry.Tags,
            entry.EntityID,
            entry.EquipableID,
            entry.ModelID,
            entry.IconID,
            entry.NameID,
            entry.DescID,
            entry.Width,
            entry.Height,
            entry.Stack,
            entry.Weight,
            entry.Component
        ) { }
    }
}