using UnityEngine;

namespace Core.Quest
{
    public abstract class QuestCondition : ScriptableObject
    {
        public static readonly QuestCondition[] NONE = new QuestCondition[0];

        public abstract bool IsSatisfied();
    }
}
