using System;
using UnityEngine;

namespace Core.UI
{
    public readonly struct UIContextMenuContext
    {
        internal readonly Vector2 Position;
        internal readonly UIContextItemContext[] Items;

        public UIContextMenuContext(Vector2 position, UIContextItemContext[] items)
        {
            Position = position;
            Items = items == null || items.Length <= 0 ? throw new ArgumentNullException(nameof(items), "UI context menu ctx failed! actions cannot be null or empty!") : items;
        }
    }
}
