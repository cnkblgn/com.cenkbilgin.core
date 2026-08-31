using System;

namespace Core.Weapon
{
    public interface IWeaponModule { }

    [Serializable]
    public abstract class WeaponModule : IWeaponModule
    {
        public abstract WeaponModule Clone();

        public abstract void Initialize(Weapon weapon);
        public abstract void Tick(float deltaTime);
    }
}