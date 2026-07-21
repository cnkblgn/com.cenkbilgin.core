using System;
using UnityEngine;

namespace Core.UI
{
    using static CoreUtility;

    [Serializable]
    internal sealed class UICursorData
    {
        [Required] public Sprite Icon = null;
        public string ID = "default";
    }
}
