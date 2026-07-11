namespace Core
{
    public static class ActorUtility
    {
        public static ulong CreateMask(this ActorTag[] tags) => ActorTag.CreateMask(tags);

        public static bool HasAll(this ulong @base, ulong target) => (@base & target) == target;
        public static bool HasAny(this ulong @base, ulong target) => (@base & target) != 0;

        public static bool HasAll(this ActorTag[] @base, ActorTag[] target) => HasAll(CreateMask(@base), CreateMask(target));
        public static bool HasAny(this ActorTag[] @base, ActorTag[] target) => HasAny(CreateMask(@base), CreateMask(target));

        public static bool HasAll(this Actor @base, ActorTag target) => HasAll(@base.Tags, target.Mask);
        public static bool HasAny(this Actor @base, ActorTag target) => HasAny(@base.Tags, target.Mask);
    }
}
