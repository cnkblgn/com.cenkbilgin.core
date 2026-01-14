using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace Core
{
    using static CoreUtility;

    public static class SerializerObject
    {
        public static bool Clear(string path, bool useInEditor = false)
        {
            if (!useInEditor)
            {
                if (Application.isEditor)
                {
                    return false;
                }
            }

            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }

            return false;
        }
        public static bool Save<T>(string path, T Data, bool useInEditor = false, bool useOptimization = true, ISerializationBinder serializationBinder = null)
        {
            if (!useInEditor)
            {
                if (Application.isEditor)
                {
                    return false;
                }
            }

            try
            {
                if (File.Exists(path))
                {
                    Debug.LogWarning($"SerializerObject.Save() File at {path} is exist. Deleting old file!");
                    File.Delete(path);
                }

                // For custom assembly names: ("$type": "Game.Logic.Runtime.SCR_SectorPersistentDataDefault, ASM_LogicRuntime") -> "$type": "SectorData"
                SerializerSettings.SETTINGS.SerializationBinder = serializationBinder;

                using FileStream fileStream = File.Create(path);
                fileStream.Close();
                File.WriteAllText(path, JsonConvert.SerializeObject(Data, useOptimization ? Formatting.None : Formatting.Indented, SerializerSettings.SETTINGS));
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"SerializerObject.Save() Failed to save data: {exception.Message} {exception.StackTrace}");
                return false;
            }
        }
        public static T Load<T>(string path, bool useInEditor = false, ISerializationBinder serializationBinder = null)
        {
            if (!useInEditor)
            {
                if (Application.isEditor)
                {
                    return default;
                }
            }

            if (!File.Exists(path))
            {
                Debug.LogError($"SerializerObject.Load() File at {path} not exists!");
                return default;
            }

            try
            {
                // For custom assembly names: ("$type": "Game.Logic.Runtime.SCR_SectorPersistentDataDefault, ASM_LogicRuntime") -> "$type": "SectorData"
                SerializerSettings.SETTINGS.SerializationBinder = serializationBinder;

                T data = JsonConvert.DeserializeObject<T>(File.ReadAllText(path), SerializerSettings.SETTINGS);
                return data;
            }
            catch (Exception exception)
            {
                Debug.LogError($"SerializerObject.Load() Failed to load data: {exception.Message} {exception.StackTrace}");
                return default;
            }
        }
    }
}