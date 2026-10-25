namespace Core.Scenario
{
    public interface IScenarioHandler
    {
        public void HandleScenarioChanged(in ScenarioContext ctx);
    }
}
