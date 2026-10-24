namespace Core.Scenario
{
    public interface IScenarioUser
    {
        public void HandleScenarioStateChanged(in ScenarioContext ctx);
    }
}
