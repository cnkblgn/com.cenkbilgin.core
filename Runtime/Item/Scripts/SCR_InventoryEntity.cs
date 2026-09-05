using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Localization;

namespace Core.Item
{
    using static CoreUtility;
    using static InventoryData;

    [DisallowMultipleComponent]
    public sealed class InventoryEntity : MonoBehaviour
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

        private InventoryData thisInventory = new(MIN_WIDTH, MIN_HEIGHT, MIN_WEIGHT);
        private IInventoryHandler thisHandler = null;
        private ulong whitelistTag;

        private void Awake()
        {
            thisHandler = GetComponent<IInventoryHandler>();

            thisInventory = new(width, height, weight);

            foreach (ItemID item in startingItems)
            {
                thisInventory.TryAddItem(item.CreateData(), null, out ItemData _, out InventoryResult _);
            }

            whitelistTag = whitelistedItems.CreateMask();
        }
        private void Start() => Initialize();        

        private void Initialize() => SetState(InventoryState.INITIALIZED);

        private void SetState(InventoryState state, InventoryResult result = InventoryResult.SUCCESS, ItemData item = null)
        {
            InventoryContext ctx = new(state, result, this, item);

            OnStateChanged?.Invoke(ctx);
            thisHandler?.HandleStateChanged(in ctx);
        }

        public void ImportFrom(InventoryData inventory)
        {
            thisInventory = new(inventory);

            Initialize();
        }
        public void ExportTo(out InventoryData inventory) => inventory = new(thisInventory);

