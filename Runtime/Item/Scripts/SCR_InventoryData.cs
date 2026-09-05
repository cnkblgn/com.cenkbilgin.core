using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Item
{
    public sealed class InventoryData
    {
        public const int MIN_WIDTH = 1;
        public const int MAX_WIDTH = 16;
        public const int MIN_HEIGHT = 1;
        public const int MAX_HEIGHT = 16;
        public const int MIN_WEIGHT = 0;
        public const int MAX_WEIGHT = 1000;

        public float CurrentWeight { get; private set; }
        public int CurrentCapacity => itemTable.Count;
        public int MaximumCapacity => itemGrid.Length;

        public readonly int GridWidth;
        public readonly int GridHeight;
        public readonly int MaximumWeight;

        private ItemData[] itemGrid;
        private readonly Dictionary<Guid, ItemData> itemTable;

        public InventoryData(List<ItemData> items, int width, int height, int maxWeight)
        {
            GridWidth = Mathf.Clamp(width, MIN_WIDTH, MAX_WIDTH);
            GridHeight = Mathf.Clamp(height, MIN_HEIGHT, MAX_HEIGHT);
            MaximumWeight = Mathf.Clamp(maxWeight, MIN_WEIGHT, MAX_WEIGHT);
            CurrentWeight = 0;
            itemGrid = new ItemData[GridWidth * GridHeight];
            itemTable = new();

            if (items == null)
            {
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                ItemData item = new(items[i]);

                if (!TryAddItem(item, item.Position, out ItemData _, out InventoryResult ctx))
                {
                    Debug.LogWarning($"Failed to add item {item.BaseID} at {item.Position} — {ctx}. Skipping.");
                }
            }
        }
        public InventoryData(InventoryData data) : this(data?.itemTable.Values.ToList() ?? throw new ArgumentNullException(nameof(data)), data.GridWidth, data.GridHeight, data.MaximumWeight) { }
        public InventoryData(int width, int height, int maxWeight) : this(new(), width, height, maxWeight) { }

        public void Clear()
        {
            itemTable.Clear();
            itemGrid = new ItemData[GridWidth * GridHeight];
            CurrentWeight = 0;
        }

        public IReadOnlyCollection<Guid> GetItems() => itemTable.Keys;
        public int GetItems(ItemID baseID)
        {
            int count = 0;

            foreach (ItemData item in itemTable.Values)
            {
                if (item.BaseID == baseID)
                {
                    count++;
                }
            }

            return count;
        }

        public bool TryGetValidPosition(ItemData item, out Vector2Int position, out InventoryResult result)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (!TryGetAnyPosition(item.GetScale(), out position, out _))
            {
                item.IsRotated = !item.IsRotated;

                if (!TryGetAnyPosition(item.GetScale(), out position, out result))
                {
                    item.IsRotated = !item.IsRotated;
                    return false;
                }
            }

            return IsPositionValid(item, position, out result);
        }
        public bool TryGetClampedPosition(Vector2Int scale, ref Vector2Int position, out InventoryResult result)
        {
            if (scale.x > GridWidth || scale.y > GridHeight)
            {
                result = InventoryResult.NO_VALID_SPACE;
                return false;
            }

            int maxX = GridWidth - scale.x;
            int maxY = GridHeight - scale.y;

            if (maxX < 0) maxX = 0;
            if (maxY < 0) maxY = 0;

            int cx = Mathf.Clamp(position.x, 0, maxX);
            int cy = Mathf.Clamp(position.y, 0, maxY);

            position = new(cx, cy);

            result = InventoryResult.SUCCESS;
            return true;
        }
        public bool TryGetAnyPosition(Vector2Int scale, out Vector2Int position, out InventoryResult result) => TryGetAnyPosition(itemGrid, scale, out position, out result);
        private bool TryGetAnyPosition(ItemData[] grid, Vector2Int scale, out Vector2Int position, out InventoryResult result)
        {
            position = Vector2Int.zero;

            if (scale.x <= 0 || scale.y <= 0)
            {
                result = InventoryResult.OUT_OF_BOUNDS;
                return false;
            }

            int maxY = GridHeight - scale.y;
            int maxX = GridWidth - scale.x;

            if (maxY < 0 || maxX < 0)
            {
                result = InventoryResult.OUT_OF_BOUNDS;
                return false;
            }

            for (int y = 0; y <= maxY; y++)
            {
                for (int x = 0; x <= maxX; x++)
                {
                    if (!IsTileOverlapping(grid, x, y, scale.x, scale.y, out _))
                    {
                        position = new(x, y);
                        result = InventoryResult.SUCCESS;
                        return true;
                    }
                }
            }

            result = InventoryResult.NO_VALID_SPACE;
            return false;
        }
        public bool TryGetItemByTag(ItemTag tag, out ItemData registered, out InventoryResult result) => TryGetItemByTag(tag.Mask, out registered, out result);
        public bool TryGetItemByTag(ulong tags, out ItemData registered, out InventoryResult result)
        {
            foreach (ItemData item in itemTable.Values)
            {
                if (item.Tags.HasAny(tags))
                {
                    registered = item;
                    result = InventoryResult.SUCCESS;
                    return true;
                }
            }

            registered = null;
            result = InventoryResult.NOT_REGISTERED;
            return false;
        }
        public bool TryGetItemsByTag(ItemTag[] tags, out List<ItemData> items, out InventoryResult result) => TryGetItemsByTag(tags.CreateMask(), out items, out result);
        public bool TryGetItemsByTag(ulong tags, out List<ItemData> items, out InventoryResult result)
        {
            items = new();
            bool found = false;

            foreach (ItemData item in itemTable.Values)
            {
                if (item.Tags.HasAny(tags))
                {
                    items.Add(item);
                    found = true;
                }
            }

            result = found ? InventoryResult.SUCCESS : InventoryResult.NOT_REGISTERED;
            return found;
        }
        public bool TryGetItemByBaseID(ItemID baseID, out ItemData registered)
        {
            foreach (ItemData item in itemTable.Values)
            {
                if (item.BaseID == baseID)
                {
                    registered = item;
                    return true;
                }
            }

            registered = null;
            return false;
        }
        public bool TryGetItemsByBaseID(ItemID baseID, List<ItemData> registered)
        {
            if (registered == null)
            {
                throw new ArgumentNullException($"registered list cannot be null! {nameof(registered)}");
            }

            bool found = false;

            foreach (ItemData item in itemTable.Values)
            {
                if (item.BaseID == baseID)
                {
                    registered.Add(item);
                    found = true;
                }
            }

            return found;
        }
        public bool TryGetItemByInstanceID(Guid instanceID, out ItemData registered)
        {
            if (!itemTable.TryGetValue(instanceID, out registered))
            {
                return false;
            }

            return true;
        }
        public bool TryGetItemByPosition(Vector2Int position, out ItemData registered)
        {
            if (!IsTileInsideBoundary(position.x, position.y))
            {
                registered = null;
                return false;
            }

            registered = itemGrid[(position.y * GridWidth) + (position.x)];
            return registered != null;
        }
        public bool TryGetItemByArea(Vector2Int scale, Vector2Int position, out ItemData overlapped, out InventoryResult result)
        {
            overlapped = null;

            if (!IsTileInsideBoundary(position.x, position.y, scale.x, scale.y))
            {
                result = InventoryResult.OUT_OF_BOUNDS;
                return false;
            }

            if (!IsTileOverlapping(position.x, position.y, scale.x, scale.y, out overlapped))
            {
                result = InventoryResult.NO_VALID_SPACE;
                return false;
            }

            result = InventoryResult.SUCCESS;
            return true;
        }
        public bool TryGetItemStack(Guid instanceID, out int stack, out InventoryResult result)
        {
            stack = 0;

            if (!TryGetItemByInstanceID(instanceID, out ItemData registered))
            {
                result = InventoryResult.NOT_REGISTERED;
                return false;
            }

            stack = registered.GetStack();
            result = InventoryResult.SUCCESS;
            return true;
        }
        internal bool TryGetItemStack(ItemData itemData, out int stack, out InventoryResult result)
        {
            stack = 0;

            if (itemData == null)
            {
                result = InventoryResult.NULL;
                Debug.LogError("Try get item stack failed! item data is null!?");
                return false;
            }

            return TryGetItemStack(itemData.InstanceID, out stack, out result);
        }

        public bool IsPositionValid(ItemData item, Vector2Int position, out InventoryResult result)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (TryGetItemByArea(item.GetScale(), position, out _, out result))
            {
                return false;
            }

            if (!IsWeightEnough(item.GetWeight()))
            {
                result = InventoryResult.WEIGHT_LIMIT_EXCEEDED;
                return false;
            }

            if (TryGetItemByInstanceID(item.InstanceID, out _))
            {
                result = InventoryResult.DUPLICATE;
                return false;
            }

            result = InventoryResult.SUCCESS;
            return true;
        }
        private bool IsWeightEnough(float weight) => weight + CurrentWeight <= MaximumWeight;
        private bool IsTileOverlapping(int tilePositionX, int tilePositionY, int tileWidth, int tileHeight, out ItemData overlapped) => IsTileOverlapping(itemGrid, tilePositionX, tilePositionY, tileWidth, tileHeight, out overlapped);
        private bool IsTileOverlapping(ItemData[] grid, int tilePositionX, int tilePositionY, int tileWidth, int tileHeight, out ItemData overlapped)
        {
            for (int y = 0; y < tileHeight; y++)
            {
                for (int x = 0; x < tileWidth; x++)
                {
                    overlapped = grid[((tilePositionY + y) * GridWidth) + (tilePositionX + x)];

                    if (overlapped != null)
                    {
                        return true;
                    }
                }
            }

            overlapped = null;
            return false;
        }
        private bool IsTileInsideBoundary(int tilePositionX, int tilePositionY, int tileWidth, int tileHeight)
        {
            if (!IsTileInsideBoundary(tilePositionX, tilePositionY))
            {
                return false;
            }

            tilePositionX += tileWidth - 1;
            tilePositionY += tileHeight - 1;

            if (!IsTileInsideBoundary(tilePositionX, tilePositionY))
            {
                return false;
            }

            return true;
        }
        private bool IsTileInsideBoundary(int tilePositionX, int tilePositionY)
        {
            if (tilePositionX < 0 || tilePositionY < 0)
            {
                return false;
            }

            if (tilePositionX >= GridWidth || tilePositionY >= GridHeight)
            {
                return false;
            }

            return true;
        }
        public bool CanPlaceItem(ItemID id, Vector2Int position, bool isRotated, out InventoryResult result)
        {
            if (!id.IsValid)
            {
                result = InventoryResult.NULL;
                return false;
            }

            ItemDefinition definition = id.GetDefinition();

            Vector2Int baseScale = new(definition.Width, definition.Height);
            Vector2Int newScale = isRotated ? new(baseScale.y, baseScale.x) : baseScale;

            if (TryGetItemByArea(newScale, position, out _, out result))
            {
                return false;
            }

            if (result == InventoryResult.OUT_OF_BOUNDS)
            {
                return false;
            }

            result = InventoryResult.SUCCESS;
            return true;
        }

        private void RegisterItem(ItemData item, Vector2Int position, out ItemData registered)
        {
            registered = new(item, position);

            SetTileItem(registered, registered.Position, registered.GetScale());

            itemTable[registered.InstanceID] = registered;
            CurrentWeight += registered.GetWeight();
        }
        private void SetTileItem(ItemData item, Vector2Int position, Vector2Int scale) => SetTileItem(itemGrid, item, position, scale);
        private void SetTileItem(ItemData[] grid, ItemData item, Vector2Int position, Vector2Int scale)
        {
            if (!IsTileInsideBoundary(position.x, position.y, scale.x, scale.y))
            {
                throw new IndexOutOfRangeException($"Item tile placement out of bounds! pos = {position} scale = {scale}");
            }

            for (int y = 0; y < scale.y; y++)
            {
                for (int x = 0; x < scale.x; x++)
                {
                    grid[((position.y + y) * GridWidth) + (position.x + x)] = item;
                }
            }
        }
        public bool TrySortItems(IInventorySorter sorter, out InventoryResult result)
        {
            if (sorter == null)
            {
                throw new ArgumentNullException(nameof(sorter), "Item sort failed! sorter missing!?");
            }

            return TrySortItems(Comparer<ItemData>.Create(sorter.Compare), out result);
        }
        private bool TrySortItems(IComparer<ItemData> comparer, out InventoryResult result)
        {
            if (itemTable.Count == 0)
            {
                result = InventoryResult.SUCCESS;
                return true;
            }

            List<ItemData> sorted = itemTable.Values.ToList();
            sorted.Sort(comparer);

            ItemData[] tempGrid = new ItemData[itemGrid.Length];
            List<(ItemData item, Vector2Int position, bool rotated)> placements = new(sorted.Count);

            foreach (ItemData item in sorted)
            {
                ItemDefinition definition = item.BaseID.GetDefinition();
                Vector2Int baseScale = new(definition.Width, definition.Height);

                bool rotated = item.IsRotated;
                Vector2Int scale = rotated ? new(baseScale.y, baseScale.x) : baseScale;

                if (!TryGetAnyPosition(tempGrid, scale, out Vector2Int position, out result))
                {
                    rotated = !rotated;
                    scale = rotated ? new(baseScale.y, baseScale.x) : baseScale;

                    if (!TryGetAnyPosition(tempGrid, scale, out position, out result))
                    {
                        return false;
                    }
                }

                SetTileItem(tempGrid, item, position, scale);
                placements.Add((item, position, rotated));
            }

            foreach ((ItemData item, Vector2Int position, bool rotated) in placements)
            {
                item.IsRotated = rotated;
                item.Position = position;
            }

            itemGrid = tempGrid;
            result = InventoryResult.SUCCESS;
            return true;
        }
        public bool TryMergeStacks(out List<ItemData> changed, out List<ItemData> removed, out InventoryResult result) => TryMergeStacks(null, out changed, out removed, out result);
        public bool TryMergeStacks(Func<ItemData, ItemData, bool> canStackPredicate, out List<ItemData> changed, out List<ItemData> removed, out InventoryResult result)
        {
            int totalMoved = 0;
            changed = new();
            removed = new();
            result = InventoryResult.SUCCESS;

            if (itemTable.Count < 2)
            {
                result = InventoryResult.NOT_REGISTERED;
                return false;
            }

            Dictionary<ItemID, List<ItemData>> groups = new();

            foreach (ItemData item in itemTable.Values)
            {
                if (!groups.TryGetValue(item.BaseID, out List<ItemData> list))
                {
                    list = new();
                    groups[item.BaseID] = list;
                }

                list.Add(item);
            }

            HashSet<Guid> touchedTargets = new();
            List<Guid> toRemove = new();

            foreach (List<ItemData> group in groups.Values)
            {
                if (group.Count < 2)
                {
                    continue;
                }

                int maxStack = group[0].BaseID.GetDefinition().Stack;

                if (maxStack <= 1)
                {
                    continue;
                }

                for (int i = 0; i < group.Count; i++)
                {
                    ItemData target = group[i];

                    if (target.GetStack() <= 0)
                    {
                        continue;
                    }

                    for (int j = i + 1; j < group.Count; j++)
                    {
                        ItemData source = group[j];

                        if (source.GetStack() <= 0)
                        {
                            continue;
                        }

                        if (canStackPredicate != null && !canStackPredicate(target, source))
                        {
                            continue;
                        }

                        int space = maxStack - target.GetStack();

                        if (space <= 0)
                        {
                            break;
                        }

                        int amount = Mathf.Min(space, source.GetStack());

                        if (amount <= 0)
                        {
                            continue;
                        }

                        TrySetItemStack(target.InstanceID, target.GetStack() + amount, out _);
                        TrySetItemStack(source.InstanceID, source.GetStack() - amount, out _);

                        totalMoved += amount;
                        touchedTargets.Add(target.InstanceID);

                        if (source.GetStack() <= 0)
                        {
                            toRemove.Add(source.InstanceID);
                        }
                    }
                }
            }

            foreach (Guid id in touchedTargets)
            {
                if (TryGetItemByInstanceID(id, out ItemData targetItem))
                {
                    changed.Add(targetItem);
                }
            }

            foreach (Guid id in toRemove)
            {
                if (TryRemoveItem(id, out ItemData removedItem, out _))
                {
                    removed.Add(removedItem);
                }
            }

            if (totalMoved <= 0)
            {
                result = InventoryResult.NO_VALID_SPACE; // ya da uygun bir "hiçbir þey birleþmedi" sonucu
                return false;
            }

            result = InventoryResult.SUCCESS;
            return true;
        }
        /// <summary> Tries to set item stack. </summary>
        public bool TrySetItemStack(Guid instanceID, int stack, out InventoryResult result)
        {
            if (!TryGetItemByInstanceID(instanceID, out ItemData registered))
            {
                result = InventoryResult.NOT_REGISTERED;
                return false;
            }

            float previousWeight = registered.GetWeight();

            registered.SetStack(stack);

            float currentWeight = registered.GetWeight();

            CurrentWeight = Mathf.Max(0f, CurrentWeight + (currentWeight - previousWeight));

            result = InventoryResult.SUCCESS;
            return true;
        }
        /// <summary> Tries to set item stack. </summary>
        internal bool TrySetItemStack(ItemData itemData, int stack, out InventoryResult result)
        {
            if (itemData == null)
            {
                result = InventoryResult.NULL;
                Debug.LogError("Try set item stack failed! item data is null!?");
                return false;
            }

            return TrySetItemStack(itemData.InstanceID, stack, out result);
        }
        /// <summary> Tries to add item. Set position null if you want automatic positioning. </summary>
        public bool TryAddItem(ItemData item, Vector2Int? position, out ItemData registered, out InventoryResult result)
        {
            registered = null;

            Vector2Int bestPosition;

            if (position.HasValue)
            {
                bestPosition = position.Value;

                if (!IsPositionValid(item, bestPosition, out result))
                {
                    return false;
                }
            }
            else
            {
                if (!TryGetValidPosition(item, out bestPosition, out result))
                {
                    return false;
                }
            }

            result = InventoryResult.SUCCESS;
            RegisterItem(item, bestPosition, out registered);
            return true;
        }
        /// <summary> Tries to completely remove existing item from inventory. </summary>
        public bool TryRemoveItem(Guid instanceID, out ItemData registered, out InventoryResult result)
        {
            if (TryClearItem(instanceID, out registered, out result))
            {
                CurrentWeight = Mathf.Max(0, CurrentWeight - registered.GetWeight());
                itemTable.Remove(registered.InstanceID);
                return true;
            }

            return false;
        }
        /// <summary> Tries to clear existing item from tile. </summary>
        public bool TryClearItem(Guid instanceID, out ItemData registered, out InventoryResult result)
        {
            if (!TryGetItemByInstanceID(instanceID, out registered))
            {
                Debug.LogError($"Inventory clear item failed! [{instanceID}] not found!");
                result = InventoryResult.NOT_REGISTERED;
                return false;
            }

            Vector2Int position = registered.Position;
            Vector2Int scale = registered.GetScale();

            if (!IsTileInsideBoundary(position.x, position.y, scale.x, scale.y))
            {
                result = InventoryResult.OUT_OF_BOUNDS;
                return false;
            }

            SetTileItem(null, position, scale);
            result = InventoryResult.SUCCESS;
            return true;
        }
        /// <summary> Tries to assign existing item to tile. </summary>
        public bool TryPlaceItem(Guid instanceID, Vector2Int position, bool isRotated, out ItemData registered, out InventoryResult result)
        {
            if (!TryGetItemByInstanceID(instanceID, out registered))
            {
                Debug.LogError($"Inventory place item failed! [{instanceID}] not found!");
                result = InventoryResult.NOT_REGISTERED;
                return false;
            }

            bool oldRotation = registered.IsRotated;
            registered.IsRotated = isRotated;
            Vector2Int newScale = registered.GetScale();

            if (TryGetItemByArea(newScale, position, out _, out result))
            {
                registered.IsRotated = oldRotation;
                return false;
            }

            if (result == InventoryResult.OUT_OF_BOUNDS)
            {
                registered.IsRotated = oldRotation;
                return false;
            }

            registered.Position = position;
            SetTileItem(registered, position, newScale);
            result = InventoryResult.SUCCESS;
            return true;
        }
    }
}