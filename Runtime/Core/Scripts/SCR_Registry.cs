using UnityEngine;

namespace Core
{
    public abstract class Registry : ScriptableObject
    {
        public abstract void OnAssemblyReload();
        public abstract void OnBeforeSceneLoad();
    }
}