using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Item
{
    [Serializable]
    public abstract class ItemComponent
    {
        public static readonly ItemComponentGeneric DEFAULT = new();

        /// <summary> Called when item created with only base InstanceID. This creates default thisData for item </summary>
        public abstract void GetDefaults(Dictionary<string, DataNode> data);

        /// <summary> Called when item thisData description requested. Builds description for item </summary>
        public abstract void GetDescription(Dictionary<string, DataNode> data, in StringBuilder sb);

#if UNITY_EDITOR
        public virtual void OnValidate() { }
#endif
    }

    [Serializable]
    public sealed class ItemComponentGeneric : ItemComponent
    {
        public override void GetDefaults(Dictionary<string, DataNode> data) { }
        public override void GetDescription(Dictionary<string, DataNode> data, in StringBuilder sb) { }
    }
}