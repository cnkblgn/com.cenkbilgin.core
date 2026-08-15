using UnityEngine;

namespace Core
{
    public abstract class Registry : ScriptableObject
    {
        internal int Priority => priority;

        [Header("_")]
        [SerializeField, Min(0)] private int priority = 0;

        public abstract void Reload();
        public abstract void OnAfterScriptLoad();
        public abstract void OnBeforeSceneLoad();
    }
}