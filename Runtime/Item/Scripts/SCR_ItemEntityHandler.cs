using System.Collections.Generic;

namespace Core.Item
{
    public interface IItemEntityHandler
    {
        public void HandleExport(ItemID id, Dictionary<string, DataNode> data);
        public void HandleImport(ItemID id, Dictionary<string, DataNode> data);
    }
}