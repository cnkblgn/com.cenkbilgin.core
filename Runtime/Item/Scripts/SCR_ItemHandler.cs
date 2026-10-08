using System.Collections.Generic;

namespace Core.Item
{
    public interface IItemHandler
    {
        public void HandleExport(Dictionary<string, DataNode> data);
        public void HandleImport(Dictionary<string, DataNode> data);
    }
}