using System;

namespace Core.Item
{
    public readonly struct ItemActionContext
    {
        public readonly InventoryController Inventory;
        public readonly ItemID ItemBaseID;
        public readonly Guid ItemInstanceID;

        public ItemActionContext(InventoryController inventory, ItemID itemBaseID, Guid itemInstanceID)
        {
            Inventory = inventory != null ? inventory : throw new ArgumentNullException(nameof(inventory), "Item action ctor failed! target inventory missing!?");
            ItemBaseID = !itemBaseID.IsValid ? throw new ArgumentNullException(nameof(inventory), "Item action ctor failed! target item base id is not valid!?") : itemBaseID;
            ItemInstanceID = itemInstanceID == Guid.Empty ? throw new ArgumentNullException(nameof(inventory), "Item action ctor failed! target item instance id is not valid!?") : itemInstanceID;
        }
    }
}