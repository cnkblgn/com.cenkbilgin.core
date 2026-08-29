using System;
using System.Collections.Generic;
using Core.Actors;

namespace Core.Trait
{
    public static class TraitUtility
    {
        private const string TRAITS = "traits";

        public static void ExportTo(this TraitContainer obj, Dictionary<string, DataNode> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            Dictionary<string, DataNode> traitTable = new();
            IReadOnlyList<TraitInstance> traitList = obj.GetTraits();

            for (int i = 0; i < traitList.Count; i++)
            {
                traitTable.SetString(i.ToString(), traitList[i].ID.Key);
            }

            data.SetData(TRAITS, traitTable);
        }
        public static void ImportFrom(this TraitContainer obj, Dictionary<string, DataNode> data, Actor actor)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            if (actor == null) throw new ArgumentNullException(nameof(actor));

            Dictionary<string, DataNode> traitTable = data.GetData(TRAITS);

            for (int i = 0; i < traitTable.Count; i++)
            {
                string key = traitTable.GetString(i.ToString());
                int index = TraitDatabase.GetIDIndex(key);
                obj.TryAddTrait(new(key, index), actor);
            }
        }
    }
}