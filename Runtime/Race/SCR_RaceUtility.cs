namespace Core.Race
{
    public static class RaceUtility
    {
        public static ulong CreateMask(this RaceTag[] tags) => RaceTag.CreateMask(tags);

        public static bool HasAll(this ulong @base, RaceTag target) => @base.HasAll(target.Mask);
        public static bool HasAny(this ulong @base, RaceTag target) => @base.HasAny(target.Mask);

        public static bool HasAll(this RaceTag[] @base, RaceTag[] target) => CreateMask(@base).HasAll(CreateMask(target));
        public static bool HasAny(this RaceTag[] @base, RaceTag[] target) => CreateMask(@base).HasAny(CreateMask(target));
    }
}
