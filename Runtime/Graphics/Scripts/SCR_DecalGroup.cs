using System;
using UnityEngine;

namespace Core.Graphics
{
    using static CoreUtility;

    [Serializable]
    internal sealed class DecalGroup
    {
#if UNITY_EDITOR
        [field: SerializeField, HideInInspector] private string Name { get; set; } = "Object";
        public void Validate() => Name = Prefab != null ? Prefab.name : STRING_NULL;
#endif

        [Required] public DecalEmitter Prefab = null;
        [Min(0)] public int Count = 1;
    }
}
