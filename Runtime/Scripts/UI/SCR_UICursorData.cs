using System;
using UnityEngine;

namespace Core.UI
{
    using static CoreUtility;

    [Serializable]
    public class UICursorData
    {
        [Required] public Sprite Icon = null;
        public string ID = "default";
    }
}
