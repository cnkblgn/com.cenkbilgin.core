using System;
using System.Collections.Generic;

namespace Core.Quest
{
    public static class QuestUtility
    {
        private const string ID = "q_id";
        private const string PROGRESS = "q_prog";
        private const string COMPLETED = "q_comp";
        private const string QUESTS = "q_quests";

        public static void ExportTo(this List<QuestInstance> obj, Dictionary<string, DataNode> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            Dictionary<string, DataNode> questTable = new();

            for (int i = 0; i < obj.Count; i++)
            {
                Dictionary<string, DataNode> questInstance = new();
                obj[i].ExportTo(questInstance);

                questTable.SetData(i.ToString(), questInstance);
            }

            data.SetData(QUESTS, questTable);
        }
        public static List<QuestInstance> CreateQuestListFrom(Dictionary<string, DataNode> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            List<QuestInstance> questList = new();
            Dictionary<string, DataNode> questTable = data.GetData(QUESTS);

            for (int i = 0; i < questTable.Count; i++)
            {
                Dictionary<string, DataNode> questInstance = questTable.GetData(i.ToString());
                questList.Add(CreateQuestInstaceFrom(questInstance));
            }

            return questList;
        }

        public static void ExportTo(this QuestInstance obj, Dictionary<string, DataNode> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            data.SetString(ID, obj.Definition.ID.Key);
            data.SetBool(COMPLETED, obj.IsCompleted);

            Dictionary<string, DataNode> progressTable = new();

            for (int i = 0; i < obj.Progress.Length; i++)
            {
                progressTable.SetInt(i.ToString(), obj.Progress[i]);
            }

            data.SetData(PROGRESS, progressTable);
        }
        public static QuestInstance CreateQuestInstaceFrom(Dictionary<string, DataNode> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            string key = data.GetString(ID);
            int index = QuestDatabase.GetIDIndex(key);
            bool completed = data.GetBool(COMPLETED);
            Dictionary<string, DataNode> progressTable = data.GetData(PROGRESS);

            int[] progress = new int[progressTable.Count];

            for (int i = 0; i < progress.Length; i++)
            {
                progress[i] = progressTable.GetInt(i.ToString());
            }

            return new(new(key, index), progress, completed);
        }
    }
}