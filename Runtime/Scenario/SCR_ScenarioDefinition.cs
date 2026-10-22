namespace Core.Scenario
{
    public sealed class ScenarioDefinition
    {
        public readonly ScenarioID ID;
        internal readonly ScenarioCompatibility[] Compatibilities;
        internal readonly ScenarioAction[] Actions;

        public readonly string IconID;
        public readonly string NameID;
        public readonly string DescID;

        internal ScenarioDefinition(ScenarioID id, ScenarioCompatibility[] compatibilities, ScenarioAction[] actions)
        {
            ID = id;
            Compatibilities = compatibilities ?? ScenarioCompatibility.NONE;
            Actions = actions ?? ScenarioAction.NONE;

            IconID = $"Scenario{ID.Key}";
            NameID = $"scenario.{ID.Key}.name";
            DescID = $"scenario.{ID.Key}.desc";
        }
        internal ScenarioDefinition(ScenarioEntry entry) : this(entry.ID, entry.Compatibilities, entry.Actions) { }
    }
}