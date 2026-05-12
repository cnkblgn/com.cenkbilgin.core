using System;
using UnityEngine;

namespace Core
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-1)]
    public abstract class Manager<T> : MonoBehaviour where T : Manager<T>
    {
        public static bool HasInstance => instance != null;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    throw new InvalidOperationException($"[{typeof(T).Name}] has not been initialized.");
                }

                return instance;
            }
        } private static T instance;

        protected virtual void Awake()
        {
            if (instance != null && instance != this)
            {
                Debug.LogError($"Duplicate manager [{nameof(T)}]");
                Destroy(gameObject);
                return;
            }

            instance = (T)this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
