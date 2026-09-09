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

        internal IInventoryUser User { get; set; }
        public float CurrentWeight { get; private set; }
        public int CurrentCapacity => itemTable.Count;
        public int MaximumCapacity => itemGrid.Length;

        public readonly int GridWidth;
        public readonly int GridHeight;
        public readonly int MaximumWeight;
        public readonly ulong ItemMask;

        private ItemData[] itemGrid;
        private readonly Dictionary<Guid, ItemData> itemTable;

        public InventoryData(List<ItemData> items, int width, int height, int maxWeight, ulong mask)
        {
            GridWidth = Mathf.Clamp(width, MIN_WIDTH, MAX_WIDTH);
            GridHeight = Mathf.Clamp(height, MIN_HEIGHT, MAX_HEIGHT);
            MaximumWeight = Mathf.Clamp(maxWeight, MIN_WEIGHT, MAX_WEIGHT);
            CurrentWeight = 0;
            itemGrid = new ItemData[GridWidth * GridHeight];
            itemTable = new();
            ItemMask = mask;

            if (items == null)
            {
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                ItemData item = new(items[i]);

                if (!TryAddItem(item, item.GetPosition(), item.GetRotation(), out ItemData _, out InventoryResult ctx))
                {
                    Debug.LogWarning($"Failed to add item {item.BaseID} at {item.GetPosition()} — {ctx}. Skipping.");
                }
            }
        }
        public InventoryData(List<ItemData> items, int width, int height, int maxWeight, ItemTag[] masks) : this(items, width, height, maxWeight, (masks != null ? masks.ToArray() : ItemDatabase.GetTags().ToArray()).CreateMask()) { }
        public InventoryData(InventoryData data) : this(data?.itemTable.Values.ToList() ?? throw new ArgumentNullException(nameof(data)), data.GridWidth, data.GridHeight, data.MaximumWeight, data.ItemMask) { }
        public InventoryData(int width, int height, int maxWeight, ItemTag[] masks) : this(new(), width, height, maxWeight, masks) { }
        public InventoryData(int width, int height, int maxWeight, ulong mask) : this(new(), width, height, maxWeight, mask) { }

        private void Notify(InventoryState state, InventoryResult result, ItemData item = null) => User?.HandleStateChanged(new(state, result, item));

        public void Clear()
        {
            itemTable.Clear();
            itemGrid = new ItemData[GridWidth * GridHeight];

            CurrentWeight = 0;

            Notify(InventoryState.INITIALIZED, InventoryResult.SUCCESS);
        }
        public IReadOnlyCollection<Guid> GetItems() => itemTable.Keys;
        public int GetItemCount(ItemID baseID)
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
        public ItemData[,] GetSnapshot()
        {
            ItemData[,] snapshot = new ItemData[GridWidth, GridHeight];

            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    snapshot[x, y] = itemGrid[(y * GridWidth) + x];
                }
            }

            return snapshot;
        }

        /// <summary> Finds the closest valid position to the desired position. Optional ignore id </summary>
        public bool TryGetNearestPosition(Vector2Int desiredPosition, Vector2Int scale, Guid ignoreID, out Vector2Int position, out InventoryResult result)
        {
            position = Vector2Int.zero;

            if (scale.x <= 0 || scale.y <= 0)
            {
                result = InventoryResult.OUT_OF_BOUNDS;
                return false;
            }

            int maxX = GridWidth - scale.x;
            int maxY = GridHeight - scale.y;

            if (maxX < 0 || maxY < 0)
            {
                result = InventoryResult.OUT_OF_BOUNDS;
                return false;
            }

            Vector2Int clamped = new(Mathf.Clamp(desiredPosition.x, 0, maxX), Mathf.Clamp(desiredPosition.y, 0, maxY));

            if (!TryGetItemByArea(clamped, scale, ignoreID, out _, out _))
            {
                position = clamped;
                result = InventoryResult.SUCCESS;
                return true;
            }

            Vector2Int bestPosition = Vector2Int.zero;
            float bestDistance = float.MaxValue;
            bool found = false;

            for (int y = 0; y <= maxY; y++)
            {
                for (int x = 0; x <= maxX; x++)
                {
                    Vector2Int candidate = new(x, y);

                    if (TryGetItemByArea(candidate, scale, ignoreID, out _, out _))
                    {
                        continue;
                    }

                    float distance = Vector2Int.Distance(candidate, clamped);

                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestPosition = candidate;
                        found = true;
                    }
                }
            }

            if (!found)
            {
                result = InventoryResult.NO_VALID_SPACE;
                return false;
            }

            position = bestPosition;
            result = InventoryResult.SUCCESS;
            return true;
        }
        /// <summary> Finds a valid position and rotation for the given item. </summary>
        public bool TryGetBestPosition(ItemData item, out Vector2Int bestPosition, out bool bestRotation, out InventoryResult result)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "Try get valid position failed! item is null!?");
            }

            bestRotation = item.GetRotation();
            Vector2Int scale = item.GetScale(bestRotation);

            if (TryGetAnyPosition(scale, out bestPosition, out _))
            {
                result = InventoryResult.SUCCESS;
                return true;
            }

            bestRotation = !bestRotation;
            scale = item.GetScale(bestRotation);

            return TryGetAnyPosition(scale, out bestPosition, out result);
        }
        /// <summary> Clamps a position so the given item scale stays inside the inventory bounds. </summary>
        public bool TryGetClampedPosition(Vector2Int scale, ref Vector2Int position, out InventoryResult result)
        {
            if (scale.x <= 0 || scale.y <= 0)
            {
                result = InventoryResult.OUT_OF_BOUNDS;
                return false;
            }

            if (scale.x > GridWidth || scale.y > GridHeight)
            {
                result = InventoryResult.NO_VALID_SPACE;
                return false;
            }

            int cx = Mathf.Clamp(position.x, 0, GridWidth - scale.x);
            int cy = Mathf.Clamp(position.y, 0, GridHeight - scale.y);

            position = new(cx, cy);

            result = InventoryResult.SUCCESS;
            return true;
        }
        /// <summary> Finds the first valid position for the given item scale in the specified grid. </summary>
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
                    if (!IsTileOverlapping(grid, x, y, scale.x, scale.y, Guid.Empty, out _, out _))
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
        /// <summary> Finds the first item that matches any of the given tags. </summary>
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
        /// <summary> Finds all items that match any of the given tags. </summary>
        public bool TryGetItemsByTag(ulong tags, out List<ItemData> items, out InventoryResult result)
        {
            items = new();
            result = InventoryResult.NOT_REGISTERED;

            foreach (ItemData item in itemTable.Values)
            {
                if (item.Tags.HasAny(tags))
                {
                    items.Add(item);
                    result = InventoryResult.SUCCESS;
                }
            }

            return result == InventoryResult.SUCCESS;
        }
        /// <summary> Finds the first item with the given base ID. </summary>
        public bool TryGetItemByBaseID(ItemID baseID, out ItemData registered, out InventoryResult result)
        {
            foreach (ItemData item in itemTable.Values)
            {
                if (item.BaseID == baseID)
                {
                    result = InventoryResult.SUCCESS;
                    registered = item;
                    return true;
                }
            }

            result = InventoryResult.NOT_REGISTERED;
            registered = null;
            return false;
        }
        /// <summary> Adds all items with the given base ID to the provided list. </summary>
        public bool TryGetItemsByBaseID(ItemID baseID, List<ItemData> registered, out InventoryResult result)
        {
            if (registered == null)
            {
                throw new ArgumentNullException(nameof(registered), $"Get items by base id failed! Registered list cannot be null!");
            }

            result = InventoryResult.NOT_REGISTERED;

            foreach (ItemData item in itemTable.Values)
            {
                if (item.BaseID == baseID)
                {
                    result = InventoryResult.SUCCESS;
                    registered.Add(item);
                }
            }

            return result == InventoryResult.SUCCESS;
        }
        /// <summary> Finds an item by its unique instance ID. </summary>
        public bool TryGetItemByInstanceID(Guid instanceID, out ItemData registered, out InventoryResult result)
        {
            if (!itemTable.TryGetValue(instanceID, out registered))
            {
                result = InventoryResult.NOT_REGISTERED;
                return false;
            }

            result = InventoryResult.SUCCESS;
            return true;
        }
        /// <summary> Finds the item occupying the given grid position. </summary>
        public bool TryGetItemByPosition(Vector2Int position, out ItemData registered, out InventoryResult result)
        {
            if (!IsTileInsideBoundary(position.x, position.y, out result))
            {
                registered = null;
                return false;
            }

            registered = itemGrid[(position.y * GridWidth) + (position.x)];

            bool foundItem = registered != null;

            result = foundItem ? InventoryResult.SUCCESS : InventoryResult.NOT_REGISTERED;

            return foundItem;
        }
        /// <summary> Checks an area for an overlapping item. Optional ignore id </summary>
        public bool TryGetItemByArea(Vector2Int position, Vector2Int scale, Guid ignoreID, out ItemData overlapped, out InventoryResult result)
        {
            overlapped = null;

            if (!IsTileInsideBoundary(position.x, position.y, scale.x, scale.y, out result))
            {
                return false;
            }

            if (!IsTileOverlapping(position.x, position.y, scale.x, scale.y, ignoreID, out overlapped, out result))
            {
                return false;
            }

            return true;
        }
        /// <summary> Finds all items directly adjacent to the given item. </summary>
        public bool TryGetItemsByAdjacent(Guid instanceID, out List<ItemData> items)
        {
            items = new();

            if (!TryGetItemByInstanceID(instanceID, out ItemData item, out _))
            {
                return false;
            }

            HashSet<Guid> visited = new() { instanceID };
            Vector2Int position = item.GetPosition();
            Vector2Int scale = item.GetScale();

            void ScanEdge(List<ItemData> list, int startX, int startY, int width, int height)
            {
                for (int y = startY; y < startY + height; y++)
                {
                    for (int x = startX; x < startX + width; x++)
                    {
                        if (!IsTileInsideBoundary(x, y, out _))
                        {
                            continue;
                        }

                        ItemData found = itemGrid[(y * GridWidth) + x];

                        if (found != null && visited.Add(found.InstanceID))
                        {
                            list.Add(found);
                        }
                    }
                }
            }

            ScanEdge(items, position.x - 1, position.y, 1, scale.y);              // sol kenar
            ScanEdge(items, position.x + scale.x, position.y, 1, scale.y);        // sað kenar
            ScanEdge(items, position.x, position.y - 1, scale.x, 1);              // üst kenar
            ScanEdge(items, position.x, position.y + scale.y, scale.x, 1);        // alt kenar

            return items.Count != 0;
        }
        public bool TryGetItemStack(Guid instanceID, out int stack, out InventoryResult result)
        {
            stack = 0;

            if (!TryGetItemByInstanceID(instanceID, out ItemData registered, out result))
            {
                return false;
            }

            stack = registered.GetStack();
            return true;
        }

        /// <summary> Checks whether the given item can be added at the specified position and rotation. </summary>
        public bool CanAddItem(ItemData item, Vector2Int position, bool isRotated, out InventoryResult result)
        {
            if (item == null)
            {
                result = InventoryResult.NULL;
                throw new ArgumentNullException(nameof(item), "Placement validation failed! item is null!?");
            }

            if (TryGetItemByInstanceID(item.InstanceID, out ItemData _, out _))
            {
                result = InventoryResult.DUPLICATE;
                return false;
            }

            if (!IsPlacementValid(position, item.GetScale(isRotated), Guid.Empty, out result))
            {
                return false;
            }

            if (CurrentWeight + item.GetWeight() > MaximumWeight)
            {
                result = InventoryResult.WEIGHT_LIMIT_EXCEEDED;
                return false;
            }

            result = InventoryResult.SUCCESS;
            return true;
        }
        /// <summary> Checks whether two items can be swapped and calculates their target positions. </summary>
        public bool CanSwapItem(Guid instanceIDA, Guid instanceIDB, InventoryData inventoryB, bool rotationA, out Vector2Int targetPositionA, out Vector2Int targetPositionB, out InventoryResult result)
        {
            // Weight kontrolü yapmýyor burasý. burda kontrol lazým yav
            if (inventoryB == null)
            {
                throw new ArgumentNullException(nameof(inventoryB), "Can swap item failed target inventory is null!?");
            }

            targetPositionA = Vector2Int.one * -1;
            targetPositionB = Vector2Int.one * -1;

            if (!TryGetItemByInstanceID(instanceIDA, out ItemData itemA, out result))
            {
                return false;
            }

            if (!inventoryB.TryGetItemByInstanceID(instanceIDB, out ItemData itemB, out result))
            {
                return false;
            }

            bool sameInventory = inventoryB == this;

            if (sameInventory && itemA.InstanceID == itemB.InstanceID)
            {
                result = InventoryResult.DUPLICATE;
                return false;
            }

            static void getSwapPositions(Vector2Int positionA, Vector2Int scaleA, Vector2Int positionB, Vector2Int scaleB, out Vector2Int targetPositionA, out Vector2Int targetPositionB)
            {
                Vector2Int direction = positionB - positionA;

                if (direction == Vector2Int.zero)
                {
                    targetPositionA = positionB;
                    targetPositionB = positionA;
                    return;
                }

                int directionX = direction.x == 0 ? 0 : (direction.x > 0 ? 1 : -1);
                int directionY = direction.y == 0 ? 0 : (direction.y > 0 ? 1 : -1);

                bool bIsStart = directionX < 0 || directionY < 0;

                if (bIsStart)
                {
                    targetPositionA = positionB;
                    targetPositionB = positionB + new Vector2Int(scaleA.x * -directionX, scaleA.y * -directionY);
                }
                else
                {
                    targetPositionB = positionA;
                    targetPositionA = positionA + new Vector2Int(scaleB.x * directionX, scaleB.y * directionY);
                }
            }
            static bool areAdjacent(Vector2Int positionA, Vector2Int scaleA, Vector2Int positionB, Vector2Int scaleB, out Vector2Int direction)
            {
                direction = Vector2Int.zero;

                if (positionA.y == positionB.y && scaleA.y == scaleB.y)
                {
                    if (positionA.x + scaleA.x == positionB.x) { direction = new Vector2Int(1, 0); return true; }
                    if (positionB.x + scaleB.x == positionA.x) { direction = new Vector2Int(-1, 0); return true; }
                }

                if (positionA.x == positionB.x && scaleA.x == scaleB.x)
                {
                    if (positionA.y + scaleA.y == positionB.y) { direction = new Vector2Int(0, 1); return true; }
                    if (positionB.y + scaleB.y == positionA.y) { direction = new Vector2Int(0, -1); return true; }
                }

                return false;
            }

            Vector2Int scaleA = itemA.GetScale(rotationA);
            Vector2Int scaleB = itemB.GetScale();
            Vector2Int positionA = itemA.GetPosition();
            Vector2Int positionB = itemB.GetPosition();

            if (sameInventory && areAdjacent(positionA, scaleA, positionB, scaleB, out _))
            {
                getSwapPositions(positionA, scaleA, positionB, scaleB, out targetPositionA, out targetPositionB);
            }
            else 
            { 
                targetPositionA = positionB; 
                targetPositionB = positionA; 
            }

            if (!inventoryB.IsPlacementValid(targetPositionA, scaleA, instanceIDB, out result))
            {
                return false;
            }

            if (!IsPlacementValid(targetPositionB, scaleB, instanceIDA, out result))
            {
                return false;
            }

            result = InventoryResult.SUCCESS;
            return true;
        }
        /// <summary> Checks whether an item can be placed at the given position and scale. </summary>
        public bool IsPlacementValid(Vector2Int position, Vector2Int scale, Guid ignoreID, out InventoryResult result)
        {
            if (TryGetItemByArea(position, scale, ignoreID, out _, out result))
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
        private bool IsTileOverlapping(int tilePositionX, int tilePositionY, int tileWidth, int tileHeight, Guid ignoreID,  out ItemData overlapped, out InventoryResult result) => IsTileOverlapping(itemGrid, tilePositionX, tilePositionY, tileWidth, tileHeight, ignoreID, out overlapped, out result);
        private bool IsTileOverlapping(ItemData[] grid, int tilePositionX, int tilePositionY, int tileWidth, int tileHeight, Guid ignoreID, out ItemData overlapped, out InventoryResult result)
        {
            overlapped = null;

            for (int y = 0; y < tileHeight; y++)
            {
                for (int x = 0; x < tileWidth; x++)
                {
                    ItemData registered = grid[((tilePositionY + y) * GridWidth) + (tilePositionX + x)];

                    if (registered == null)
                    {
                        continue;
                    }

                    if (ignoreID == registered.InstanceID)
                    {
                        continue;
                    }

                    overlapped = registered;
                    result = InventoryResult.OVERLAPPING;
                    return true;
                }
            }

            result = InventoryResult.EMPTY;
            return false;
        }
        private bool IsTileInsideBoundary(int tilePositionX, int tilePositionY, int tileWidth, int tileHeight, out InventoryResult result)
        {
            if (!IsTileInsideBoundary(tilePositionX, tilePositionY, out result))
            {
                return false;
            }

            tilePositionX += tileWidth - 1;
            tilePositionY += tileHeight - 1;

            if (!IsTileInsideBoundary(tilePositionX, tilePositionY, out result))
            {
                return false;
            }

            return true;
        }
        private bool IsTileInsideBoundary(int tilePositionX, int tilePositionY, out InventoryResult result)
        {
            result = InventoryResult.OUT_OF_BOUNDS;

            if (tilePositionX < 0 || tilePositionY < 0)
            {
                return false;
            }

            if (tilePositionX >= GridWidth || tilePositionY >= GridHeight)
            {
                return false;
            }

            result = InventoryResult.SUCCESS;
            return true;
        }

        private void SetTileItem(ItemData item, Vector2Int position, Vector2Int scale) => SetTileItem(itemGrid, item, position, scale);
        private void SetTileItem(ItemData[] grid, ItemData item, Vector2Int position, Vector2Int scale)
        {
            if (!IsTileInsideBoundary(position.x, position.y, scale.x, scale.y, out _))
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
        /// <summary> Merges compatible items using an optional stacking rule. </summary>
        public bool TryMergeItems(Func<ItemData, ItemData, bool> canStackPredicate, out InventoryResult result)
        {
            int totalMoved = 0;
            result = InventoryResult.SUCCESS;

            IReadOnlyCollection<Guid> items = GetItems();

            if (items.Count < 2)
            {
                result = InventoryResult.NOT_REGISTERED;
                return false;
            }

            Dictionary<ItemID, List<ItemData>> groups = new();

            foreach (Guid id in items.ToArray())
            {
                if (TryGetItemByInstanceID(id, out ItemData registered, out _))
                {
                    if (!groups.TryGetValue(registered.BaseID, out List<ItemData> list))
                    {
                        list = new();
                        groups[registered.BaseID] = list;
                    }

                    list.Add(registered);
                }
            }

            foreach (List<ItemData> group in groups.Values)
            {
                if (group.Count < 2)
                {
                    continue;
                }

                if (!group[0].IsStackable())
                {
                    continue;
                }

                for (int i = 0; i < group.Count; i++)
                {
                    ItemData target = group[i];

                    if (target == null || !itemTable.ContainsKey(target.InstanceID) || target.GetStack() <= 0)
                    {
                        continue;
                    }

                    for (int j = i + 1; j < group.Count; j++)
                    {
                        ItemData source = group[j];

                        if (source == null || !itemTable.ContainsKey(source.InstanceID))
                        {
                            continue;
                        }

                        if (TryMergeItem(target, source, this, canStackPredicate, out _))
                        {
                            totalMoved++;
                        }
                    }
                }
            }

            if (totalMoved <= 0)
            {
                result = InventoryResult.FAILED;
                return false;
            }

            result = InventoryResult.SUCCESS;
            return true;
        }
        /// <summary> Merges an item using an optional stacking rule. </summary>
        public bool TryMergeItem(Guid targetInstanceID, Guid sourceInstanceID, InventoryData sourceInventory, Func<ItemData, ItemData, bool> canStackPredicate, out InventoryResult result)
        {
            if (sourceInventory == null)
            {
                throw new ArgumentNullException(nameof(sourceInventory), "Merge failed source inventory missing!?");
            }

            if (!TryGetItemByInstanceID(targetInstanceID, out ItemData targetItem, out result))
            {
                return false;
            }

            if (!sourceInventory.TryGetItemByInstanceID(sourceInstanceID, out ItemData sourceItem, out result))
            {
                return false;
            }

            return TryMergeItem(targetItem, sourceItem, sourceInventory, canStackPredicate, out result);
        }
        private bool TryMergeItem(ItemData targetItem, ItemData sourceItem, InventoryData sourceInventory, Func<ItemData, ItemData, bool> canStackPredicate, out InventoryResult result)
        {
            if (sourceInventory == null)
            {
                throw new ArgumentNullException(nameof(sourceInventory), "Item merge failed source inventory missing!?");
            }

            if (sourceItem == null)
            {
                throw new ArgumentNullException(nameof(sourceItem), "Item merge failed source item missing!?");
            }

            if (targetItem == null)
            {
                throw new ArgumentNullException(nameof(targetItem), "Item merge failed target item missing!?");
            }

            if (targetItem.InstanceID == sourceItem.InstanceID)
            {
                Debug.LogError("Trying to merge with duplicate item");
                result = InventoryResult.DUPLICATE;
                return false;
            }

            int maxStack = sourceItem.BaseID.GetDefinition().Stack;

            if (maxStack <= 1)
            {
                result = InventoryResult.STACK_FULL;
                return false;
            }

            if (targetItem.BaseID != sourceItem.BaseID)
            {
                result = InventoryResult.NOT_SUPPORTED;
                return false;
            }

            if (canStackPredicate != null && !canStackPredicate(targetItem, sourceItem))
            {
                result = InventoryResult.NOT_SUPPORTED;
                return false;
            }

            int space = maxStack - targetItem.GetStack();

            if (space <= 0)
            {
                result = InventoryResult.STACK_FULL;
                return false;
            }

            int amount = Mathf.Min(space, sourceItem.GetStack());

            if (amount <= 0)
            {
                result = InventoryResult.NO_VALID_SPACE;
                return false;
            }

            if (sourceInventory != this)
            {
                int sourceStack = sourceItem.GetStack();
                float unitWeight = sourceStack > 0 ? sourceItem.GetWeight() / sourceStack : 0f;

                if (unitWeight > 0f)
                {
                    int maxByWeight = Mathf.FloorToInt((MaximumWeight - CurrentWeight) / unitWeight);
                    amount = Mathf.Min(amount, Mathf.Max(0, maxByWeight));
                }

                if (amount <= 0)
                {
                    result = InventoryResult.WEIGHT_LIMIT_EXCEEDED;
                    return false;
                }
            }

            TrySetItemStack(targetItem, targetItem.GetStack() + amount, out _);
            sourceInventory.TrySetItemStack(sourceItem, sourceItem.GetStack() - amount, out _);

            result = InventoryResult.SUCCESS;
            return true;
        }
        /// <summary> Rearranges items to fill the inventory from the top left without changing their order. </summary>
        public bool TryCompactItems(out InventoryResult result)
        {
            if (itemTable.Count == 0)
            {
                result = InventoryResult.SUCCESS;
                return true;
            }

            List<ItemData> ordered = itemTable.Values.OrderBy(i => i.GetPosition().y).ThenBy(i => i.GetPosition().x).ToList();

            return TryArrangeItems(ordered, out result);
        }
        /// <summary> Sorts and rearranges items using the given inventory sorter. </summary>
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

            return TryArrangeItems(sorted, out result);
        }
        private bool TryArrangeItems(IReadOnlyList<ItemData> items, out InventoryResult result)
        {
            ItemData[] tempGrid = new ItemData[itemGrid.Length];

            List<(ItemData item, Vector2Int position, bool rotated)> placements = new(items.Count);

            foreach (ItemData item in items)
            {
                ItemDefinition definition = item.BaseID.GetDefinition();

                Vector2Int baseScale = new(definition.Width, definition.Height);

                bool rotated = item.GetRotation();

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
                item.SetRotation(rotated);
                item.SetPosition(position);
                Notify( InventoryState.ITEM_CHANGED, InventoryResult.SUCCESS, item);
            }

            itemGrid = tempGrid;
            Notify(InventoryState.SORTED, result = InventoryResult.SUCCESS, null);
            return true;
        }
        public bool TrySetItemStack(Guid instanceID, int stack, out InventoryResult result)
        {
            if (!TryGetItemByInstanceID(instanceID, out ItemData registered, out result))
            {
                return false;
            }

            return TrySetItemStack(registered, stack, out result);
        }
        private bool TrySetItemStack(ItemData item, int stack, out InventoryResult result)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "Set item stack failed item missing!?");
            }

            float previousWeight = item.GetWeight();

            item.SetStack(stack);

            float currentWeight = item.GetWeight();

            CurrentWeight = Mathf.Max(0f, CurrentWeight + (currentWeight - previousWeight));

            if (item.GetStack() <= 0)
            {
                return TryRemoveItem(item.InstanceID, out _, out result);
            }

            Notify(InventoryState.ITEM_CHANGED, result = InventoryResult.SUCCESS, item);
            return true;
        }
        /// <summary> Transfers all items that can fit into the target inventory. </summary>
        public bool TryTransferItems(InventoryData inventory, out InventoryResult result)
        {
            result = InventoryResult.NULL;

            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory), "Inventory transfer items failed target inventory is null!");
            }

            if (inventory == this)
            {
                Debug.LogError("Transfer items failed! target inventory same as current inventory!?");
                return false;
            }

            bool transfered = false;

            foreach (Guid id in GetItems().ToArray())
            {
                if (!TryGetItemByInstanceID(id, out ItemData registered, out result))
                {
                    continue;
                }

                if (!inventory.TryAddItem(registered, null, null, out ItemData transferred, out _))
                {
                    continue;
                }

                if (TryRemoveItem(id, out _, out _))
                {
                    transfered = true;
                    continue;
                }

                if (!inventory.TryRemoveItem(transferred.InstanceID, out _, out _))
                {
                    Debug.LogError($"CRITICAL: Transfer rollback failed! Item [{transferred.InstanceID}] may now exist in two inventories.");
                }
            }

            result = transfered ? InventoryResult.SUCCESS : InventoryResult.NOT_REGISTERED;
            return transfered;
        }
        /// <summary> Transfers one item to the target inventory. </summary>
        public bool TryTransferItem(Guid instanceID, Vector2Int? position, bool? isRotated, InventoryData inventory, out ItemData transfered, out InventoryResult result)
        {
            transfered = null;

            if (inventory == null)
            {
                result = InventoryResult.NULL;
                throw new ArgumentNullException(nameof(inventory), $"Inventory transfer item failed target inventory is null!");
            }

            if (!TryGetItemByInstanceID(instanceID, out ItemData registered, out result))
            {
                Debug.LogError($"Inventory transfer item failed! [{instanceID}] not found!");
                return false;
            }

            if (inventory.TryAddItem(registered, position, isRotated, out transfered, out result))
            {
                if (TryRemoveItem(instanceID, out _, out result))
                {
                    Notify(InventoryState.ITEM_TRANSFERED, result, transfered);
                    return true;
                }

                if (!inventory.TryRemoveItem(transfered.InstanceID, out _, out _))
                {
                    Debug.LogError($"CRITICAL: Transfer rollback failed! Item [{transfered.InstanceID}] may now exist in two inventories.");
                }

                transfered = null;
            }

            return false;
        }
        /// <summary> Adds an item to the inventory at the requested or best available position. </summary>
        public bool TryAddItem(ItemData item, Vector2Int? position, bool? isRotated, out ItemData registered, out InventoryResult result)
        {
            registered = null;

            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "Add item failed item missing!?");
            }

            if (!item.Tags.HasAny(ItemMask))
            {
                result = InventoryResult.NOT_SUPPORTED;
                return false;
            }

            Vector2Int bestPosition;
            bool bestRotation = isRotated ?? item.GetRotation();

            if (position.HasValue)
            {
                bestPosition = position.Value;
            }
            else
            {
                if (!TryGetBestPosition(item, out bestPosition, out bestRotation, out result))
                {
                    return false;
                }
            }

            if (!CanAddItem(item, bestPosition, bestRotation, out result))
            {
                return false;
            }

            registered = new(item);
            registered.SetPosition(bestPosition);
            registered.SetRotation(bestRotation);

            SetTileItem(registered, registered.GetPosition(), registered.GetScale());

            itemTable[registered.InstanceID] = registered;
            CurrentWeight += registered.GetWeight();

            Notify(InventoryState.ITEM_ADDED, result = InventoryResult.SUCCESS, registered);
            return true;
        }
        /// <summary> Removes an item and creates its world entity with the given drop force. </summary>
        public bool TryDropItem(Guid instanceID, Vector3 position, Vector3 force, out ItemData registered, out InventoryResult result)
        {
            if (!TryRemoveItem(instanceID, out registered, out result))
            {
                return false;
            }

            ItemEntity entity = ItemDatabase.CreateEntity(registered, position, Quaternion.identity);

            if (entity == null)
            {
                result = InventoryResult.NULL;
                return false;
            }

            Notify(InventoryState.ITEM_DROPPED, result, registered);

            if (entity.TryGetComponent(out Rigidbody body))
            {
                if (body.isKinematic)
                {
                    body.isKinematic = false;
                }

                body.useGravity = true;

                body.AddForce(force + (UnityEngine.Random.onUnitSphere * 0.25f), ForceMode.Impulse);
                body.AddTorque(force + (UnityEngine.Random.onUnitSphere * 0.25f), ForceMode.Impulse);
            }

            return true;
        }
        /// <summary> Removes an item from the inventory and updates its weight. </summary>
        public bool TryRemoveItem(Guid instanceID, out ItemData registered, out InventoryResult result)
        {
            if (TryClearItem(instanceID, out registered, out result))
            {
                CurrentWeight = Mathf.Max(0, CurrentWeight - registered.GetWeight());
                itemTable.Remove(registered.InstanceID);

                Notify(InventoryState.ITEM_REMOVED, result, registered);
                return true;
            }

            return false;
        }
        /// <summary> Clears an item's occupied tiles without removing it from the item table. </summary>
        public bool TryClearItem(Guid instanceID, out ItemData registered, out InventoryResult result)
        {
            if (!TryGetItemByInstanceID(instanceID, out registered, out result))
            {
                Debug.LogError($"Inventory clear item failed! [{instanceID}] not found!");
                return false;
            }

            Vector2Int position = registered.GetPosition();
            Vector2Int scale = registered.GetScale();

            SetTileItem(null, position, scale);
            result = InventoryResult.SUCCESS;
            return true;
        }
        /// <summary> Moves an item to a new position and optionally clears its old tiles. </summary>
        public bool TryMoveItem(Guid instanceID, Vector2Int position, bool isRotated, bool clearOldTile, out ItemData registered, out InventoryResult result)
        {
            if (!TryGetItemByInstanceID(instanceID, out registered, out result))
            {
                Debug.LogError($"Inventory place item failed! [{instanceID}] not found!");
                return false;
            }

            Vector2Int oldPosition = registered.GetPosition();
            Vector2Int oldScale = registered.GetScale();
            Vector2Int newScale = registered.GetScale(isRotated);

            if (!IsPlacementValid(position, newScale, instanceID, out result))
            {
                return false;
            }

            if (clearOldTile)
            {
                SetTileItem(null, oldPosition, oldScale);
            }

            registered.SetPosition(position);
            registered.SetRotation(isRotated);
            SetTileItem(registered, position, newScale);
            Notify(InventoryState.ITEM_CHANGED, result = InventoryResult.SUCCESS, registered);
            return true;
        }
        /// <summary> Swaps two items between the same or different inventories. </summary>
        public bool TrySwapItem(Guid instanceIDA, Guid instanceIDB, InventoryData inventoryB, bool rotationA, out InventoryResult result)
        {
            if (inventoryB == null)
            {
                throw new ArgumentNullException(nameof(inventoryB), "Swap item failed target inventory is null!?");
            }

            if (!CanSwapItem(instanceIDA, instanceIDB, inventoryB, rotationA, out Vector2Int targetPositionA, out Vector2Int targetPositionB, out result))
            {
                return false;
            }

            if (!TryGetItemByInstanceID(instanceIDA, out ItemData itemA, out result))
            {
                return false;
            }

            if (!inventoryB.TryGetItemByInstanceID(instanceIDB, out ItemData itemB, out result))
            {
                return false;
            }

            bool defaultRotationA = itemA.GetRotation();
            bool defaultRotationB = itemB.GetRotation();
            Vector2Int defaultPositionA = itemA.GetPosition();
            Vector2Int defaultPositionB = itemB.GetPosition();

            // 2. [B][-][-][-]
            if (!TryRemoveItem(instanceIDA, out ItemData removedA, out result))
            {
                return false;
            }

            // 3. [-][-][-][-]
            if (!inventoryB.TryRemoveItem(instanceIDB, out ItemData removedB, out result))
            {
                if (!TryAddItem(removedA, defaultPositionA, defaultRotationA, out _, out InventoryResult rollbackA))
                {
                    Debug.LogError($"CRITICAL: Swap rollback failed! Item [{removedA.InstanceID}] could not be restored — {rollbackA}.");
                    return false;
                }
            }

            // 4. [A][A][A][-]
            if (!inventoryB.TryAddItem(removedA, targetPositionA, rotationA, out ItemData placedA, out result))
            {
                if (!TryAddItem(removedA, defaultPositionA, defaultRotationA, out _, out InventoryResult rollbackA))
                {
                    Debug.LogError($"CRITICAL: Swap rollback failed! Item [{removedA.InstanceID}] could not be restored — {rollbackA}.");
                }

                if (!inventoryB.TryAddItem(removedB, defaultPositionB, defaultRotationB, out _, out InventoryResult rollbackB))
                {
                    Debug.LogError($"CRITICAL: Swap rollback failed! Item [{removedB.InstanceID}] could not be restored — {rollbackB}.");
                }

                return false;
            }

            // 4. [A][A][A][B]
            if (!TryAddItem(removedB, targetPositionB, defaultRotationB, out _, out result))
            {
                if (!inventoryB.TryRemoveItem(placedA.InstanceID, out _, out InventoryResult undoA))
                {
                    Debug.LogError($"CRITICAL: Swap rollback failed! Item [{placedA.InstanceID}] could not be pulled back — {undoA}.");
                }

                if (!TryAddItem(removedA, defaultPositionA, defaultRotationA, out _, out InventoryResult rollbackA))
                {
                    Debug.LogError($"CRITICAL: Swap rollback failed! Item [{removedA.InstanceID}] could not be restored — {rollbackA}.");
                }

                if (!inventoryB.TryAddItem(removedB, defaultPositionB, defaultRotationB, out _, out InventoryResult rollbackB))
                {
                    Debug.LogError($"CRITICAL: Swap rollback failed! Item [{removedB.InstanceID}] could not be restored — {rollbackB}.");
                }
                return false;
            }

            result = InventoryResult.SUCCESS;
            return true;
        }
    }
}