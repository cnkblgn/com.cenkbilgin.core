using UnityEngine;

namespace Core
{
    public abstract class Registry : ScriptableObject
    {
        public abstract void OnAfterScriptLoad();
        public abstract void OnBeforeSceneLoad();
    }
}