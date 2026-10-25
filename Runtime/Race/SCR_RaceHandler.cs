namespace Core.Race
{
    public interface IRaceHandler
    {
        public void HandleRaceChanged(in RaceContext ctx);
    }
}
