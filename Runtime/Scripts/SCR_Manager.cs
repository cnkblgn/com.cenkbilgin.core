using System;
using UnityEngine;

namespace Core
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public abstract class Manager<T> : MonoBehaviour where T : Manager<T>
    {        
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    throw new InvalidOperationException($"Manager() [{typeof(T).Name}] has not been initialized.");
                }

                return instance;
            }
        } private static T instance;

        protected virtual void Awake()
        {
            if (instance != null && instance != this)
            {
                Debug.LogError($"Manager.Awake() [{typeof(T).Name}] duplicate found!");
                Destroy(gameObject);
                return;
            }

            instance = (T)this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