        /// <summary> Clears inventory. Removes all items and calls Initialize. </summary>
        public void Clear()
        {
            thisInventory.Clear();

            Initialize();
        } 

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
                throw new ArgumentNullException(nameof(sourceInventory), "Merge failed source inventory missing!?");
            }

            if (targetInstanceID == sourceInstanceID)
            {
                Debug.LogError("Trying to merge with duplicate item");
                result = InventoryResult.DUPLICATE;
                return false;
            }

            if (!TryGetItemByInstanceID(targetInstanceID, out ItemData target))
            {
                result = InventoryResult.NOT_REGISTERED;
                return false;
            }

            if (!sourceInventory.TryGetItemByInstanceID(sourceInstanceID, out ItemData source))
            {
                result = InventoryResult.NOT_REGISTERED;
                return false;
            }

            if (canStackPredicate != null && !canStackPredicate(target, source))
            {
                result = InventoryResult.NOT_SUPPORTED;
                return false;
            }
            else if (target.BaseID != source.BaseID)
            {
                result = InventoryResult.NOT_SUPPORTED;
                return false;
            }

            int maxStack = target.BaseID.GetDefinition().Stack;
            int space = maxStack - target.GetStack();

            if (space <= 0)
            {
                result = InventoryResult.STACK_FULL;
                return false;
            }

            int amount = Mathf.Min(space, source.GetStack());

            if (amount <= 0)
            {
                result = InventoryResult.NO_VALID_SPACE;
                return false;
            }

            TrySetItemStack(targetInstanceID, target.GetStack() + amount, out result);
            sourceInventory.TrySetItemStack(sourceInstanceID, source.GetStack() - amount, out _); 

            result = InventoryResult.SUCCESS;
            return true;
        }
        public bool TryGetItemStack(Guid instanceID, out int stack, out InventoryResult result) => thisInventory.TryGetItemStack(instanceID, out stack, out result);
        public bool TrySetItemStack(Guid instanceID, int stack, out InventoryResult result)
        {
            if (!TryGetItemByInstanceID(instanceID, out ItemData registered))
            {
                result = InventoryResult.NOT_REGISTERED;
                return false;
            }

            thisInventory.TrySetItemStack(registered, stack, out _);

            SetState(InventoryState.ITEM_CHANGED, result = InventoryResult.SUCCESS, registered);

            return registered.GetStack() > 0 || TryRemoveItem(instanceID, out _, out result);
        }
        public bool TrySwapItems(Guid instanceID, InventoryEntity otherInventory, Guid otherInstanceID, out InventoryResult result)
        {
            if (otherInventory == null)
            {
                throw new ArgumentNullException(nameof(otherInventory));
            }

            if (!TryGetItemByInstanceID(instanceID, out ItemData itemA))
            {
                result = InventoryResult.NOT_REGISTERED;
                return false;
            }

            if (!otherInventory.TryGetItemByInstanceID(otherInstanceID, out ItemData itemB))
            {
                result = InventoryResult.NOT_REGISTERED;
                return false;
            }

            Vector2Int positionA = itemA.Position;
            Vector2Int positionB = itemB.Position;

            if (!itemB.Tags.HasAny(whitelistTag) || !itemA.Tags.HasAny(otherInventory.whitelistTag))
            {
                result = InventoryResult.NOT_SUPPORTED;
                return false;
            }

            if (!TryRemoveItem(instanceID, out ItemData removedA, out result))
            {
                return false;
            }

            if (!otherInventory.TryRemoveItem(otherInstanceID, out ItemData removedB, out result))
            {
                TryAddItem(removedA, positionA, out _, out _);
                return false;
            }

            if (!TryAddItem(removedB, positionA, out ItemData placedB, out result))
            {
                otherInventory.TryAddItem(removedB, positionB, out _, out _); 
                TryAddItem(removedA, positionA, out _, out _);                
                return false;
            }

            if (!otherInventory.TryAddItem(removedA, positionB, out ItemData placedA, out result))
            {
                TryRemoveItem(placedB.InstanceID, out _, out _);
                TryAddItem(removedA, positionA, out _, out _);
                otherInventory.TryAddItem(removedB, positionB, out _, out _);
                return false;
            }

            result = InventoryResult.SUCCESS;
            return true;
        }
        public bool TryTransferItem(Guid instanceID, Vector2Int? position, InventoryEntity inventory, out ItemData transfered, out InventoryResult result)
        {
            transfered = null;

            if (inventory == null)
            {
                result = InventoryResult.NULL;
                throw new ArgumentNullException($"Inventory transfer item failed target inventory is null! {nameof(inventory)}");
            }

            if (!TryGetItemByInstanceID(instanceID, out ItemData registered))
            {
                Debug.LogError($"Inventory transfer item failed! [{instanceID}] not found!");
                result = InventoryResult.NOT_REGISTERED;
                return false;
            }

            if (inventory.TryAddItem(registered, position, out transfered, out result))
            {
                if (TryRemoveItem(instanceID, out _, out result))
                {
                    SetState(InventoryState.ITEM_TRANSFERED, result, transfered);
                    return true;
                }
            }

            return false;
        }
        public bool TryTransferItems(InventoryEntity inventory, out InventoryResult result)
        {
            if (inventory == null)
            {
                result = InventoryResult.NULL;
                throw new ArgumentNullException($"Inventory transfer items failed target inventory is null! {nameof(inventory)}");
            }

            bool addedAny = false;

            foreach (Guid id in GetItems())
            {
                if (TryGetItemByInstanceID(id, out ItemData registered) && inventory.TryAddItem(registered, null, out _, out _))
                {
                    if (TryRemoveItem(id, out _, out _))
                    {
                        addedAny = true;
                    }
                }
            }

            result = InventoryResult.SUCCESS;
            return addedAny;
        }
        public bool TryAddItem(ItemData item, Vector2Int? position, out ItemData registered, out InventoryResult result)
        {
            registered = null;

            if (item == null)
            {
                throw new ArgumentNullException($"item adding failed incoming item is null! {nameof(item)}");
            }

            if (!item.Tags.HasAny(whitelistTag))
            {
                result = InventoryResult.NOT_SUPPORTED;
                return false;
            }

            if (!thisInventory.TryAddItem(item, position, out registered, out result))
            {
                return false;
            }

            SetState(InventoryState.ITEM_ADDED, result, registered);
            return true;
        }
        public bool TryDropItem(Guid instanceID, out ItemData registered, out InventoryResult result)
        {
            if (!TryRemoveItem(instanceID, out registered, out result))
            {
                return false;
            }

            ItemEntity entity = ItemDatabase.CreateEntity(registered, dropOrigin.position, Quaternion.identity);

            if (entity == null)
            {
                return false;
            }

            SetState(InventoryState.ITEM_DROPPED, result, registered);

            if (entity.TryGetComponent(out Rigidbody body))
            {
                if (body.isKinematic)
                {
                    body.isKinematic = false;
                }

                body.useGravity = true;

                body.AddForce((dropForce * dropOrigin.forward) + (UnityEngine.Random.onUnitSphere * 0.25f), ForceMode.Impulse);
                body.AddTorque((dropForce * dropOrigin.forward) + (UnityEngine.Random.onUnitSphere * 0.25f), ForceMode.Impulse);
            }

            return true;
        }
        public bool TryRemoveItem(Guid instanceID, out ItemData registered, out InventoryResult result)
        {
            if (!thisInventory.TryRemoveItem(instanceID, out registered, out result))
            {
                return false;
            }

            SetState(InventoryState.ITEM_REMOVED, result, registered);

            return true;
        }
        public bool TryClearItem(Guid instanceID, out ItemData registered, out InventoryResult result) => thisInventory.TryClearItem(instanceID, out registered, out result);
        public bool TryPlaceItem(Guid instanceID, Vector2Int position, bool rotate, out ItemData registered, out InventoryResult ctx) => thisInventory.TryPlaceItem(instanceID, position, rotate, out registered, out ctx);       
    }
}
