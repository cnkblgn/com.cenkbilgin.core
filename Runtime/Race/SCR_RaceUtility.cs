namespace Core.Race
{
    public static class RaceUtility
    {
        public static RaceMask CreateMask(this RaceTag[] tags) => RaceTag.CreateMask(tags);
        public static bool HasAll(this RaceTag[] @base, RaceTag[] target) => CreateMask(@base).HasAll(target);
        public static bool HasAny(this RaceTag[] @base, RaceTag[] target) => CreateMask(@base).HasAny(target);

    }
}
