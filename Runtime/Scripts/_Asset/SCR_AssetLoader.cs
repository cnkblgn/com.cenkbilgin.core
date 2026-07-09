using UnityEngine;
using Core;

namespace Game
{
    using static CoreUtility;

    [DefaultExecutionOrder(-1)]
    public abstract class AssetLoader<TDatabase> : MonoBehaviour where TDatabase : ScriptableObject
    {
        public abstract string Path { get; }

        [Header("_")]
        [SerializeField, Required] private TDatabase[] database;

        private void Awake() => Build(database);
        protected abstract void Build(TDatabase[] database);

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (TryFindAssetsByType(out database, Path)) { Debug.Log($"Assets Loaded -> {Path}"); }
        }
#endif
    }
}
