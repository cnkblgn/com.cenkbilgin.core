using System.Collections.Generic;

namespace Core.Item
{
    public interface IItemHandler
    {
        public void HandleExport(Dictionary<string, DataNode> thisData);
        public void HandleImport(Dictionary<string, DataNode> thisData);
    }
}