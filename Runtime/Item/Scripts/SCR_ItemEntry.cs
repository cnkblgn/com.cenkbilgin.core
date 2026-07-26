using System;
using UnityEngine;
using Core.Graphics;
using Core.Localization;
using Core.Prefab;

namespace Core.Item
{
    [Serializable]
    public struct ItemEntry
    {
        [Info("Please generate id if its not visible")] public ItemID ID;
        public ItemTag[] Tags;
        public PrefabID EntityID;
        public PrefabID EquipableID;
        public IconID IconID;
        public LocalizedID NameID;
        public LocalizedID DescID;
        [Min(1)] public int Width;
        [Min(1)] public int Height;
        [Min(1)] public int Stack;
        [Min(0)] public float Weight;
        [SerializeReference, Reference] public ItemComponent Component;

        public ItemEntry(ItemID id, ItemTag[] tags, PrefabID entityID, PrefabID equipableID, IconID iconID, LocalizedID nameID, LocalizedID descID, int width, int height, int stack, float weight, ItemComponent component)
        {
            ID = id;
            Tags = tags;
            EntityID = entityID;
            EquipableID = equipableID;
            IconID = iconID;
            NameID = nameID;
            DescID = descID;
            Width = width;
            Height = height;
            Stack = stack;
            Weight = weight;
            Component = component;
        }
    }
}