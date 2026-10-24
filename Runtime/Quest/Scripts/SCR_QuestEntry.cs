using System;
using UnityEngine;

namespace Core.Quest
{
    [Serializable]
    public struct QuestEntry
    {
#if UNITY_EDITOR
        [HideInInspector] public string Name;
        [SerializeField, ReadOnly] private string IconID;
        [SerializeField, ReadOnly] private string NameID;
        [SerializeField, ReadOnly] private string DescID;
#endif

        public QuestID ID;
        public QuestID NextID;
        public QuestRequirement[] Requirements;
        [Expandable, Required] public QuestCondition[] Conditions;
        [Expandable, Required] public QuestAction[] Actions;

        public QuestEntry(QuestID id, QuestID nextID, QuestRequirement[] requirements, QuestCondition[] conditions, QuestAction[] actions)
        {
            ID = id;
            NextID = nextID;
            Requirements = requirements;
            Conditions = conditions;
            Actions = actions;

#if UNITY_EDITOR
            Name = ID.Key;
            IconID = $"Quest{ID.Key}";
            NameID = $"quest.{ID.Key}Name";
            DescID = $"quest.{ID.Key}Desc";
#endif
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            Name = ID.Key;
            IconID = $"Quest{ID.Key}";
            NameID = $"quest.{ID.Key}Name";
            DescID = $"quest.{ID.Key}Desc";
        }
#endif
    }
}