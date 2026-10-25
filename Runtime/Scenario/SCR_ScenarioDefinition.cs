namespace Core.Scenario
{
    public sealed class ScenarioDefinition
    {
        public readonly ScenarioID ID;
        internal readonly Compatibility[] Compatibilities;
        internal readonly ScenarioAction[] Actions;

        public readonly string IconID;
        public readonly string NameID;
        public readonly string DescID;

        internal ScenarioDefinition(ScenarioID id, Compatibility[] compatibilities, ScenarioAction[] actions)
        {
            ID = id;
            Compatibilities = compatibilities ?? Compatibility.NONE;
            Actions = actions ?? ScenarioAction.NONE;

            IconID = $"Scenario{ID.Key}";
            NameID = $"scenario.{ID.Key}Name";
            DescID = $"scenario.{ID.Key}Desc";
        }
        internal ScenarioDefinition(ScenarioEntry entry) : this(entry.ID, entry.Compatibilities, entry.Actions) { }
    }
}