using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace Core
{
    using static CoreUtility;

    [Serializable] 
    public abstract class SerializeableData<T> where T : SerializeableData<T>
    {
        [JsonIgnore] protected string Path => $"{Application.persistentDataPath}/{Name}.json";
        [JsonIgnore] protected abstract string Name { get; }
        [JsonIgnore] protected abstract bool Optimize { get; }

        [Header("_")]
        [SerializeField, JsonRequired] protected string version = "0.1";

        private bool isInitialized = false;

        public bool IsExists() => File.Exists(Path);
        public void Clear(bool useInEditor = false)
        {
            if (!useInEditor && Application.isEditor)
            {
                return;
            }

            SerializerSystem.Clear(Path, useInEditor);
            isInitialized = false;
        }
        public void Save(bool useInEditor = false)
        {
            if (!useInEditor && Application.isEditor)
            {
                return;
            }

            SerializerSystem.Save(Path, this, useInEditor, Optimize);
            isInitialized = true;
        }
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

            if (!IsExists())
            {
                Save(useInEditor);
                return;
            }

            T data = SerializerSystem.Load<T>(Path, useInEditor);

            if (data == null)
            {
                Save(useInEditor);
                Debug.LogError("data == null");
                return;
            }

            if (!string.Equals(data.version, version))
            {
                Debug.LogError($"Version Mismatch! || " + "Path: " + data.Path + " << Path Version: " + data.version + " << Game Version : " + version);

                Save(useInEditor);
                OnLoad(SerializerSystem.Load<T>(Path, useInEditor));
            }
            else
            {
                OnLoad(data);
            }

            isInitialized = true;
        }
        protected abstract void OnLoad(T data);
    }
}
