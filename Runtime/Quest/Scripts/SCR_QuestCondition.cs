using System;

namespace Core.Quest
{
    [Serializable]
    public abstract class QuestCondition
    {
        public abstract bool IsSatisfied();
    }
}
