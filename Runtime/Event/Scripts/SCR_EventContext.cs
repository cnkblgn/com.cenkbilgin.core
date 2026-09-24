namespace Core.Event
{
    public readonly struct EventContext
    {
        public readonly EventID ID;
        public readonly int Actor;
        public readonly ulong Tags;
        public readonly int Amount;

        public EventContext(EventID id, int actor, ulong tags, int amount = 1)
        {
            ID = id;
            Actor = actor;
            Tags = tags;
            Amount = amount;
        }
        public EventContext(EventID id, int actor, int amount = 1) : this(id, actor, 0, amount) { }
        public EventContext(EventID id, int amount = 1) : this(id, 0, 0, amount) { }
    }
}