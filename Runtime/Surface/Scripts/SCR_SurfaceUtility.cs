namespace Core.Surface
{
    public static class SurfaceUtility
    {
        public static SurfaceMask CreateMask(this SurfaceTag[] tags) => SurfaceTag.CreateMask(tags);
        public static bool HasAll(this SurfaceTag[] @base, SurfaceTag[] target) => CreateMask(@base).HasAll(target);
        public static bool HasAny(this SurfaceTag[] @base, SurfaceTag[] target) => CreateMask(@base).HasAny(target);
    }
}
