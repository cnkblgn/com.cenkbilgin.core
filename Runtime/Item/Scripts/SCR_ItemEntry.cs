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
#if UNITY_EDITOR
        [HideInInspector] public string Name;
#endif

        [Info("Please generate id if its not visible")] 
        public ItemID ID;
        public ItemTag[] Tags;
        public PrefabID EntityID;
        public PrefabID EquipableID;
        public MeshID MeshID;
        public IconID IconID;
        public LocalizedID NameID;
        public LocalizedID DescID;
        [Range(InventoryData.MIN_WIDTH, InventoryData.MAX_WIDTH)] public int Width;
        [Range(InventoryData.MIN_HEIGHT, InventoryData.MAX_HEIGHT)] public int Height;
        [Min(1)] public int Stack;
        [Range(InventoryData.MIN_WEIGHT, InventoryData.MAX_WEIGHT)] public float Weight;
        [SerializeReference, Reference] public ItemComponent Component;

        public ItemEntry(ItemID id, ItemTag[] tags, PrefabID entityID, PrefabID equipableID, MeshID meshID, IconID iconID, LocalizedID nameID, LocalizedID descID, int width, int height, int stack, float weight, ItemComponent component)
        {
            ID = id;
            Tags = tags;
            EntityID = entityID;
            EquipableID = equipableID;
            MeshID = meshID;
            IconID = iconID;
            NameID = nameID;
            DescID = descID;
            Width = width;
            Height = height;
            Stack = stack;
            Weight = weight;
            Component = component;
#if UNITY_EDITOR
            Name = ID.Key;
#endif
        }
    }
}