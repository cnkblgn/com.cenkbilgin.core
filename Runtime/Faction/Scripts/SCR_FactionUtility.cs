using System;
using System.Collections.Generic;

namespace Core.Faction
{
    public static class FactionUtility
    {
        private const string ID = "f_id";
        private const string RELATIONS = "f_rels";
        private const string RELATION = "f_relVal";
        private const string FACTIONS = "f_factions";

        public static void ExportTo(this FactionContainer obj, Dictionary<string, DataNode> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            data.SetString(ID, obj.GetFaction().Key);

            Dictionary<string, DataNode> relationTable = new();

            obj.GetRelations().ExportTo(relationTable);

            data.SetData(RELATIONS, relationTable);
        }
        public static void ImportFrom(this FactionContainer obj, Dictionary<string, DataNode> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            string key = data.GetString(ID);
            int index = FactionDatabase.GetIDIndex(key);
            FactionRelation[] relations = CreateFactionRelationsFrom(data.GetData(RELATIONS));

            obj.TrySetFaction(new(key, index));

            for (int i = 0; i < relations.Length; i++)
            {
                obj.SetRelation(relations[i].ID, relations[i].Relation);
            }
        }

        public static void ExportTo(this IReadOnlyList<FactionInstance> obj, Dictionary<string, DataNode> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            Dictionary<string, DataNode> factionTable = new();

            for (int i = 0; i < obj.Count; i++)
            {
                Dictionary<string, DataNode> factionInstance = new();

                obj[i].ExportTo(factionInstance);

                factionTable.SetData(i.ToString(), factionInstance);
            }

            data.SetData(FACTIONS, factionTable);
        }
        public static FactionInstance[] CreateFactionListFrom(Dictionary<string, DataNode> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            FactionInstance[] factionList = new FactionInstance[data.Count];

            for (int i = 0; i < factionList.Length; i++)
            {
                factionList[i] = CreateFactionInstaceFrom(data.GetData(i.ToString()));
            }

            return factionList;
        }

        public static void ExportTo(this FactionInstance obj, Dictionary<string, DataNode> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            data.SetString(ID, obj.ID.Key);

            Dictionary<string, DataNode> relationTable = new();
            obj.GetRelations().ExportTo(relationTable);
            data.SetData(RELATIONS, relationTable);
        }
        public static FactionInstance CreateFactionInstaceFrom(Dictionary<string, DataNode> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            string key = data.GetString(ID);
            int index = FactionDatabase.GetIDIndex(key);
            FactionRelation[] relations = CreateFactionRelationsFrom(data.GetData(RELATIONS));

            return new(new(key, index), relations, FactionDatabase.GetDefinitions().Count);
        }

        public static void ExportTo(this IReadOnlyList<FactionRelation> obj, Dictionary<string, DataNode> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            Dictionary<string, DataNode> relationTable = new();

            for (int i = 0; i < obj.Count; i++)
            {
                Dictionary<string, DataNode> relation = new();

                obj[i].ExportTo(relation);

                relationTable.SetData(i.ToString(), relation);
            }

            data.SetData(RELATIONS, relationTable);
        }
        public static FactionRelation[] CreateFactionRelationsFrom(Dictionary<string, DataNode> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            FactionRelation[] relations = new FactionRelation[data.Count];

            for (int i = 0; i < relations.Length; i++)
            {
                relations[i] = CreateFactionRelationFrom(data.GetData(i.ToString()));
            }

            return relations;
        }

        public static void ExportTo(this FactionRelation obj, Dictionary<string, DataNode> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            data.SetString(ID, obj.ID.Key);
            data.SetFloat(RELATION, obj.Relation);
        }
        public static FactionRelation CreateFactionRelationFrom(Dictionary<string, DataNode> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            string key = data.GetString(ID);
            int index = FactionDatabase.GetIDIndex(key);
            int val = data.GetInt(RELATION);

            return new FactionRelation(new(key, index), val);
        }
    }
}