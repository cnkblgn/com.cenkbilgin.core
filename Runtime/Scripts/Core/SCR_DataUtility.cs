using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public static class DataUtility
    {
        public static void SetInt(this Dictionary<string, DataNode> data, string key, int value) => data[key] = new DataNode { Type = DataType.INT, Value = new DataValue { Int = value } };
        public static void SetFloat(this Dictionary<string, DataNode> data, string key, float value) => data[key] = new DataNode { Type = DataType.FLOAT, Value = new DataValue { Float = value } };
        public static void SetLong(this Dictionary<string, DataNode> data, string key, long value) => data[key] = new DataNode { Type = DataType.LONG, Value = new DataValue { Long = value } };
        public static void SetBool(this Dictionary<string, DataNode> data, string key, bool value) => data[key] = new DataNode { Type = DataType.BOOL, Value = new DataValue { Bool = value } };
        public static void SetString(this Dictionary<string, DataNode> data, string key, string value) => data[key] = new DataNode { Type = DataType.STRING, Value = new DataValue { String = value } };
        public static void SetVector3(this Dictionary<string, DataNode> data, string key, Vector3 value) => data[key] = new DataNode { Type = DataType.VECTOR3, Value = new DataValue { Vector3 = value } };
        public static void SetVector2(this Dictionary<string, DataNode> data, string key, Vector2 value) => data[key] = new DataNode { Type = DataType.VECTOR2, Value = new DataValue { Vector2 = value } };
        public static void SetGuid(this Dictionary<string, DataNode> data, string key, Guid value) => data[key] = new DataNode { Type = DataType.GUID, Value = new DataValue { Guid = value } };
        public static void SetData(this Dictionary<string, DataNode> data, string key, Dictionary<string, DataNode> value) => data[key] = new DataNode { Type = DataType.DATA, Value = new DataValue { Data = value == null ? new() : new Dictionary<string, DataNode>(value) } };

        public static int GetInt(this Dictionary<string, DataNode> data, string key, int def = 0) => data.TryGetValue(key, out var v) && v.Type == DataType.INT ? v.Value.Int : def;
        public static float GetFloat(this Dictionary<string, DataNode> data, string key, float def = 0f) => data.TryGetValue(key, out var v) && v.Type == DataType.FLOAT ? v.Value.Float : def;
        public static long GetLong(this Dictionary<string, DataNode> data, string key, long def = 0) => data.TryGetValue(key, out var v) && v.Type == DataType.LONG ? v.Value.Long : def;
        public static bool GetBool(this Dictionary<string, DataNode> data, string key, bool def = false) => data.TryGetValue(key, out var v) && v.Type == DataType.BOOL ? v.Value.Bool : def;
        public static string GetString(this Dictionary<string, DataNode> data, string key, string def = "") => data.TryGetValue(key, out var v) && v.Type == DataType.STRING ? v.Value.String : def;
        public static Vector3 GetVector3(this Dictionary<string, DataNode> data, string key, Vector3 def = default) => data.TryGetValue(key, out var v) && v.Type == DataType.VECTOR3 ? v.Value.Vector3 : def;
        public static Vector2 GetVector2(this Dictionary<string, DataNode> data, string key, Vector2 def = default) => data.TryGetValue(key, out var v) && v.Type == DataType.VECTOR2 ? v.Value.Vector2 : def;
        public static Guid GetGuid(this Dictionary<string, DataNode> data, string key, Guid def = default) => data.TryGetValue(key, out var v) && v.Type == DataType.GUID ? v.Value.Guid : def;
        public static Dictionary<string, DataNode> GetData(this Dictionary<string, DataNode> data, string key) => data.TryGetValue(key, out var v) && v.Type == DataType.DATA && v.Value.Data != null ? new(v.Value.Data) : new();
    }
}