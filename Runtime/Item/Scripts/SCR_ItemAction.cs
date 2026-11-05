using UnityEngine;

namespace Core.Item
{
    public abstract class ItemAction : ScriptableObject
    {
        public static readonly ItemAction[] NONE = new ItemAction[0];

        public abstract string Name { get; }

        public abstract bool IsValid(GameObject target, in ItemActionContext ctx);
        public abstract ItemActionResult Apply(GameObject target, in ItemActionContext ctx);
    }
}