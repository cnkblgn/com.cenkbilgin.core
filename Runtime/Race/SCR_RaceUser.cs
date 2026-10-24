namespace Core.Race
{
    public interface IRaceUser
    {
        public void HandleRaceChanged(in RaceContext ctx);
    }
}
