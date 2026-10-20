using UnityEngine;

namespace Core.Item
{
    public abstract class ItemAction : ScriptableObject
    {
        internal static readonly ItemAction[] NONE = new ItemAction[0];

        public abstract string Name { get; }

        public abstract bool IsValid(in ItemActionContext context);
        public abstract ItemActionResult Apply(in ItemActionContext context);
    }
}