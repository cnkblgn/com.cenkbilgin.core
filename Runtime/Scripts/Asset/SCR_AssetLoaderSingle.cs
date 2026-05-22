using UnityEngine;
using Core;

namespace Game
{
    using static CoreUtility;

    public abstract class AssetLoaderSingle<TDatabase> : MonoBehaviour where TDatabase : ScriptableObject
    {
        public abstract string Path { get; }

        [Header("_")]
        [SerializeField, Required] private TDatabase database;

        private void Awake() => Build(database);
        protected abstract void Build(TDatabase database);

#if UNITY_EDITOR
        private void OnValidate()
        {
            Debug.Log($"Assets Loaded -> {Path}");
            database = Core.Editor.EditorUtility.FindAssetByType<TDatabase>(Path);
        }
#endif
    }
}
