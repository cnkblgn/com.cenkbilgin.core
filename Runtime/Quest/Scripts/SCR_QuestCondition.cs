using System;

namespace Core.Quest
{
    [Serializable]
    public abstract class QuestCondition
    {
        public abstract bool IsSatisfied();

#if UNITY_EDITOR
        public virtual void OnValidate() { }
#endif
    }
}
