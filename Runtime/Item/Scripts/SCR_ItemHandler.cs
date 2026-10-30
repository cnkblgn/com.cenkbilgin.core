using System.Collections.Generic;

namespace Core.Item
{
    public interface IItemHandler
    {
        public void HandleExport(ItemID id, Dictionary<string, DataNode> data);
        public void HandleImport(ItemID id, Dictionary<string, DataNode> data);
    }
}