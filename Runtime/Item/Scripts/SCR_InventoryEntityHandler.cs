namespace Core.Item
{
    public interface IInventoryEntityHandler
    {
        public void HandleStateChanged(in InventoryContext ctx);
    }
}