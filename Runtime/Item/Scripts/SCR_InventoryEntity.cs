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
                thisInventory.TryAddItem(item.CreateData(), null, out ItemData _, out InventoryResult _);
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
        public int GetMaxWeight() => thisInventory.MaximumWeight;
        public Vector2Int GetDimensions() => new(thisInventory.GridWidth, thisInventory.GridHeight);
        public IReadOnlyCollection<Guid> GetItems() => thisInventory.GetItems();
        public int GetItems(ItemID baseID) => thisInventory.GetItems(baseID);
        public bool TryGetValidPositionForItem(ItemData item, out Vector2Int position, out InventoryResult result) => this.thisInventory.TryGetValidPosition(item, out position, out result);
        public bool TryGetClampedPosition(Vector2Int scale, ref Vector2Int position, out InventoryResult result) => thisInventory.TryGetClampedPosition(scale, ref position, out result);
        public bool TryGetAnyPosition(Vector2Int scale, out Vector2Int position, out InventoryResult result) => thisInventory.TryGetAnyPosition(scale, out position, out result);
        public bool TryGetItemByTag(ItemTag tag, out ItemData registered, out InventoryResult result) => thisInventory.TryGetItemByTag(tag, out registered, out result);
        public bool TryGetItemByTag(ulong tags, out ItemData registered, out InventoryResult result) => thisInventory.TryGetItemByTag(tags, out registered, out result);
        public bool TryGetItemsByTag(ItemTag[] tags, out List<ItemData> registered, out InventoryResult result) => thisInventory.TryGetItemsByTag(tags, out registered, out result);
        public bool TryGetItemsByTag(ulong tags, out List<ItemData> registered, out InventoryResult result) => thisInventory.TryGetItemsByTag(tags, out registered, out result);
        public bool TryGetItemByBaseID(ItemID baseID, out ItemData registered) => thisInventory.TryGetItemByBaseID(baseID, out registered);
        public bool TryGetItemsByBaseID(ItemID baseID, List<ItemData> registered) => thisInventory.TryGetItemsByBaseID(baseID, registered);
        public bool TryGetItemByInstanceID(Guid instanceID, out ItemData registered) => thisInventory.TryGetItemByInstanceID(instanceID, out registered);
        public bool TryGetItemByPosition(Vector2Int position, out ItemData registered) => thisInventory.TryGetItemByPosition(position, out registered);
        public bool TryGetItemByArea(Vector2Int scale, Vector2Int position, out ItemData overlapped, out InventoryResult ctx) => thisInventory.TryGetItemByArea(scale, position, out overlapped, out ctx);

        public bool IsPositionValid(ItemData item, Vector2Int position, out InventoryResult result) => thisInventory.IsPositionValid(item, position, out result);
        public bool CanPlaceItem(ItemID id, Vector2Int position, bool isRotated, out InventoryResult result) => thisInventory.CanPlaceItem(id, position, isRotated, out result);

        public bool TrySortItems(IInventorySorter sorter, out InventoryResult result) => thisInventory.TrySortItems(sorter, out result);
        public bool TrySortItemsByArea(bool descending, out InventoryResult result) => thisInventory.TrySortItems(descending ? InventorySorter.SortByAreaDescending : InventorySorter.SortByArea, out result);
        public bool TrySortItemsByTag(out InventoryResult result) => thisInventory.TrySortItems(InventorySorter.SortByTag, out result);
        public bool TrySortItemsByTag(IReadOnlyList<ItemTag> tags, out InventoryResult result) => thisInventory.TrySortItems(new InventorySortByTag(tags), out result);

        public bool TryMergeItems(out InventoryResult result) => TryMergeItems(null, out result);
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

            foreach (Guid id in items)
            {
                if (TryGetItemByInstanceID(id, out ItemData registered))
                {
                    if (!groups.TryGetValue(registered.BaseID, out List<ItemData> list))
                    {
                        list = new();
                        groups[registered.BaseID] = list;
                    }

                    list.Add(registered);
                }
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

            if (totalMoved <= 0)
            {
                result = InventoryResult.NO_VALID_SPACE;
                return false;
            }

            result = InventoryResult.SUCCESS;
            return true;
        }
        public bool TryMergeItem(Guid targetInstanceID, Guid sourceInstanceID, out InventoryResult result) => TryMergeItem(targetInstanceID, this, sourceInstanceID, null, out result);
        public bool TryMergeItem(Guid targetInstanceID, Guid sourceInstanceID, Func<ItemData, ItemData, bool> canStackPredicate, out InventoryResult result) => TryMergeItem(targetInstanceID, this, sourceInstanceID, canStackPredicate, out result);
        public bool TryMergeItem(Guid targetInstanceID, InventoryEntity sourceInventory, Guid sourceInstanceID, out InventoryResult result) => TryMergeItem(targetInstanceID, sourceInventory, sourceInstanceID, null, out result);
        public bool TryMergeItem(Guid targetInstanceID, InventoryEntity sourceInventory, Guid sourceInstanceID, Func<ItemData, ItemData, bool> canStackPredicate, out InventoryResult result)
        {
            if (sourceInventory == null)
            {
                throw new ArgumentNullException(nameof(sourceInventory), "Try merge teim failed! source inventory is missing!?");
            }

            return thisInventory.TryMergeItem(targetInstanceID, sourceInventory.thisInventory, sourceInstanceID, canStackPredicate, out result);
        }
        public bool TryGetItemStack(Guid instanceID, out int stack, out InventoryResult result) => thisInventory.TryGetItemStack(instanceID, out stack, out result);
        public bool TrySetItemStack(Guid instanceID, int stack, out InventoryResult result) => thisInventory.TrySetItemStack(instanceID, stack, out result);
        public bool TrySwapItems(Guid instanceID, InventoryEntity targetInventory, Guid otherInstanceID, out InventoryResult result)
        {
            if (targetInventory == null)
            {
                throw new ArgumentNullException(nameof(targetInventory), "Try swap items failed! inventory is missing!?");
            }

            return thisInventory.TrySwapItems(instanceID, targetInventory.thisInventory, otherInstanceID, out result);
        }
        public bool TryTransferItem(Guid instanceID, Vector2Int? position, InventoryEntity inventory, out ItemData transfered, out InventoryResult result)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory), "Try transfer items failed! inventory is missing!?");
            }

            return thisInventory.TryTransferItem(instanceID, position, inventory.thisInventory, out transfered, out result);
        }
        public bool TryTransferItems(InventoryEntity inventory, out InventoryResult result)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory), "Try transfer items failed! inventory is missing!?");
            }

            return thisInventory.TryTransferItems(inventory.thisInventory, out result);
        }
        public bool TryAddItem(ItemData item, Vector2Int? position, out ItemData registered, out InventoryResult result) => thisInventory.TryAddItem(item, position, out registered, out result);
        public bool TryDropItem(Guid instanceID, out ItemData registered, out InventoryResult result) => thisInventory.TryDropItem(instanceID, dropOrigin.position, dropForce * dropOrigin.forward, out registered, out result);
        public bool TryRemoveItem(Guid instanceID, out ItemData registered, out InventoryResult result) => thisInventory.TryRemoveItem(instanceID, out registered, out result);
        public bool TryClearItem(Guid instanceID, out ItemData registered, out InventoryResult result) => thisInventory.TryClearItem(instanceID, out registered, out result);
        public bool TryPlaceItem(Guid instanceID, Vector2Int position, bool rotate, out ItemData registered, out InventoryResult ctx) => thisInventory.TryPlaceItem(instanceID, position, rotate, out registered, out ctx);
    }
}
