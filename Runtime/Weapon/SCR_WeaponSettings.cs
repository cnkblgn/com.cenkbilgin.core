using System;
using System.Collections.Generic;

namespace Core.Weapon
{
    [Serializable]
    public abstract class WeaponSettings
    {
        public abstract WeaponSettings Clone();
        public abstract void ExportTo(Dictionary<string, DataNode> data);
        public abstract void ImportFrom(Dictionary<string, DataNode> data);

#if UNITY_EDITOR
        public virtual void OnValidate() { }
#endif
    }
}
