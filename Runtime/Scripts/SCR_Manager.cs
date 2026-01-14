using UnityEngine;

namespace Core
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public class Manager<T> : MonoBehaviour where T : Component
    {
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<T>();
                }

                return instance;
            }
        } private static T instance = default;

        protected virtual void Awake()
        {
            DontDestroyOnLoad(transform.gameObject);

            if (this != Instance)
            {
                LogError("Manager.Awake() " + typeof(T).Name + " type duplicate is found!");
                Destroy(this.gameObject);
            }
        }
    }
}
