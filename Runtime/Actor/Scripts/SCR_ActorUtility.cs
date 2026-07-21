namespace Core.Actors
{
    public static class ActorUtility
    {
        public static ulong CreateMask(this ActorTag[] tags) => ActorTag.CreateMask(tags);

        public static bool HasAll(this ActorTag[] @base, ActorTag[] target) => CreateMask(@base).HasAll(CreateMask(target));                                                           
        public static bool HasAny(this ActorTag[] @base, ActorTag[] target) => CreateMask(@base).HasAny(CreateMask(target));

        public static bool HasAll(this ulong @base, ActorTag target) => @base.HasAll(target.Mask);
        public static bool HasAny(this ulong @base, ActorTag target) => @base.HasAny(target.Mask);

        public static bool HasAll(this Actor @base, ActorTag target) => @base.Tags.HasAll(target.Mask);
        public static bool HasAny(this Actor @base, ActorTag target) => @base.Tags.HasAny(target.Mask);

        public static bool HasAll(this Actor @base, ulong target) => @base.Tags.HasAll(target);
        public static bool HasAny(this Actor @base, ulong target) => @base.Tags.HasAny(target);
    }
}
