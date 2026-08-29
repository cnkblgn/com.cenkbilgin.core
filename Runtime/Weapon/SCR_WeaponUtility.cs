namespace Core.Weapon
{
    public static class WeaponUtility
    {
        public static ulong CreateMask(this WeaponTag[] tags) => WeaponTag.CreateMask(tags);

        public static bool HasAll(this ulong @base, WeaponTag target) => @base.HasAll(target.Mask);
        public static bool HasAny(this ulong @base, WeaponTag target) => @base.HasAny(target.Mask);

        public static bool HasAll(this WeaponTag[] @base, WeaponTag[] target) => CreateMask(@base).HasAll(CreateMask(target));
        public static bool HasAny(this WeaponTag[] @base, WeaponTag[] target) => CreateMask(@base).HasAny(CreateMask(target));
    }
}