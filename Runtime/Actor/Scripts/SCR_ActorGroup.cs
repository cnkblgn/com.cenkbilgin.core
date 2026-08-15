using System.Collections.Generic;

namespace Core.Actors
{
    public sealed class ActorGroup
    {
        public readonly ActorID ID;
        internal readonly List<ActorEntry> Group;

        internal ActorGroup(ActorID id)
        {
            ID = id;
            Group = new();
        }
    }
}
