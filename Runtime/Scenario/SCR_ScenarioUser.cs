namespace Core.Scenario
{
    public interface IScenarioUser
    {
        public void HandleScenarioChanged(in ScenarioContext ctx);
    }
}
