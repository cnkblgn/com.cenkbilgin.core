using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Item
{
    public static class ItemUtility
    {
        public static ulong CreateMask(this ItemTag[] tags) => ItemTag.CreateMask(tags);

        public static bool HasAll(this ulong @base, ItemTag target) => @base.HasAll(target.Mask);
        public static bool HasAny(this ulong @base, ItemTag target) => @base.HasAny(target.Mask);

        public static bool HasAll(this ItemTag[] @base, ItemTag[] target) => CreateMask(@base).HasAll(CreateMask(target));
        public static bool HasAny(this ItemTag[] @base, ItemTag[] target) => CreateMask(@base).HasAny(CreateMask(target));


        #region IMPORT / EXPORT

        private const string BASE_ID = "i_bID";
        private const string INSTANCE_ID = "i_iID";
        private const string STACK = "i_stack";
        private const string DATA = "i_data";
        private const string POS = "i_pos";
        private const string ROT = "i_rot";
        private const string WIDTH = "i_width";
        private const string HEIGHT = "i_height";
        private const string WEIGHT = "i_weight";
        private const string ITEMS = "i_items";

        public static void ExportTo(this InventoryEntity obj, Dictionary<string, DataNode> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            obj.ExportTo(out InventoryData inventoryData);

            inventoryData.ExportTo(data);
        }
        public static void ImportFrom(this InventoryEntity obj, Dictionary<string, DataNode> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            obj.ImportFrom(CreateInventoryFrom(data));
        }

        public static void ExportTo(this InventoryData obj, Dictionary<string, DataNode> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            IReadOnlyCollection<Guid> itemList = obj.GetItems();
            Dictionary<string, DataNode> itemTable = new();
            int count = 0;

            foreach (Guid id in itemList)
            {
                Dictionary<string, DataNode> slot = new();

                if (obj.TryGetItemByInstanceID(id, out ItemData registered))
                {
                    registered.ExportTo(slot);
                }

                itemTable.SetData(count.ToString(), slot);

                count++;
            }

            data.SetInt(WIDTH, obj.GridWidth);
            data.SetInt(HEIGHT, obj.GridHeight);
            data.SetInt(WEIGHT, obj.MaximumWeight);
            data.SetData(ITEMS, itemTable);
        }
        public static InventoryData CreateInventoryFrom(Dictionary<string, DataNode> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            int width = data.GetInt(WIDTH);
            int height = data.GetInt(HEIGHT);
            int weight = data.GetInt(WEIGHT);
            Dictionary<string, DataNode> itemTable = data.GetData(ITEMS);

            InventoryData inventory = new(width, height, weight);

            for (int i = 0; i < itemTable.Count; i++)
            {
                Dictionary<string, DataNode> slot = itemTable.GetData(i.ToString());

                ItemData item = CreateItemFrom(slot);

                inventory.TryAddItem(item, item.Position, out ItemData _, out InventoryResult _);
            }

            return inventory;
        }

        public static void ExportTo(this ItemEntity obj, Dictionary<string, DataNode> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            obj.transform.ExportTo(data);
            obj.ExportTo(out ItemData itemData);

            itemData.ExportTo(data);
        }
        public static void ImportFrom(this ItemEntity obj, Dictionary<string, DataNode> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            obj.transform.ImportFrom(data);
            obj.ImportFrom(CreateItemFrom(data));
        }

        public static void ExportTo(this ItemData obj, Dictionary<string, DataNode> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            data.SetString(BASE_ID, obj.BaseID.Key);
            data.SetGuid(INSTANCE_ID, obj.InstanceID);
            data.SetVector2(POS, new(obj.Position.x, obj.Position.y));
            data.SetBool(ROT, obj.IsRotated);
            data.SetInt(STACK, obj.GetStack());
            data.SetData(DATA, obj.Data);
        }
        public static ItemData CreateItemFrom(Dictionary<string, DataNode> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            string id = data.GetString(BASE_ID);

            ItemID baseID = new(id, ItemDatabase.GetIDIndex(id));
            Guid instanceID = data.GetGuid(INSTANCE_ID);
            Vector2Int position = new((int)data.GetVector2(POS).x, (int)data.GetVector2(POS).y);
            bool isRotated = data.GetBool(ROT);
            int stack = data.GetInt(STACK);
            Dictionary<string, DataNode> _data = data.GetData(DATA);

            return new(baseID, instanceID, _data, position, stack, isRotated);
        }

        #endregion
    }
}
