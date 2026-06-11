using System;

namespace Core
{
    [Serializable]
    public struct DataNode
    {
        public DataType Type;
        public DataValue Value;
    }
}