using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Localization;
using System.Linq;

namespace Core.Item
{
    using static CoreUtility;
    using static InventoryData;

    [DisallowMultipleComponent]
    public sealed class InventoryEntity : MonoBehaviour, IInventoryUser
    {
        public event Action<InventoryContext> OnStateChanged = null;

        public string Name => name.Get();

        [Header("_")]
        [SerializeField] private new LocalizedID name;

        [Header("_")]
        [SerializeField] private ItemID[] startingItems = new ItemID[] { };
        [SerializeField] private ItemTag[] whitelistedItems = new ItemTag[] {};

        [Header("_")]
        [SerializeField, Range(MIN_WIDTH, MAX_WIDTH)] private int width = 5;
        [SerializeField, Range(MIN_HEIGHT, MAX_HEIGHT)] private int height = 5;
        [SerializeField, Range(MIN_WEIGHT, MAX_WEIGHT)] private int weight = 100;

        [Header("_")]
        [SerializeField, Required] private Transform dropOrigin = null;
        [SerializeField] private float dropForce = 5;

        private InventoryData thisInventory = null;
        private IInventoryHandler thisHandler = null;

        private void Awake()
        {
            thisHandler = GetComponent<IInventoryHandler>();
            thisInventory = new(width, height, weight, whitelistedItems);

            foreach (ItemID item in startingItems)
            {
                thisInventory.TryAddItem(item.CreateData(), null, null, out ItemData _, out InventoryResult _);
            }
        }
        private void Start() => Initialize();

        public void HandleStateChanged(InventoryContext ctx)
        {
            OnStateChanged?.Invoke(ctx);
            thisHandler?.HandleStateChanged(in ctx);
        }

        public static InventoryEntity Create(int width, int height, int weight)
        {
            GameObject @object = new("_DO_NOT_DELETE_ORPHAN_INVENTORY_!", typeof(InventoryEntity));
            InventoryEntity entity = @object.GetComponent<InventoryEntity>();

            entity.startingItems = Array.Empty<ItemID>();
            entity.whitelistedItems = ItemDatabase.GetTags().ToArray();
            entity.width = width;
            entity.height = height;
            entity.weight = weight;
            entity.thisInventory = new(width, height, weight, entity.whitelistedItems);

            return entity;
        }
        private void Initialize()
        {
            thisInventory.User = this;
            HandleStateChanged(new(InventoryState.INITIALIZED, InventoryResult.SUCCESS, null));
        }
        public void Clear() => thisInventory.Clear();
        public void ImportFrom(InventoryData inventory) { thisInventory = new(inventory); Initialize(); }
        public void ExportTo(out InventoryData inventory) => inventory = new(thisInventory);

        public int GetCurrentCapacity() => thisInventory.CurrentCapacity;
        public int GetMaximumCapacity() => thisInventory.MaximumCapacity;
        public float GetCurrentWeight() => thisInventory.CurrentWeight;
        public int GetMaximumWeight() => thisInventory.MaximumWeight;
        public Vector2Int GetDimensions() => new(thisInventory.GridWidth, thisInventory.GridHeight);
        public IReadOnlyCollection<Guid> GetItems() => thisInventory.GetItems();
        public int GetItemCount() => GetItems().Count;
        public int GetItemCount(ItemID baseID) => thisInventory.GetItemCount(baseID);
        public ItemData[,] GetSnapshot() => thisInventory.GetSnapshot();

