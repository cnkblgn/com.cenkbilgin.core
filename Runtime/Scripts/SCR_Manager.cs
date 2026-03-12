using System;
using UnityEngine;

namespace Core
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-1)]
    public abstract class Manager<T> : MonoBehaviour where T : Manager<T>
    {        
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogWarning($"Manager() [{typeof(T).Name}] has not been initialized.");
                }

                return instance;
            }
        } private static T instance;

        protected virtual void Awake()
        {
            instance = (T)this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
