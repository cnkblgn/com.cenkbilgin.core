namespace Core.Race
{
    public readonly struct RaceContext
    {
        public readonly RaceID ID;

        public RaceContext(RaceID id)
        {
            ID = id;
        }
    }
}
