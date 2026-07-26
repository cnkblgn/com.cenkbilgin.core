namespace Core.Item
{
    public interface IInventoryHandler
    {
        public void HandleStateChanged(in InventoryContext ctx);
    }
}