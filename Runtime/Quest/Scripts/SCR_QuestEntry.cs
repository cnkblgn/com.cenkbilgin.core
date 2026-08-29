using System;
using UnityEngine;
using Core.Localization;

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

#if UNITY_EDITOR
            Name = ID.Key;
#endif
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            Name = ID.Key;

            for (int i = 0; i < Conditions.Length; i++)
            {
                Conditions[i].OnValidate();
            }

            for (int i = 0; i < Actions.Length; i++)
            {
                Actions[i].OnValidate();
            }
        }
#endif
    }
}