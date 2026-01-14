using System;
using System.IO;
using UnityEngine;

namespace Core
{
    using static CoreUtility;

    [Serializable] 
    public sealed class SerializeableFile<T> where T : class, new()
    {
        public T Data { get; private set; }

        private readonly string path = STRING_NULL;
        private bool isInitialized = false;
       
        public SerializeableFile(string fileName) => path = Path.Combine(Application.persistentDataPath, $"{fileName}.json");
        public void Load(bool useInEditor = false)
        {
            if (isInitialized)
            {
                return;
            }

            if (!useInEditor && Application.isEditor)
            {
                return;
            }

            Data = SerializerObject.Load<T>(path, useInEditor);

            if (Data == null)
            {
                Data = new T();
                Save(useInEditor);
            }

            isInitialized = true;
        }
        public void Save(bool useInEditor = false)
        {
            if (!useInEditor && Application.isEditor)
            {
                return;
            }

            SerializerObject.Save(path, Data, useInEditor);
            isInitialized = true;
        }
        public void Clear()
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            isInitialized = false;
        }
    }
}
