using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public static class MaterialUtility
    {
        private const string PREFIX = "Material/";
        private const string DEFAULT = "Untagged";

        private static readonly Dictionary<string, MaterialID> Table = new()
        {
            [DEFAULT] = MaterialID.DEFAULT,
            [PREFIX + "Stone"] = MaterialID.STONE,
            [PREFIX + "Wood"] = MaterialID.WOOD,
            [PREFIX + "Dirt"] = MaterialID.DIRT,
            [PREFIX + "Metal"] = MaterialID.METAL,
            [PREFIX + "Cloth"] = MaterialID.CLOTH,
            [PREFIX + "Flesh"] = MaterialID.FLESH,
        };

        public static MaterialID GetMaterial(this GameObject gameObject) => GetMaterialFromTag(gameObject.tag);

        public static MaterialID GetMaterial(this Collider collider) => GetMaterialFromTag(collider.tag);

        public static string GetTag(this MaterialID id) => GetTagFromMaterial(id);

        private static MaterialID GetMaterialFromTag(string tag)
        {
            if (Table.TryGetValue(tag, out MaterialID id))
            {
                return id;
            }

            return MaterialID.DEFAULT;
        }
        private static string GetTagFromMaterial(MaterialID id)
        {
            foreach (var item in Table)
            {
                if (item.Value == id)
                {
                    return item.Key;
                }
            }

            return DEFAULT;
        }

        public static bool HasAll(this MaterialID baseIDs, MaterialID targetIDs) => (baseIDs & targetIDs) == targetIDs;
        public static bool HasAny(this MaterialID baseIDs, MaterialID targetIDs) => (baseIDs & targetIDs) != 0;
    }
}