using System;
using System.Collections.Generic;

namespace Core.Race
{
    public static class RaceUtility
    {
        public static RaceMask CreateMask(this RaceTag[] tags) => RaceTag.CreateMask(tags);
        public static bool HasAll(this RaceTag[] @base, RaceTag[] target) => CreateMask(@base).HasAll(target);
        public static bool HasAny(this RaceTag[] @base, RaceTag[] target) => CreateMask(@base).HasAny(target);


        private const string RACE_ID = "race_id";

        public static void ExportTo(this RaceContainer obj, Dictionary<string, DataNode> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            RaceID id = obj.GetRace();
            data.SetString(id.Key, id.Key);
        }
        public static void ImportFrom<TContext>(this RaceContainer obj, Dictionary<string, DataNode> data, TContext context)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            string key = data.GetString(RACE_ID);
            int index = RaceDatabase.GetIDIndex(key);
            obj.TrySetRace(new(key, index), context);
        }
    }
}
