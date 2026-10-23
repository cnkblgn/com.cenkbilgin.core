using UnityEngine;

namespace Core.Quest
{
    public abstract class QuestAction : ScriptableObject
    {
        public static readonly QuestAction[] NONE = new QuestAction[0];

        public abstract void OnStarted();
        public abstract void OnCompleted();
    }
}