        /// <summary> Finds the closest valid position to the desired position. </summary>
        public bool TryGetNearestPosition(Vector2Int desiredPosition, Vector2Int scale, out Vector2Int position, out InventoryResult result) => TryGetNearestPosition(desiredPosition, scale, Guid.Empty, out position, out result);
        /// <summary> Finds the closest valid position to the desired position. Optional ignore id </summary>
        public bool TryGetNearestPosition(Vector2Int desiredPosition, Vector2Int scale, Guid ignoreID, out Vector2Int position, out InventoryResult result) => thisInventory.TryGetNearestPosition(desiredPosition, scale, ignoreID, out position, out result);
        /// <summary> Finds a valid position and rotation for the given item. </summary>
        public bool TryGetBestPosition(ItemData item, out Vector2Int position, out bool isRotated, out InventoryResult result) => thisInventory.TryGetBestPosition(item, out position, out isRotated, out result);
        /// <summary> Clamps a position so the given item scale stays inside the inventory bounds. </summary>
        public bool TryGetClampedPosition(Vector2Int scale, ref Vector2Int position, out InventoryResult result) => thisInventory.TryGetClampedPosition(scale, ref position, out result);
        /// <summary> Finds the first valid position for the given item scale in the specified grid. </summary>
        public bool TryGetAnyPosition(Vector2Int scale, out Vector2Int position, out InventoryResult result) => thisInventory.TryGetAnyPosition(scale, out position, out result);
        /// <summary> Finds the first item that matches any of the given tags. </summary>
        public bool TryGetItemByTag(ItemTag tag, out ItemData registered, out InventoryResult result) => thisInventory.TryGetItemByTag(tag.Mask, out registered, out result);
        /// <summary> Finds the first item that matches any of the given tags. </summary>
        public bool TryGetItemByTag(ulong tags, out ItemData registered, out InventoryResult result) => thisInventory.TryGetItemByTag(tags, out registered, out result);
        /// <summary> Finds all items that match any of the given tags. </summary>
        public bool TryGetItemsByTag(ItemTag[] tags, out List<ItemData> registered, out InventoryResult result) => thisInventory.TryGetItemsByTag(tags.CreateMask(), out registered, out result);
        /// <summary> Finds all items that match any of the given tags. </summary>
        public bool TryGetItemsByTag(ulong tags, out List<ItemData> registered, out InventoryResult result) => thisInventory.TryGetItemsByTag(tags, out registered, out result);
        /// <summary> Finds the first item with the given base ID. </summary>
        public bool TryGetItemByBaseID(ItemID baseID, out ItemData registered, out InventoryResult result) => thisInventory.TryGetItemByBaseID(baseID, out registered, out result);
        /// <summary> Adds all items with the given base ID to the provided list. </summary>
        public bool TryGetItemsByBaseID(ItemID baseID, List<ItemData> registered, out InventoryResult result) => thisInventory.TryGetItemsByBaseID(baseID, registered, out result);
        /// <summary> Finds an item by its unique instance ID. </summary>
        public bool TryGetItemByInstanceID(Guid instanceID, out ItemData registered, out InventoryResult result) => thisInventory.TryGetItemByInstanceID(instanceID, out registered, out result);
        /// <summary> Finds the item occupying the given grid position. </summary>
        public bool TryGetItemByPosition(Vector2Int position, out ItemData registered, out InventoryResult result) => thisInventory.TryGetItemByPosition(position, out registered, out result);
        /// <summary> Checks an area for an overlapping item. </summary>
        public bool TryGetItemByArea(Vector2Int position, Vector2Int scale, out ItemData overlapped, out InventoryResult ctx) => TryGetItemByArea(position, scale, Guid.Empty, out overlapped, out ctx);
        /// <summary> Checks an area for an overlapping item. Optional ignore id </summary>
        public bool TryGetItemByArea(Vector2Int position, Vector2Int scale, Guid ignoreID, out ItemData overlapped, out InventoryResult ctx) => thisInventory.TryGetItemByArea(position, scale, ignoreID, out overlapped, out ctx);
        /// <summary> Finds all items directly adjacent to the given item. </summary>
        public bool TryGetItemsByAdjacent(Guid instanceID, out List<ItemData> items) => thisInventory.TryGetItemsByAdjacent(instanceID, out items);

