using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Item
{
    public interface IItemComponent
    {
        public static IItemComponent DEFAULT = new ItemComponentGeneric();

        /// <summary> Called when item created with only base InstanceID. This creates default thisData for item </summary>
        public void GetDefaults(Dictionary<string, DataNode> data);

        /// <summary> Called when item thisData description requested. Builds description for item </summary>
        public void GetDescription(Dictionary<string, DataNode> data, in StringBuilder sb);

        /// <summary> Called when editor validates component. </summary>
        public void OnValidate(ItemID id);
    }

    [Serializable]
    internal sealed class ItemComponentGeneric : IItemComponent
    {
        public void GetDefaults(Dictionary<string, DataNode> data) { }
        public void GetDescription(Dictionary<string, DataNode> data, in StringBuilder sb) { }
        public void OnValidate(ItemID id) { }
    }
}