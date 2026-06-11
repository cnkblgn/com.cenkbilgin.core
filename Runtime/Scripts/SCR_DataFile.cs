using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Core
{
    [Serializable]
    public sealed class SaveFile
    {
        public Dictionary<string, DataNode> Data { get; private set; }

        private readonly string path;

        public SaveFile(string fileName)
        {
            path = Path.Combine(Application.persistentDataPath, $"{fileName}.dat");
            Data = new Dictionary<string, DataNode>();
        }

        public void Load()
        {
            try
            {
                if (!File.Exists(path))
                {
                    Data = new Dictionary<string, DataNode>();
                    Save();
                    return;
                }

                byte[] bytes = File.ReadAllBytes(path);
                Data = DataSerializer.Deserialize(bytes);
            }
            catch (Exception e)
            {
                Debug.LogError($"Load failed! {e}");
                Data = new Dictionary<string, DataNode>();
            }
        }
        public void Save()
        {
            try
            {
                string directory = Path.GetDirectoryName(path);

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                byte[] bytes = DataSerializer.Serialize(Data);

                string tempPath = path + ".tmp";

                File.WriteAllBytes(tempPath, bytes);

                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                File.Move(tempPath, path);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Save failed! {exception}");
            }
        }
        public void Clear()
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                Data.Clear();
            }
            catch (Exception exception)
            {
                Debug.LogError($"Clear failed! {exception}");
            }
        }
    }
}