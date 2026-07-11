using UnityEngine;

namespace Core
{
    public abstract class Registry : ScriptableObject
    {
        /// <summary> Called after scripts recompiled! </summary>
        public abstract void OnInitialize();
        /// <summary> Called before awake! </summary>
        public abstract void OnAwake();
    }
}