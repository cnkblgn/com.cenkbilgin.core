using UnityEngine;

namespace Core.Actors
{
    using static CoreUtility;

    [CreateAssetMenu(fileName = "SCO_ActorRegistry", menuName = "Resources/Core/Actor Registry", order = 10)]
    public sealed class RegistryActor : Registry
    {
        [Header("_")]
        [SerializeField, Required] private string[] ids;
        [SerializeField, Required] private string[] tags;

        public override void OnBeforeSceneLoad() => ActorDatabase.Build(ids, tags);
        public override void OnAssemblyReload() => Generate();

#if UNITY_EDITOR
        [Clickable("Build")]
        public void Generate()
        {
            ActorDatabase.Build(ids, tags);

            GenerateScriptDatabase(this, "Core.Actors", "ActorTag", ActorDatabase.GetTags(), "SCR_GeneratedActorTag.cs", true, true, 1);
            GenerateScriptDatabase(this, "Core.Actors", "ActorID", ActorDatabase.GetIDs(), "SCR_GeneratedActorID.cs", true, false);
        }

        /// <summary> Overrides ids </summary>
        public void OverrideIDs(string[] entries)
        {
            if (entries == null)
            {
                Debug.LogError("Registry override entry cannot be null!");
                return;
            }

            ids = new string[entries.Length];

            for (int i = 0; i < entries.Length; i++)
            {
                ids[i] = entries[i];
            }
        }

        /// <summary> Overrides tags </summary>
        public void OverrideTags(string[] entries)
        {
            if (entries == null)
            {
                Debug.LogError("Registry override entry cannot be null!");
                return;
            }

            tags = new string[entries.Length];

            for (int i = 0; i < entries.Length; i++)
            {
                tags[i] = entries[i];
            }
        }
#endif
    }
}
