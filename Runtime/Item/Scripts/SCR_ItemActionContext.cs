using System;

namespace Core.Item
{
    public readonly struct ItemActionContext<TContext>
    {
        public readonly TContext Context;
        public readonly InventoryEntity Inventory;
        public readonly ItemID ItemBaseID;
        public readonly Guid ItemInstanceID;

        public ItemActionContext(TContext context, InventoryEntity inventory, ItemID itemBaseID, Guid itemInstanceID)
        {
            Context = context != null ? context : throw new ArgumentNullException(nameof(inventory), "Item action ctor failed! context missing!?");
            Inventory = inventory != null ? inventory : throw new ArgumentNullException(nameof(inventory), "Item action ctor failed! target inventory missing!?");
            ItemBaseID = !itemBaseID.IsValid ? throw new ArgumentNullException(nameof(inventory), "Item action ctor failed! target item base id is not valid!?") : itemBaseID;
            ItemInstanceID = itemInstanceID == Guid.Empty ? throw new ArgumentNullException(nameof(inventory), "Item action ctor failed! target item instance id is not valid!?") : itemInstanceID;
        }
    }
}