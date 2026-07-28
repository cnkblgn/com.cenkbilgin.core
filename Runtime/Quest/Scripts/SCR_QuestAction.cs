using System;

namespace Core.Quest
{
    [Serializable]
    public abstract class QuestAction
    {
        public abstract void Started();
        public abstract void Completed();
    }
}