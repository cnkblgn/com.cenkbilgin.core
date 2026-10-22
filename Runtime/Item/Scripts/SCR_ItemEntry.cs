using System;
using UnityEngine;

namespace Core.Item
{
    [Serializable]
    public struct ItemEntry
    {
#if UNITY_EDITOR
        [HideInInspector] public string Name;
#endif
         
        public ItemID ID;
        public ItemTag[] Tags;
        [Min(1)] public int Stack;
        [Range(InventoryData.MIN_WIDTH, InventoryData.MAX_WIDTH)] public int Width;
        [Range(InventoryData.MIN_HEIGHT, InventoryData.MAX_HEIGHT)] public int Height;
        [Range(InventoryData.MIN_WEIGHT, InventoryData.MAX_WEIGHT)] public float Weight;
        [Expandable] public ItemComponent Component;
        [Required, Expandable] public ItemAction[] Actions;

        public ItemEntry(ItemID id, ItemTag[] tags, int stack, int width, int height, float weight, ItemComponent component, ItemAction[] actions)
        {
            ID = id;
            Tags = tags;
            Width = width;
            Height = height;
            Stack = stack;
            Weight = weight;
            Component = component;
            Actions = actions;

#if UNITY_EDITOR
            Name = ID.Key;
#endif
        }


#if UNITY_EDITOR
        public void OnValidate()
        {
            Name = ID.Key;

            if (Component != null)
            {
                Component.Validate(ID);
            }
        }
#endif
    }
}