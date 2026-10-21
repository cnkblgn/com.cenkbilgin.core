using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Core.Item
{
    public abstract class ItemComponent : ScriptableObject
    {
        public static ItemComponent NONE
        {
            get
            {
                if (none == null)
                {
                    none = CreateInstance<ItemComponentNone>();
                }

                return none;
            }
        } private static ItemComponentNone none;

        /// <summary> Called when item created with only base InstanceID. This creates default thisData for item </summary>
        public abstract void GetDefaults(Dictionary<string, DataNode> data);

        /// <summary> Called when item thisData description requested. Builds description for item </summary>
        public abstract void GetDescription(Dictionary<string, DataNode> data, in StringBuilder sb);

        /// <summary> Called when editor validates component. </summary>
        public abstract void Validate(ItemID id);
    }

    internal sealed class ItemComponentNone : ItemComponent
    {
        public override void GetDefaults(Dictionary<string, DataNode> data) { }
        public override void GetDescription(Dictionary<string, DataNode> data, in StringBuilder sb) { }
        public override void Validate(ItemID id) { }
    }
}