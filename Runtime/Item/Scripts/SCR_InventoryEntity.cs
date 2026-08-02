using System;
using System.Collections.Generic;
using System.Linq;
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
        private InventoryEntity targetInventory = null;
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

        public bool TryGetValidPositionForItem(ItemData item, out Vector2Int position, out InventoryResult result) => this.thisInventory.TryGetValidPositionForItem(item, out position, out result);
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

        /// <summary> Tries to add item. Set position null if you want automatic positioning. </summary>
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
        /// <summary> Tries to drop item. </summary>
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
                body.AddForce((dropForce * dropOrigin.forward) + (UnityEngine.Random.onUnitSphere * 0.25f), ForceMode.Impulse);
                body.AddTorque((dropForce * dropOrigin.forward) + (UnityEngine.Random.onUnitSphere * 0.25f), ForceMode.Impulse);
            }

            return true;
        }

        /// <summary> Tries to completely remove existing item from inventory. </summary>
        public bool TryRemoveItem(Guid instanceID, out ItemData registered, out InventoryResult result)
        {
            if (!thisInventory.TryRemoveItem(instanceID, out registered, out result))
            {
                return false;
            }

            SetState(InventoryState.ITEM_REMOVED, result, registered);

            return true;
        }
        /// <summary> Tries to clear existing item from tile. </summary>
        public bool TryClearItem(Guid instanceID, out ItemData registered, out InventoryResult result) => thisInventory.TryClearItem(instanceID, out registered, out result);
        /// <summary> Tries to assign existing item to tile. </summary>
        public bool TryPlaceItem(Guid instanceID, Vector2Int position, bool rotate, out ItemData registered, out InventoryResult ctx) => thisInventory.TryPlaceItem(instanceID, position, rotate, out registered, out ctx);
        /// <summary> Tries to transfer item. Set position null if you want automatic positioning. </summary>
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
        /// <summary> Tries to transfer all items with automatic positioning. </summary>
        public bool TryTransferItems(InventoryEntity inventory, out InventoryResult result)
        {
            if (inventory == null)
            {
                result = InventoryResult.NULL;
                throw new ArgumentNullException($"Inventory transfer items failed target inventory is null! {nameof(inventory)}");
            }

            bool addedAny = false;

            foreach (Guid id in GetItems().ToList())
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

        public bool IsPositionValidForItem(ItemData item, Vector2Int position, out InventoryResult result) => this.thisInventory.IsPositionValidForItem(item, position, out result);
    }
}
