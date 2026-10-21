using System;
using UnityEngine;

namespace Core.Quest
{
    [Serializable]
    public struct QuestEntry
    {
#if UNITY_EDITOR
        [HideInInspector] public string Name;
#endif

        public QuestID ID;
        public QuestID NextID;
        public QuestRequirement[] Requirements;
        [Required, Expandable] public QuestCondition[] Conditions;
        [Required, Expandable] public QuestAction[] Actions;

        public QuestEntry(QuestID id, QuestID nextID, QuestRequirement[] requirements, QuestCondition[] conditions, QuestAction[] actions)
        {
            ID = id;
            NextID = nextID;
            Requirements = requirements;
            Conditions = conditions;
            Actions = actions;

#if UNITY_EDITOR
            Name = ID.Key;
#endif
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            Name = ID.Key;
        }
#endif
    }
}