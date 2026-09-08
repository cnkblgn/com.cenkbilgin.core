using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Localization;

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
        [SerializeField] private ItemID[] startingItems;
        [SerializeField] private ItemTag[] whitelistedItems;

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
        public int GetItemCount(ItemID baseID) => thisInventory.GetItemCount(baseID);
        public ItemData[,] GetSnapshot() => thisInventory.GetSnapshot();

        public bool TryGetNearestPosition(Vector2Int desiredPosition, Vector2Int scale, out Vector2Int position, out InventoryResult result) => TryGetNearestPosition(desiredPosition, scale, Guid.Empty, out position, out result);
        public bool TryGetNearestPosition(Vector2Int desiredPosition, Vector2Int scale, Guid ignoreID, out Vector2Int position, out InventoryResult result) => thisInventory.TryGetNearestPosition(desiredPosition, scale, ignoreID, out position, out result);
        public bool TryGetBestPosition(ItemData item, out Vector2Int position, out bool isRotated, out InventoryResult result) => thisInventory.TryGetBestPosition(item, out position, out isRotated, out result);
        public bool TryGetClampedPosition(Vector2Int scale, ref Vector2Int position, out InventoryResult result) => thisInventory.TryGetClampedPosition(scale, ref position, out result);
        public bool TryGetAnyPosition(Vector2Int scale, out Vector2Int position, out InventoryResult result) => thisInventory.TryGetAnyPosition(scale, out position, out result);
        public bool TryGetItemByTag(ItemTag tag, out ItemData registered, out InventoryResult result) => thisInventory.TryGetItemByTag(tag, out registered, out result);
        public bool TryGetItemByTag(ulong tags, out ItemData registered, out InventoryResult result) => thisInventory.TryGetItemByTag(tags, out registered, out result);
        public bool TryGetItemsByTag(ItemTag[] tags, out List<ItemData> registered, out InventoryResult result) => thisInventory.TryGetItemsByTag(tags, out registered, out result);
        public bool TryGetItemsByTag(ulong tags, out List<ItemData> registered, out InventoryResult result) => thisInventory.TryGetItemsByTag(tags, out registered, out result);
        public bool TryGetItemByBaseID(ItemID baseID, out ItemData registered, out InventoryResult result) => thisInventory.TryGetItemByBaseID(baseID, out registered, out result);
        public bool TryGetItemsByBaseID(ItemID baseID, List<ItemData> registered, out InventoryResult result) => thisInventory.TryGetItemsByBaseID(baseID, registered, out result);
        public bool TryGetItemByInstanceID(Guid instanceID, out ItemData registered, out InventoryResult result) => thisInventory.TryGetItemByInstanceID(instanceID, out registered, out result);
        public bool TryGetItemByPosition(Vector2Int position, out ItemData registered, out InventoryResult result) => thisInventory.TryGetItemByPosition(position, out registered, out result);
        public bool TryGetItemByArea(Vector2Int position, Vector2Int scale, out ItemData overlapped, out InventoryResult ctx) => TryGetItemByArea(position, scale, Guid.Empty, out overlapped, out ctx);
        public bool TryGetItemByArea(Vector2Int position, Vector2Int scale, Guid ignoreID, out ItemData overlapped, out InventoryResult ctx) => thisInventory.TryGetItemByArea(position, scale, ignoreID, out overlapped, out ctx);
        public bool TryGetItemsByAdjacent(Guid instanceID, out List<ItemData> items) => thisInventory.TryGetItemsByAdjacent(instanceID, out items);

        public bool CanAddItem(ItemData item, Vector2Int position, bool isRotated, out InventoryResult result) => thisInventory.CanAddItem(item, position, isRotated, out result);
        public bool CanSwapItem(Guid instanceIDA, Guid instanceIDB, InventoryEntity inventoryB, bool rotationA, out Vector2Int targetPositionA, out Vector2Int targetPositionB, out InventoryResult result) => thisInventory.CanSwapItem(instanceIDA, instanceIDB, inventoryB.thisInventory, rotationA, out targetPositionA, out targetPositionB, out result);
        public bool CanSwapItem(Guid instanceIDA, Guid instanceIDB, bool rotationA, out Vector2Int targetPositionA, out Vector2Int targetPositionB, out InventoryResult result) => CanSwapItem(instanceIDA, instanceIDB, this, rotationA, out targetPositionA, out targetPositionB, out result);
        public bool IsPlacementValid(Vector2Int position, Vector2Int scale, out InventoryResult result) => IsPlacementValid(position, scale, Guid.Empty, out result);
        public bool IsPlacementValid(Vector2Int position, Vector2Int scale, Guid ignoreID, out InventoryResult result) => thisInventory.IsPlacementValid(position, scale, ignoreID, out result);

        public bool TryCompactItems(out InventoryResult result) => thisInventory.TryCompactItems(out result);
        public bool TrySortItems(IInventorySorter sorter, out InventoryResult result) => thisInventory.TrySortItems(sorter, out result);
        public bool TrySortItemsByArea(bool descending, out InventoryResult result) => thisInventory.TrySortItems(descending ? InventorySorter.SortByAreaDescending : InventorySorter.SortByArea, out result);
        public bool TrySortItemsByTag(out InventoryResult result) => thisInventory.TrySortItems(InventorySorter.SortByTag, out result);
        public bool TrySortItemsByTag(IReadOnlyList<ItemTag> tags, out InventoryResult result) => thisInventory.TrySortItems(new InventorySortByTag(tags), out result);
        public bool TryMergeItems(out InventoryResult result) => TryMergeItems(null, out result);
        public bool TryMergeItems(Func<ItemData, ItemData, bool> canStackPredicate, out InventoryResult result) => thisInventory.TryMergeItems(canStackPredicate, out result);
        public bool TryMergeItem(Guid targetInstanceID, Guid sourceInstanceID, out InventoryResult result) => TryMergeItem(targetInstanceID, sourceInstanceID, this, null, out result);
        public bool TryMergeItem(Guid targetInstanceID, Guid sourceInstanceID, Func<ItemData, ItemData, bool> canStackPredicate, out InventoryResult result) => TryMergeItem(targetInstanceID, sourceInstanceID, this, canStackPredicate, out result);
        public bool TryMergeItem(Guid targetInstanceID, Guid sourceInstanceID, InventoryEntity sourceInventory,  out InventoryResult result) => TryMergeItem(targetInstanceID, sourceInstanceID, sourceInventory, null, out result);
        public bool TryMergeItem(Guid targetInstanceID, Guid sourceInstanceID, InventoryEntity sourceInventory, Func<ItemData, ItemData, bool> canStackPredicate, out InventoryResult result)
        {
            if (sourceInventory == null)
            {
                throw new ArgumentNullException(nameof(sourceInventory), "Try merge teim failed! source inventory is missing!?");
            }

            return thisInventory.TryMergeItem(targetInstanceID, sourceInstanceID, sourceInventory.thisInventory, canStackPredicate, out result);
        }
        public bool TryGetItemStack(Guid instanceID, out int stack, out InventoryResult result) => thisInventory.TryGetItemStack(instanceID, out stack, out result);
        public bool TrySetItemStack(Guid instanceID, int stack, out InventoryResult result) => thisInventory.TrySetItemStack(instanceID, stack, out result);
        public bool TryTransferItem(Guid instanceID, Vector2Int? position, bool? isRotated, InventoryEntity inventory, out ItemData transfered, out InventoryResult result)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory), "Try transfer items failed! inventory is missing!?");
            }

            return thisInventory.TryTransferItem(instanceID, position, isRotated, inventory.thisInventory, out transfered, out result);
        }
        public bool TryTransferItems(InventoryEntity inventory, out InventoryResult result)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory), "Try transfer items failed! inventory is missing!?");
            }

            return thisInventory.TryTransferItems(inventory.thisInventory, out result);
        }
        public bool TryAddItem(ItemData item, Vector2Int? position, bool? isRotated, out ItemData registered, out InventoryResult result) => thisInventory.TryAddItem(item, position, isRotated, out registered, out result);
        public bool TryDropItem(Guid instanceID, out ItemData registered, out InventoryResult result) => thisInventory.TryDropItem(instanceID, dropOrigin.position, dropForce * dropOrigin.forward, out registered, out result); 
        public bool TryRemoveItem(Guid instanceID, out ItemData registered, out InventoryResult result) => thisInventory.TryRemoveItem(instanceID, out registered, out result);
        public bool TryClearItem(Guid instanceID, out ItemData registered, out InventoryResult result) => thisInventory.TryClearItem(instanceID, out registered, out result);
        public bool TryMoveItem(Guid instanceID, Vector2Int position, bool isRotated, out ItemData registered, out InventoryResult ctx) => thisInventory.TryMoveItem(instanceID, position, isRotated, out registered, out ctx);
        public bool TrySwapItem(Guid instanceIDA, Guid instanceIDB, bool rotationA, out InventoryResult result) => TrySwapItem(instanceIDA, instanceIDB, this, rotationA, out result);
        public bool TrySwapItem(Guid instanceIDA, Guid instanceIDB, InventoryEntity inventoryB, bool rotationA, out InventoryResult result) => thisInventory.TrySwapItem(instanceIDA, instanceIDB, inventoryB.thisInventory, rotationA, out result);
    }
}
