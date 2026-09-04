using System;

namespace Core.Item
{
    public readonly struct ItemActionContext
    {
        public readonly InventoryEntity Inventory;
        public readonly ItemID ItemBaseID;
        public readonly Guid ItemInstanceID;

        public ItemActionContext(InventoryEntity inventory, ItemID itemBaseID, Guid itemInstanceID)
        {
            Inventory = inventory != null ? inventory : throw new ArgumentNullException(nameof(inventory), "Item action ctx failed! target inventory missing!?");
            ItemBaseID = !itemBaseID.IsValid ? throw new ArgumentNullException(nameof(inventory), "Item action ctx failed! target item base id is not valid!?") : itemBaseID;
            ItemInstanceID = itemInstanceID == Guid.Empty ? throw new ArgumentNullException(nameof(inventory), "Item action ctx failed! target item instance id is not valid!?") : itemInstanceID;
        }
    }
}