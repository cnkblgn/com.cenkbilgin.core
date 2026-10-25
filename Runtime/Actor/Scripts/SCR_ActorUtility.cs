namespace Core.Actors
{
    public static class ActorUtility
    {
        public static ActorMask CreateMask(this ActorTag[] tags) => ActorTag.CreateMask(tags);
        public static bool HasAll(this ActorTag[] @base, ActorTag[] target) => CreateMask(@base).HasAll(target);
        public static bool HasAny(this ActorTag[] @base, ActorTag[] target) => CreateMask(@base).HasAny(target);

    }
}
