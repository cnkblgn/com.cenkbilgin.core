namespace Core.Stat
{
    public readonly struct StatModifierSource
    {
        public readonly uint TypeID;
        public readonly uint ContextID;

        public StatModifierSource(uint typeID, uint contextID)
        {
            TypeID = typeID;
            ContextID = contextID;
        }
    }
}
