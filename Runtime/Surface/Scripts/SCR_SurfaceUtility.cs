namespace Core.Surface
{
    public static class SurfaceUtility
    {
        public static ulong CreateMask(this SurfaceTag[] tags) => SurfaceTag.CreateMask(tags);

        public static bool HasAll(this ulong @base, SurfaceTag target) => @base.HasAll(target.Mask);
        public static bool HasAny(this ulong @base, SurfaceTag target) => @base.HasAny(target.Mask);

        public static bool HasAll(this SurfaceTag[] @base, SurfaceTag[] target) => CreateMask(@base).HasAll(CreateMask(target));
        public static bool HasAny(this SurfaceTag[] @base, SurfaceTag[] target) => CreateMask(@base).HasAny(CreateMask(target));
    }
}
