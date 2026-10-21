namespace Core.Event
{
    public readonly struct EventContext
    {
        public readonly EventID ID;
        public readonly int Source;
        public readonly ulong Tags;
        public readonly int Amount;

        public EventContext(EventID id, int source, ulong tags, int amount = 1)
        {
            ID = id;
            Source = source;
            Tags = tags;
            Amount = amount;
        }
        public EventContext(EventID id, int source, int amount = 1) : this(id, source, 0, amount) { }
        public EventContext(EventID id, int amount = 1) : this(id, 0, 0, amount) { }
    }
}