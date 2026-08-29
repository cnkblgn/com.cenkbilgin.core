using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Stat
{
    public static class StatUtility
    {
        private const string STATS = "s_stats";
        private const string STAT_ID = "s_id";
        private const string STAT_VAL = "s_val";

        public static void ExportTo(this StatContainer obj, Dictionary<string, DataNode> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            Dictionary<string, DataNode> statTable = new();
            IReadOnlyCollection<StatID> statList = obj.GetStats();
            int count = 0;

            foreach (StatID stat in statList)
            {
                Dictionary<string, DataNode> statSlot = new();
                statSlot.SetString(STAT_ID, stat.Key);
                statSlot.SetFloat(STAT_VAL, obj.GetStat(stat));
                statTable.SetData(count.ToString(), statSlot);
                count++;
            }

            data.SetData(STATS, statTable);
        }
        public static void ImportFrom(this StatContainer obj, Dictionary<string, DataNode> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            Dictionary<string, DataNode> statTable = data.GetData(STATS);

            for (int i = 0; i < statTable.Count; i++)
            {
                Dictionary<string, DataNode> statSlot = statTable.GetData(i.ToString());
                string key = statSlot.GetString(STAT_ID);
                float value = statSlot.GetFloat(STAT_VAL);
                int index = StatDatabase.GetIDIndex(key);

                obj.SetStat(new(key, index), value);
            }
        }
    }
}