        /// <summary> Checks whether the given item can be added at the specified position and rotation. </summary>
        public bool CanAddItem(ItemData item, Vector2Int position, bool isRotated, out InventoryResult result) => thisInventory.CanAddItem(item, position, isRotated, out result);
        /// <summary> Checks whether two items can be swapped and calculates their target positions. </summary>
        public bool CanSwapItem(Guid instanceIDA, Guid instanceIDB, InventoryEntity inventoryB, bool rotationA, out Vector2Int targetPositionA, out Vector2Int targetPositionB, out InventoryResult result) => thisInventory.CanSwapItem(instanceIDA, instanceIDB, inventoryB.thisInventory, rotationA, out targetPositionA, out targetPositionB, out result);
        /// <summary> Checks whether two items can be swapped and calculates their target positions. </summary>
        public bool CanSwapItem(Guid instanceIDA, Guid instanceIDB, bool rotationA, out Vector2Int targetPositionA, out Vector2Int targetPositionB, out InventoryResult result) => CanSwapItem(instanceIDA, instanceIDB, this, rotationA, out targetPositionA, out targetPositionB, out result);
        /// <summary> Checks whether an item can be placed at the given position and scale. </summary>
        public bool IsPlacementValid(Vector2Int position, Vector2Int scale, out InventoryResult result) => IsPlacementValid(position, scale, Guid.Empty, out result);
        /// <summary> Checks whether an item can be placed at the given position and scale. Optional ignore id </summary>
        public bool IsPlacementValid(Vector2Int position, Vector2Int scale, Guid ignoreID, out InventoryResult result) => thisInventory.IsPlacementValid(position, scale, ignoreID, out result);

