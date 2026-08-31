using System;

namespace Core.Weapon
{
    [Serializable]
    public abstract class WeaponModule
    {
        public abstract WeaponModule Clone();

        public abstract void Initialize(Weapon weapon);
        public abstract void Tick(float deltaTime);
    }
}