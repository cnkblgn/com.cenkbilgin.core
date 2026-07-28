using System;
using UnityEngine;
using Core.Localization;

namespace Core.Quest
{
    [Serializable]
    public struct QuestEntry
    {
        [HideInInspector] public string Name;

        public QuestID ID;
        public QuestID NextID;
        public LocalizedID NameID;
        public LocalizedID DescID;
        public QuestRequirement[] Requirements;
        [SerializeReference, Reference] public QuestCondition[] Conditions;
        [SerializeReference, Reference] public QuestAction[] Actions;

        public QuestEntry(QuestID id, QuestID nextID, LocalizedID nameID, LocalizedID descID, QuestRequirement[] requirements, QuestCondition[] conditions, QuestAction[] actions)
        {
            ID = id;
            NextID = nextID;
            NameID = nameID;
            DescID = descID;
            Requirements = requirements;
            Conditions = conditions;
            Actions = actions;

            Name = ID.Key;
        }
    }
}