        /// <summary> Merges compatible items without any rule. </summary>
        public bool TryMergeItems(out InventoryResult result) => TryMergeItems(null, out result);
        /// <summary> Merges compatible items using an optional stacking rule. </summary>
        public bool TryMergeItems(Func<ItemData, ItemData, bool> canStackPredicate, out InventoryResult result) => thisInventory.TryMergeItems(canStackPredicate, out result);
        /// <summary> Merges an item without any rule. </summary>
        public bool TryMergeItem(Guid targetInstanceID, Guid sourceInstanceID, out InventoryResult result) => TryMergeItem(targetInstanceID, sourceInstanceID, this, null, out result);
        /// <summary> Merges an item using an optional stacking rule. </summary>
        public bool TryMergeItem(Guid targetInstanceID, Guid sourceInstanceID, Func<ItemData, ItemData, bool> canStackPredicate, out InventoryResult result) => TryMergeItem(targetInstanceID, sourceInstanceID, this, canStackPredicate, out result);
        /// <summary> Merges an item without any rule. </summary>
        public bool TryMergeItem(Guid targetInstanceID, Guid sourceInstanceID, InventoryEntity sourceInventory,  out InventoryResult result) => TryMergeItem(targetInstanceID, sourceInstanceID, sourceInventory, null, out result);
        /// <summary> Merges an item using an optional stacking rule. </summary>
        public bool TryMergeItem(Guid targetInstanceID, Guid sourceInstanceID, InventoryEntity sourceInventory, Func<ItemData, ItemData, bool> canStackPredicate, out InventoryResult result)
        {
            if (sourceInventory == null)
            {
                throw new ArgumentNullException(nameof(sourceInventory), "Try merge teim failed! source inventory is missing!?");
            }

            return thisInventory.TryMergeItem(targetInstanceID, sourceInstanceID, sourceInventory.thisInventory, canStackPredicate, out result);
        }
        /// <summary> Tries to split item with given target stack. Returns false if copy cannot be added inventory! </summary>
        public bool TrySplitItem(Guid instanceID, int value, out ItemData copy, out InventoryResult result) => thisInventory.TrySplitItem(instanceID, value, out copy, out result);
        /// <summary> Rearranges items to fill the inventory from the top left without changing their order. </summary>
        public bool TryCompactItems(out InventoryResult result) => thisInventory.TryCompactItems(out result);
        /// <summary> Sorts and rearranges items using the given inventory sorter. </summary>
        public bool TrySortItems(IInventorySorter sorter, out InventoryResult result) => thisInventory.TrySortItems(sorter, out result);
        /// <summary> Sorts and rearranges items using item area with order. </summary>
        public bool TrySortItemsByArea(bool descending, out InventoryResult result) => thisInventory.TrySortItems(descending ? InventorySorter.SortByAreaDescending : InventorySorter.SortByArea, out result);
        /// <summary> Sorts and rearranges items using the default tags. </summary>
        public bool TrySortItemsByTag(out InventoryResult result) => thisInventory.TrySortItems(InventorySorter.SortByTag, out result);
        /// <summary> Sorts and rearranges items using tag priority list. </summary>
        public bool TrySortItemsByTag(IReadOnlyList<ItemTag> tags, out InventoryResult result) => thisInventory.TrySortItems(new InventorySortByTag(tags), out result);
        public bool TryGetItemStack(Guid instanceID, out int stack, out InventoryResult result) => thisInventory.TryGetItemStack(instanceID, out stack, out result);
        public bool TrySetItemStack(Guid instanceID, int stack, out InventoryResult result) => thisInventory.TrySetItemStack(instanceID, stack, out result);
        /// <summary> Transfers all items that can fit into the target inventory. </summary>
        public bool TryTransferItems(InventoryEntity inventory, out InventoryResult result)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory), "Try transfer items failed! inventory is missing!?");
            }

            return thisInventory.TryTransferItems(inventory.thisInventory, out result);
        }
        /// <summary> Transfers one item to the target inventory. </summary>
        public bool TryTransferItem(Guid instanceID, Vector2Int? position, bool? isRotated, InventoryEntity inventory, out ItemData transfered, out InventoryResult result)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory), "Try transfer items failed! inventory is missing!?");
            }

            return thisInventory.TryTransferItem(instanceID, position, isRotated, inventory.thisInventory, out transfered, out result);
        }
        /// <summary> Adds an item to the inventory at the requested or best available position. </summary>
        public bool TryAddItem(ItemData item, Vector2Int? position, bool? isRotated, out ItemData registered, out InventoryResult result) => thisInventory.TryAddItem(item, position, isRotated, out registered, out result);
        /// <summary> Removes an item and creates its world entity with the given drop force. </summary>
        public bool TryDropItem(Guid instanceID, out ItemData registered, out InventoryResult result) => thisInventory.TryDropItem(instanceID, dropOrigin.position, dropForce * dropOrigin.forward, out registered, out result);
        /// <summary> Removes an item from the inventory and updates its weight. </summary>
        public bool TryRemoveItem(Guid instanceID, out ItemData registered, out InventoryResult result) => thisInventory.TryRemoveItem(instanceID, out registered, out result);
        /// <summary> Clears an item's occupied tiles without removing it from the item table. </summary>
        public bool TryClearItem(Guid instanceID, out ItemData registered, out InventoryResult result) => thisInventory.TryClearItem(instanceID, out registered, out result);
        /// <summary> Moves an item to a new position and optionally clears its old tiles. </summary>
        public bool TryMoveItem(Guid instanceID, Vector2Int position, bool isRotated, bool clearOldTile, out ItemData registered, out InventoryResult ctx) => thisInventory.TryMoveItem(instanceID, position, isRotated, clearOldTile, out registered, out ctx);
        /// <summary> Swaps two items between the same or different inventories. </summary>
        public bool TrySwapItem(Guid instanceIDA, Guid instanceIDB, bool rotationA, out InventoryResult result) => TrySwapItem(instanceIDA, instanceIDB, this, rotationA, out result);
        /// <summary> Swaps two items between the same or different inventories. </summary>
        public bool TrySwapItem(Guid instanceIDA, Guid instanceIDB, InventoryEntity inventoryB, bool rotationA, out InventoryResult result) => thisInventory.TrySwapItem(instanceIDA, instanceIDB, inventoryB.thisInventory, rotationA, out result);
    }
}
