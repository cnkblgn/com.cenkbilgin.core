using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Core
{
    [Serializable]
    public sealed class DataFile
    {
        public Dictionary<string, DataNode> Data { get; private set; }

        private readonly string name;

        public DataFile(string name)
        {
            this.name = name;
            Data = new Dictionary<string, DataNode>();
        }

        public void Load(out bool hasPath)
        {
            hasPath = false;

            string path = Path.Combine(Application.persistentDataPath, $"{name}.dat");

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

                hasPath = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Load failed! {e}");
                Data = new Dictionary<string, DataNode>();
            }
        }
        public void Save()
        {
            string path = Path.Combine(Application.persistentDataPath, $"{name}.dat");

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
            string path = Path.Combine(Application.persistentDataPath, $"{name}.dat");

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