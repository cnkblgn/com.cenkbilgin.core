namespace Core
{
    public static class ActorUtility
    {
        public static ulong CreateMask(this ActorTag[] tags)
        {
            ulong mask = 0;

            for (int i = 0; i < tags.Length; i++)
            {
                mask |= tags[i].Mask;
            }

            return mask;
        }

        public static bool HasAll(ulong @base, ulong target) => (@base & target) == target;
        public static bool HasAny(ulong @base, ulong target) => (@base & target) != 0;

        public static bool HasAll(this ActorTag[] @base, ActorTag[] target) => HasAll(CreateMask(@base), CreateMask(target));
        public static bool HasAny(this ActorTag[] @base, ActorTag[] target) => HasAny(CreateMask(@base), CreateMask(target));

        public static bool HasAll(this Actor @base, ActorTag target) => HasAll(@base.Mask, target.Mask);
        public static bool HasAny(this Actor @base, ActorTag target) => HasAny(@base.Mask, target.Mask);
    }
}
