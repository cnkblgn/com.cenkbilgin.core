using System;

namespace Core.Item
{
    [Serializable]
    public abstract class ItemAction
    {
        public abstract string GetName();
        public abstract ItemActionResult Apply(in ItemActionContext context);
    }
}