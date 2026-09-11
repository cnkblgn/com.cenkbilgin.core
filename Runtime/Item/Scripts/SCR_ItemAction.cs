using System;

namespace Core.Item
{
    [Serializable]
    public abstract class ItemAction
    {
        public abstract string GetName();
        public abstract bool IsValid(in ItemActionContext context);
        public abstract ItemActionResult Apply(in ItemActionContext context);
    }
}