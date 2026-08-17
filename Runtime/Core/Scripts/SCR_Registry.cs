using UnityEngine;

namespace Core
{
    public abstract class Registry : ScriptableObject
    {
        public abstract int Priority { get; }

        [Info("Registry load order.\n" + "Path: " + RegistryLoader.PATH)]
        [SerializeField, ReadOnly] private int priority;

#if UNITY_EDITOR
        protected virtual void OnValidate() => priority = Priority;
#endif

        public abstract void Reload();
        public abstract void OnAfterScriptLoad();
        public abstract void OnAfterAssembliesLoaded();
    }
}