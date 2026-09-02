using System.Collections.Generic;

namespace Core.Weapon
{
    public interface IWeaponModule 
    {
        public void Initialize(Weapon weapon);
        public void Tick(float deltaTime);

        public abstract void ExportTo(Dictionary<string, DataNode> data);
        public abstract void ImportFrom(Dictionary<string, DataNode> data);
    }
}