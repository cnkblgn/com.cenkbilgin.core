using UnityEngine;

namespace Core.Quest
{
    public abstract class QuestAction : ScriptableObject
    {
        public static readonly QuestAction[] NONE = new QuestAction[0];

        public abstract void Started();
        public abstract void Completed();
    }
}