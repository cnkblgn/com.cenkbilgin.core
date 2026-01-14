using System;
using UnityEngine;

namespace Core.Graphics
{
    using static CoreUtility;

    [Serializable]
    public class PoolGroupDecal
    {
#if UNITY_EDITOR
        [field: SerializeField, HideInInspector] public string Name { get; set; } = "Object";
#endif

        [Required] public DecalEmitter Prefab = null;
        [Min(0)] public int Count = 1;
    }
}
