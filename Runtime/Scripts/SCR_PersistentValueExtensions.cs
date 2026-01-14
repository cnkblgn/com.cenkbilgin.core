using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public static class PersistentValueExtensions
    {
        public static void SetInt(this Dictionary<string, PersistentValue> data, string key, int value) => data[key] = new PersistentValue { Int = value };
        public static void SetFloat(this Dictionary<string, PersistentValue> data, string key, float value) => data[key] = new PersistentValue { Float = value };
        public static void SetBool(this Dictionary<string, PersistentValue> data, string key, bool value) => data[key] = new PersistentValue { Bool = value };
        public static void SetString(this Dictionary<string, PersistentValue> data, string key, string value) => data[key] = new PersistentValue { String = value };
        public static void SetVector3(this Dictionary<string, PersistentValue> data, string key, Vector3 value) => data[key] = new PersistentValue { Vector3 = value };
        public static void SetVector2(this Dictionary<string, PersistentValue> data, string key, Vector2 value) => data[key] = new PersistentValue { Vector2 = value };
        public static void SetGuid(this Dictionary<string, PersistentValue> data, string key, Guid value) => data[key] = new PersistentValue { Guid = value };
        public static void SetData(this Dictionary<string, PersistentValue> data, string key, Dictionary<string, PersistentValue> value) => data[key] = new PersistentValue { Data = value == null ? new() : new Dictionary<string, PersistentValue>(value) };
        public static int GetInt(this Dictionary<string, PersistentValue> data, string key, int def = 0) => data.TryGetValue(key, out var v) && v.Int.HasValue ? v.Int.Value : def;
        public static float GetFloat(this Dictionary<string, PersistentValue> data, string key, float def = 0f) => data.TryGetValue(key, out var v) && v.Float.HasValue ? v.Float.Value : def;
        public static bool GetBool(this Dictionary<string, PersistentValue> data, string key, bool def = false) => data.TryGetValue(key, out var v) && v.Bool.HasValue ? v.Bool.Value : def;
        public static string GetString(this Dictionary<string, PersistentValue> data, string key, string def = "") => data.TryGetValue(key, out var v) && v.String != null ? v.String : def;
        public static Vector3 GetVector3(this Dictionary<string, PersistentValue> data, string key, Vector3 def = default) => data.TryGetValue(key, out var v) && v.Vector3.HasValue ? v.Vector3.Value : def;
        public static Vector2 GetVector2(this Dictionary<string, PersistentValue> data, string key, Vector2 def = default) => data.TryGetValue(key, out var v) && v.Vector2.HasValue ? v.Vector2.Value : def;
        public static Guid GetGuid(this Dictionary<string, PersistentValue> data, string key, Guid def = default) => data.TryGetValue(key, out var v) && v.Guid.HasValue ? v.Guid.Value : def;
        public static Dictionary<string, PersistentValue> GetData(this Dictionary<string, PersistentValue> data, string key, Dictionary<string, PersistentValue> def = default) => data.TryGetValue(key, out var v) && v.Data != null ? new(v.Data) : def;
    }
}