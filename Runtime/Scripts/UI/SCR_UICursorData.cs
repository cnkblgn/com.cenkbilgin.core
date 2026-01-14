using System;
using UnityEngine;

namespace Core.UI
{
    using static CoreUtility;

    [Serializable]
    public class UICursorData
    {
        [Required] public Sprite Icon = null;
        public Vector2 Hotspot = Vector2.zero;
        public UICursorType Type = UICursorType.DEFAULT;
    }
}
