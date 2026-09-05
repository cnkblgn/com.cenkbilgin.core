using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Item
{
    public interface IInventorySorter
    {
        public int Compare(ItemData a, ItemData b);
    }

    public sealed class InventorySortByArea : IInventorySorter
    {
        private readonly bool descending;

        public InventorySortByArea(bool descending) => this.descending = descending;

        public int Compare(ItemData a, ItemData b)
        {
            int areaA = a.GetScale().x * a.GetScale().y;
            int areaB = b.GetScale().x * b.GetScale().y;

            return descending ? areaB.CompareTo(areaA) : areaA.CompareTo(areaB);
        }
    }

    public sealed class InventorySortByTag : IInventorySorter
    {
        private readonly IReadOnlyList<ItemTag> priority;

        public InventorySortByTag(IReadOnlyList<ItemTag> priority)
        {
            this.priority = priority ?? throw new ArgumentNullException(nameof(priority));
        }
        public int Compare(ItemData a, ItemData b)
        {
            int priorityA = GetPriorityIndex(a);
            int priorityB = GetPriorityIndex(b);

            if (priorityA != priorityB)
            {
                return priorityA.CompareTo(priorityB);
            }

            Vector2Int scaleA = a.GetScale();
            Vector2Int scaleB = b.GetScale();
            return (scaleB.x * scaleB.y).CompareTo(scaleA.x * scaleA.y);
        }
        private int GetPriorityIndex(ItemData item)
        {
            for (int i = 0; i < priority.Count; i++)
            {
                if (item.Tags.HasAny(priority[i].Mask))
                {
                    return i;
                }
            }

            return priority.Count;
        }
    }

    public static class InventorySorter
    {
        public static readonly InventorySortByArea SortByArea = new(false);

        public static readonly InventorySortByArea SortByAreaDescending = new(true);

        public static readonly InventorySortByTag SortByTag = new(ItemDatabase.GetTags());
    }
}
