namespace Core.Item
{
    internal interface IInventoryUser
    {
        public void HandleStateChanged(InventoryContext ctx);
    }
}
